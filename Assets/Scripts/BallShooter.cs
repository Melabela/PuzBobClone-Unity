using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallShooter : MonoBehaviour
{
    [SerializeField] GameObject ballPrefab;

    // rotation bound
    [SerializeField] float ROTATION_SPEED_MULT = 60.0f;
    [SerializeField] float Z_ROT_MIN = -86;
    [SerializeField] float Z_ROT_MAX = +86;

    [SerializeField] float BallShootForce = 12.0f;

    // local state
    bool isHoldingBall;  // dropper has ball attached
    bool isBallDropped;  // ball released, waiting
    int ballIndex;
    GameObject ballBeingHeld;
    Transform transformArrowRef;  // gameobject that's on the arrow pointer

    // Start is called before the first frame update
    void Start()
    {
        // initial ball state
        // Update() will populate ball
        isHoldingBall = false;
        isBallDropped = false;
        ballIndex = 0;
        ballBeingHeld = null;

        transformArrowRef = transform.Find("Shooter_ArrowRef");
    }

    // Update is called once per frame
    void Update()
    {
        RotateSelf();
        CheckForDrop();
    }

    // called from outside.  let's us know previous ball dropped
    //  has settled, and we can create another in the dropper
    public void BallDropDone()
    {
        // Debug.Log("ENTER BallDropper.BallDropDone");
        if (isBallDropped) {
            isBallDropped = false;
        }
    }

    void RotateSelf()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        if (horizontalInput != 0) {
            float rotAmountZ = horizontalInput * Time.deltaTime * ROTATION_SPEED_MULT;
            transform.Rotate(0, 0, rotAmountZ);

            float myRotZ = transform.eulerAngles.z;
            // angles are read out as [0.0, 360.0)
            // need adjust high numbers, to continue w/ negative value calc
            if (myRotZ >= 180) {
                myRotZ -= 360;
            }
            // Debug.Log($"BallShooter.RotateSelf() - myRotZ={myRotZ}");

            // keep in bounds
            if (myRotZ < Z_ROT_MIN) {
                // E.G. above values = -89 & -86
                // WANT:  rotate +3 (+ -89) -> -86
                // do a reverse rotation to keep at MIN
                transform.Rotate(0, 0, -(myRotZ - Z_ROT_MIN));
            }
            else if (myRotZ > Z_ROT_MAX) {
                // E.G. above values = +89 & +86
                // WANT:  rotate -3 (+ 89) -> 86
                // do a reverse rotation to keep at MAX
                transform.Rotate(0, 0, -(myRotZ - Z_ROT_MAX));
            }
        }

        if (ballBeingHeld) {
            // move ball held horizontally as well
            ballBeingHeld.transform.position = transform.position;
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
                ShootBall();
                ballBeingHeld = null;
            }
        }
    }

    void GenerateBall()
    {
        // Debug.Log("ENTER BallDropper.GenerateBall()");
        var newBall = Instantiate(ballPrefab,
                        transform.position,
                        ballPrefab.transform.rotation).gameObject;
        ballIndex += 1;
        newBall.name += $"_{ballIndex}";  // append index to ball name for identification
        ballBeingHeld = newBall;
    }

    void ShootBall()
    {
        // Debug.Log("ENTER BallDropper.ShootBall()");
        Rigidbody ballHeldRb = ballBeingHeld.GetComponent<Rigidbody>();
        ballHeldRb.velocity = Vector3.zero;  // clear first

        // determine angle
        Vector3 arrowDir = transformArrowRef.position - transform.position;
        // Debug.Log($"ShootBall() - arrowDir={arrowDir}");
        ballHeldRb.AddForce(Vector3.Normalize(arrowDir) * BallShootForce, ForceMode.Impulse);
    }
}
