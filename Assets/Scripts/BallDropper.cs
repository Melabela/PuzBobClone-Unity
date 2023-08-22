using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallDropper : MonoBehaviour
{
    [SerializeField] GameObject ballPrefab;

    [SerializeField] float HORIZ_SPEED_MULT = 6.0f;

    // bounds, based on center coords
    [SerializeField] float X_MIN = 0 + 0.5f;
    [SerializeField] float X_MAX = 8 - 0.5f;

    [SerializeField] float Ball_Drop_Force = 6.0f;

    // local state
    bool isHoldingBall;  // dropper has ball attached
    bool isBallDropped;  // ball released, waiting
    int ballIndex;
    GameObject ballBeingHeld;

    // Start is called before the first frame update
    void Start()
    {
        // initial ball state
        // Update() will populate ball
        isHoldingBall = false;
        isBallDropped = false;
        ballIndex = 0;
        ballBeingHeld = null;
    }

    // Update is called once per frame
    void Update()
    {
        MoveSelfHoriz();
        CheckForDrop();
    }

    // called from outside.  let's us know previous ball dropped
    //  has settled, and we can create another in the dropper
    public void BallDropOver()
    {
        if (isBallDropped) {
            isBallDropped = false;
        }
    }

    void MoveSelfHoriz()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        if (horizontalInput != 0) {
            float newX = transform.position.x
                         + (horizontalInput * Time.deltaTime * HORIZ_SPEED_MULT);
            // keep in bounds
            if (newX < X_MIN) {
                newX = X_MIN;
            }
            if (newX > X_MAX) {
                newX = X_MAX;
            }
            Vector3 my_pos = transform.position;
            transform.position = new Vector3(newX, my_pos.y, my_pos.z);
        }

        if (ballBeingHeld) {
            // move ball held horizontally as well
            ballBeingHeld.transform.position = this.transform.position;
        }
    }

    void CheckForDrop()
    {
        if (isBallDropped) {
            // waiting... nothing to do, return early!
            return;
        }

        // one frame where !isBallDropped && !isHoldingBall
        if (!isHoldingBall) {
            // generate a new ball to hold
            GenerateBall();
            isHoldingBall = true;
            return;
        }

        if (isHoldingBall) {
            // ready to drop ball, on Space Bar press
            if (Input.GetButton("Jump")) {
                isHoldingBall = false;
                isBallDropped = true;
                DropBall();
                ballBeingHeld = null;
            }
        }
    }

    void GenerateBall()
    {
        Debug.Log("ENTER BallDropper.GenerateBall()");
        var newBall = Instantiate(ballPrefab,
                        this.transform.position,
                        ballPrefab.transform.rotation).gameObject;
        ballIndex += 1;
        newBall.name += $"_{ballIndex}";  // append index to ball name for identification
        ballBeingHeld = newBall;

        BallInfo newBallScript = newBall.GetComponent<BallInfo>();
        int newBallId = Random.Range(BallInfo.BALL_MIN_ID, BallInfo.BALL_MAX_ID + 1);
        newBallScript.SetId(newBallId);
    }

    void DropBall()
    {
        Debug.Log("ENTER BallDropper.DropBall()");
        Rigidbody ballHeldRb = ballBeingHeld.GetComponent<Rigidbody>();
        ballHeldRb.AddForce(Vector3.down * Ball_Drop_Force, ForceMode.Acceleration);
    }
}
