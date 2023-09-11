using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallInfo : MonoBehaviour
{
    static int BALL_MIN_ID = 1;  // inclusive
    static int BALL_MAX_ID = 6;  // inclusive

    static float BALL_COLLIDE_MAX_DIFF_FROM_FORWARD_ANGLE = 60f;

    static List<string> ballIndexToColorName = new List<string>()
    {
        "",       // [0]
        "red",    // [1]
        "orange", // [2]
        "yellow", // [3]
        "green",  // [4]
        "blue",   // [5]
        "purple"  // [6]
    };

    static Dictionary<string, string> ballColorNameToMaterialName = new Dictionary<string, string>()
    {
        ["red"]    = "Materials/BallRed",
        ["orange"] = "Materials/BallOrange",
        ["yellow"] = "Materials/BallYellow",
        ["green"]  = "Materials/BallGreen",
        ["blue"]   = "Materials/BallBlue",
        ["purple"] = "Materials/BallPurple"
    };

    // other object references
    GameObject ballShooterObj;
    GridPositions gridPosScript;

    // internal vars / state
    Rigidbody myRb;
    int myId;
    bool bDetectCollision;
    Vector3 lastVelocity;
    Vector2Int ballGridPos;

    // Start is called before the first frame update
    void Start()
    {
        ballShooterObj = GameObject.Find("BallShooter");
        GameObject gridObj = GameObject.Find("Grid");
        gridPosScript = gridObj.GetComponent<GridPositions>();

        myRb = GetComponent<Rigidbody>();

        myId = Random.Range(BALL_MIN_ID, BALL_MAX_ID + 1);
        // Debug.Log($"BallInfo.Start() - myId={myId}");
        UpdateColor();

        bDetectCollision = true;
        tag = "ActiveBall";
    }

    // Update is called once per frame
    void Update()
    {
    }

    // FixedUpdate is called every physics step
    // REF: https://gamedev.stackexchange.com/a/197366
    void FixedUpdate()
    {
        // need PREVIOUS velocity, since within OnCollisionEnter()
        // collision has already occurred, and lost previous motion
        if (bDetectCollision) {
            lastVelocity = myRb.velocity;

            // 3. IF any-time, motion incorrectly goes upwards
            if (myRb.velocity.y > 0) {
                Debug.Log($"FixedUpdate(), w/ bDetectCollision - myRb.velocity.y={myRb.velocity.y}");
                StopActiveBall();
            }
        }
    }

    public int GetId()
    {
        return myId;
    }

    void UpdateColor()
    {
        if ((myId >= BALL_MIN_ID) && (myId <= BALL_MAX_ID))
        {
            // active state
            var color_name = ballIndexToColorName[myId];
            var material_name = ballColorNameToMaterialName[color_name];
            var color_material = Resources.Load<Material>(material_name);

            // update material
            Renderer ballRenderer = GetComponent<Renderer>();
            ballRenderer.material = color_material;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // if not detecting, return early
        if (!bDetectCollision) {
            return;
        }

        GameObject otherObj = collision.gameObject;
        // Debug.Log($"ENTER BallInfo.OnCollisionEnter() - bDetectCollision={bDetectCollision}, otherObjTag={otherObjTag}");

        // if hit side wall, reflect motion on x-axis, "bounce" the other way
        if (otherObj.CompareTag("SideWall")) {
            // NOTE1:  Cannot use current myRb.velocity, has already impacted and lost previous travel
            // NOTE2:  keep checking collisions after wall-bounce

            // Debug.Log($"SideWall collision: lastVelo={lastVelocity}");
            Vector3 reflectNormal = (lastVelocity.x < 0) ? Vector3.right : Vector3.left;
            Vector3 newVelocity = Vector3.Reflect(lastVelocity, reflectNormal);
            myRb.velocity = newVelocity;
            // Debug.Log($"SideWall collision: reflNormal={reflectNormal}, newVelo={newVelocity}");
            return;
        }

        bool bStopThisBall = false;

        if (otherObj.CompareTag("BottomWall")) {
            bStopThisBall = true;
        }
        else if (otherObj.CompareTag("PlayedBall")) {
            // 1. avoid immediate snapping, e.g. if contacted ball is "to the side"
            // - check relative position of collider ball
            // - if angle too far from current "forward", then ignore
            // - (if there IS ball forward, that should trigger separate call to OnCollisionEnter())
            Vector3 ballGoFwdDir = lastVelocity;
            Debug.Log($"OnCollisionEnter(), w/ PlayedBall - lastVelocity={lastVelocity}");

            Vector3 ballCollidedWithDir = otherObj.transform.position - transform.position;
            float diffAngle = Vector3.Angle(ballGoFwdDir, ballCollidedWithDir);
            Debug.Log($"OnCollisionEnter(), w/ PlayedBall - diffAngle={diffAngle}");
            if (diffAngle <= BALL_COLLIDE_MAX_DIFF_FROM_FORWARD_ANGLE) {
                // Vector3.Angle() returns smallest positive angle
                // so this should account for +/- 60deg, from forward-dir
                bStopThisBall = true;
            }
        }

        // only stop on contact w/ objects of above two types
        if (bStopThisBall) {
            StopActiveBall();
        }
    }

    void StopActiveBall()
    {
        if (CompareTag("ActiveBall")) {
            StopBallMovement();
            SnapBallToPositionInGrid();
            NotifyBallPlayed();
        }
    }

    void StopBallMovement()
    {
        bDetectCollision = false;
        // update tag, to become detector to future ball shots
        tag = "PlayedBall";
        // stop movement & forces
        myRb.velocity = Vector3.zero;
        myRb.constraints |= RigidbodyConstraints.FreezePosition;
        // change RigidBody to kinematic, removes it from future physics-based movement (i.e. collisions)
        // https://docs.unity3d.com/2021.3/Documentation/Manual/RigidbodiesOverview.html
        myRb.isKinematic = true;
    }

    void SnapBallToPositionInGrid()
    {
        // snap position to closest in grid
        ballGridPos = gridPosScript.GetClosestPositionForCenterCoord(transform.position);
        Vector3 gridCoordForBall = gridPosScript.GetCenterCoordForPosition(ballGridPos);
        Debug.Log($"Snap Ball to grid: posAt={transform.position}, posSnapTo={gridCoordForBall}, gridPos={ballGridPos}");
        transform.position = gridCoordForBall;
    }

    void NotifyBallPlayed()
    {
        // notify shooter
        BallShooter ballShooterScript = ballShooterObj.GetComponent<BallShooter>();
        ballShooterScript.BallShotDone();

        // also mark ball in grid
        gridPosScript.MarkBallInGrid(ballGridPos, myId, gameObject);
        // and pop balls if needed
        gridPosScript.CheckAndPopBalls(myId, ballGridPos);
    }

}
