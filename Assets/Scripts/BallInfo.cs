using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallInfo : MonoBehaviour
{
    static int BALL_MIN_ID = 1;  // inclusive
    static int BALL_MAX_ID = 6;  // inclusive

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

    public int GetId()
    {
        return myId;
    }

    // FixedUpdate is called every physics step
    // REF: https://gamedev.stackexchange.com/a/197366
    void FixedUpdate()
    {
        // need PREVIOUS velocity, since within OnCollisionEnter()
        // collision has already occurred, and lost previous motion
        if (bDetectCollision) {
            lastVelocity = myRb.velocity;
        }
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
        }

        // only stop on contact w/ objects of these two types
        if (otherObj.CompareTag("BottomWall") || otherObj.CompareTag("PlayedBall")) {
            bDetectCollision = false;
            // update tag, to become detector to future ball drops
            tag = "PlayedBall";
            // stop movement & forces
            myRb.velocity = Vector3.zero;
            myRb.constraints |= RigidbodyConstraints.FreezePosition;

            // snap position to closest in grid
            Vector2Int gridPos = gridPosScript.GetClosestPositionForCenterCoord(transform.position);
            Vector3 gridCoordForBall = gridPosScript.GetCenterCoordForPosition(gridPos);
            Debug.Log($"Snap Ball to grid: posAt={transform.position}, posSnapTo={gridCoordForBall}, gridPos={gridPos}");
            transform.position = gridCoordForBall;

            // notify dropper
            BallShooter ballShooterScript = ballShooterObj.GetComponent<BallShooter>();
            ballShooterScript.BallDropDone();

            // also mark ball in grid
            gridPosScript.MarkBallInGrid(gridPos, myId, gameObject);
        }
    }
}
