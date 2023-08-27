using System;  // for Math
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
    GameObject shooterGuide;

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
        shooterGuide = GameObject.Find("Shooter_Guide");
        UpdateShotGuide();
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
        UpdateShotGuide();
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

            UpdateShotGuide();
        }
    }

    // rotation in z-axis, for XY rotation, between two points (on XY plane)
    float GetCorrectZRotAngle(Vector3 ptFrom, Vector3 ptTo)
    {
        float xDiff = ptFrom.x - ptTo.x;
        float yDiff = ptFrom.y - ptTo.y;

        double atanAngle = Math.Atan2(yDiff, xDiff) * (180 / Math.PI);
        return (float)atanAngle;
    }

    void UpdateShotGuide()
    {
        Vector3 vOrigin = transformArrowRef.position;
        Vector3 vArrowDir = transformArrowRef.position - transform.position;

        RaycastHit hitInfo;
        bool bDidRayHit = Physics.Raycast(vOrigin, vArrowDir, out hitInfo, 100);
        if (bDidRayHit){
            // Debug.Log($"UpdateShotGuide - RayCast hitInfo.distance={hitInfo.distance}, hitObject.name={hitInfo.collider.gameObject.name}");
            // Debug.Log($"UpdateShotGuide - RayCast transformArrowRef.pos={transformArrowRef.position}, hitInfo.point={hitInfo.point}");

            // update shooter guide, position, etc.
            Vector3 midPoint = (transformArrowRef.position + hitInfo.point) / 2;
            shooterGuide.transform.position = midPoint;

            Vector3 sg_localScale = shooterGuide.transform.localScale;
            shooterGuide.transform.localScale = new Vector3(
                    hitInfo.distance, sg_localScale.y, sg_localScale.z);

            float zAngle = GetCorrectZRotAngle(transformArrowRef.position, hitInfo.point);
            // Debug.Log($"UpdateShotGuide - RayCast midPoint={midPoint}, zAngle={zAngle}");
            shooterGuide.transform.eulerAngles = new Vector3(0, 0, zAngle);
        }
        // Debug.DrawRay();
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
        else {  // isHoldingBall == true
            // ready to drop ball, on Space Bar press
            if (Input.GetButton("Jump")) {
                isHoldingBall = false;
                isBallDropped = true;
                ShootBall();
                ballBeingHeld = null;
                return;
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

        // clear motion on new ball
        Rigidbody ballHeldRb = ballBeingHeld.GetComponent<Rigidbody>();
        ballHeldRb.velocity = Vector3.zero;  // clear first
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
