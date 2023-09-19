using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallInfo : MonoBehaviour
{
    // ID zero (0) intentionally NOT used, keep that to identify empty spot
    static readonly public int BALL_MIN_ID = 1;  // inclusive
    static readonly public int BALL_MAX_ID = 6;  // inclusive

    static readonly float BALL_COLLIDE_MAX_DIFF_FROM_FORWARD_ANGLE = 60f;

    static readonly List<string> ballIndexToColorName = new List<string>()
    {
        "",       // [0]
        "red",    // [1]
        "orange", // [2]
        "yellow", // [3]
        "green",  // [4]
        "blue",   // [5]
        "purple"  // [6]
    };

    static readonly Dictionary<string, string> ballColorNameToMaterialName = new Dictionary<string, string>()
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
        gridPosScript = GameObject.Find("Grid").GetComponent<GridPositions>();

        myRb = GetComponent<Rigidbody>();

        int ballId = ChooseId();
        // Debug.Log($"BallInfo.Start() - ballId={ballId}");
        UpdateColor(ballId);

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
                // Debug.Log($"FixedUpdate(), w/ bDetectCollision - myRb.velocity.y={myRb.velocity.y}");
                StopActiveBall();
            }
        }
    }

    public int GetId()
    {
        return myId;
    }

    int ChooseId()
    {
        bool[] allowedBallIds = gridPosScript.GetAllowedBallIds();
        int randId;

        do {
            randId = Random.Range(BALL_MIN_ID, BALL_MAX_ID + 1);
        } while (!allowedBallIds[randId]);

        return randId;
    }

    void UpdateColor(int colorId)
    {
        if ((colorId >= BALL_MIN_ID) && (colorId <= BALL_MAX_ID))
        {
            // active state
            myId = colorId;
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
            Vector3 ballCollidedWithDir = otherObj.transform.position - transform.position;
            // Debug.Log($"OnCollisionEnter(), w/ PlayedBall - ballGoFwdDir={ballGoFwdDir}, ballCollidedWithDir={ballCollidedWithDir}");

            float diffAngle = Vector3.Angle(ballGoFwdDir, ballCollidedWithDir);
            // Debug.Log($"OnCollisionEnter(), w/ PlayedBall - diffAngle={diffAngle}");
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
            var bOverTop = SnapBallToPositionInGrid();
            if (!bOverTop) {
                var bCleared = MarkBallAndCheckPop();
                if (!bCleared) {
                    // load new ball into shooter
                    NotifyBallPlayed();
                } else {
                    // TODO: add more here!
                    Debug.LogWarning("StopActiveBall: bCleared - game cleared!");
                }
            } else {
                // TODO: add more here!
                Debug.LogWarning("StopActiveBall: bOverTop - game end!");
            }
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

    bool SnapBallToPositionInGrid()
    {
        var rawGridPos = gridPosScript.GetClosestPositionForCenterCoordAbsolute(transform.position);
        bool bOverTop = rawGridPos.y >= gridPosScript.GRID_ROWS;

        if (bOverTop) {
            ballGridPos = rawGridPos;
        } else {
            // call again, and get position ensured in grid
            ballGridPos = gridPosScript.GetClosestPositionForCenterCoordSafe(transform.position);
        }

        Vector3 gridCoordForBall = gridPosScript.GetCenterCoordForPosition(ballGridPos);
        Debug.Log($"SnapBallToPositionInGrid: bOverTop={bOverTop}, gridPos={ballGridPos}, posSnapTo={gridCoordForBall}");
        transform.position = gridCoordForBall;

        return bOverTop;
    }

    bool MarkBallAndCheckPop()
    {
        // also mark ball in grid
        gridPosScript.MarkBallInGrid(ballGridPos, myId, gameObject);
        // and pop balls if needed
        gridPosScript.CheckAndPopBalls(myId, ballGridPos);

        // is field cleared, after (possibly) popping balls
        return gridPosScript.GetBallCountInGrid() == 0;
    }

    void NotifyBallPlayed()
    {
        // notify shooter
        BallShooter ballShooterScript = ballShooterObj.GetComponent<BallShooter>();
        ballShooterScript.BallShotDone();
    }

}
