using System;  // for Math
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallShooter : MonoBehaviour
{
    [SerializeField] GameObject ballPrefab;

    // rotation bound
    [SerializeField] float ROTATION_SPEED_MULT = 60.0f;
    [SerializeField] float Z_ROT_MIN = -82;
    [SerializeField] float Z_ROT_MAX = +82;

    [SerializeField] float BallShootForce = 12.0f;

    // local state
    bool isHoldingBall;  // shooter has ball attached
    bool isBallShot;     // ball released, waiting
    int ballIndex;
    GameObject ballBeingHeld;
    Transform transformArrowRef;  // gameobject that's on the arrow pointer
    GameObject shooterGuide;
    GameObject shooterGuide2;

    // Start is called before the first frame update
    void Start()
    {
        // initial ball state
        // Update() will populate ball
        isHoldingBall = false;
        isBallShot = false;
        ballIndex = 0;
        ballBeingHeld = null;

        transformArrowRef = transform.Find("Shooter_ArrowRef");
        shooterGuide = GameObject.Find("Shooter_Guide");
        shooterGuide2 = GameObject.Find("Shooter_Guide_2");
        UpdateShotGuide();
    }

    // Update is called once per frame
    void Update()
    {
        RotateSelf();
        CheckForShot();
    }

    // called from outside.  informs us that previous ball shot
    //  has settled, and we can create another ball in the shooter
    public void BallShotDone()
    {
        // Debug.Log("ENTER BallShooter.BallShotDone");
        if (isBallShot) {
            isBallShot = false;
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
    float GetZRotateAngle(Vector3 ptFrom, Vector3 ptTo)
    {
        float xDiff = ptFrom.x - ptTo.x;
        float yDiff = ptFrom.y - ptTo.y;

        double atanAngle = Math.Atan2(yDiff, xDiff) * (180 / Math.PI);
        return (float)atanAngle;
    }

    void UpdateShotGuide()
    {
        Vector3 arrowRefPos = transformArrowRef.position;  // nickname, as we use often
        Vector3 vArrowDir = arrowRefPos - transform.position;

        RaycastHit hitInfo;
        bool bDidRayHit = Physics.Raycast(arrowRefPos, vArrowDir, out hitInfo, 100);
        if (bDidRayHit){
            // Debug.Log($"UpdateShotGuide - RayCast hitInfo.distance={hitInfo.distance}, hitObject.name={hitInfo.collider.gameObject.name}");
            // Debug.Log($"UpdateShotGuide - RayCast transformArrowRef.pos={transformArrowRef.position}, hitInfo.point={hitInfo.point}");

            // update shooter guide
            Vector3 hitPoint = hitInfo.point;  // nickname, as we use often
            Vector3 midPoint = (arrowRefPos + hitPoint) /2;
            shooterGuide.transform.position = midPoint;

            Vector3 sg_localScale = shooterGuide.transform.localScale;
            shooterGuide.transform.localScale = new Vector3(
                    hitInfo.distance, sg_localScale.y, sg_localScale.z);

            float zAngle = GetZRotateAngle(arrowRefPos, hitPoint);
            // Debug.Log($"UpdateShotGuide - RayCast midPoint={midPoint}, zAngle={zAngle}");
            shooterGuide.transform.eulerAngles = new Vector3(0, 0, zAngle);

            // if first bounce is to side-wall,
            //  give a second guide, to show post-bounce direction
            GameObject rayHitObj = hitInfo.collider.gameObject;
            if (rayHitObj.CompareTag("SideWallRaycastDetect")) {
                shooterGuide2.SetActive(true);
                UpdateShotGuide2nd(arrowRefPos, hitPoint);
            } else {
                shooterGuide2.SetActive(false);
            }
        }
    }

    void UpdateShotGuide2nd(Vector3 arrowRefPos, Vector3 sideWallHitPoint)
    {
        Vector3 reflectNormal = (sideWallHitPoint.x < arrowRefPos.x) ? Vector3.right : Vector3.left;
        Vector3 beforeBounceDir = (sideWallHitPoint - arrowRefPos).normalized;
        Vector3 afterBounceDir = Vector3.Reflect(beforeBounceDir, reflectNormal);

        // RayCast from point where would bounce on wall
        RaycastHit hitInfo;
        bool bDidRayHit = Physics.Raycast(sideWallHitPoint, afterBounceDir, out hitInfo, 100);
        if (bDidRayHit){
            // update 2nd shooter (after first side-wall bounce) guide
            Vector3 midPoint = (sideWallHitPoint + hitInfo.point) / 2;
            shooterGuide2.transform.position = midPoint;

            Vector3 sg_localScale = shooterGuide2.transform.localScale;
            shooterGuide2.transform.localScale = new Vector3(
                    hitInfo.distance, sg_localScale.y, sg_localScale.z);

            float zAngle = GetZRotateAngle(sideWallHitPoint, hitInfo.point);
            // Debug.Log($"UpdateShotGuide2nd - RayCast midPoint={midPoint}, zAngle={zAngle}");
            shooterGuide2.transform.eulerAngles = new Vector3(0, 0, zAngle);
        }
    }

    void CheckForShot()
    {
        if (isBallShot) {
            // waiting... nothing to do, return early!
            return;
        }

        // one frame where !isBallShot && !isHoldingBall
        if (!isHoldingBall) {
            // generate a new ball to hold
            GenerateBall();
            isHoldingBall = true;
            return;
        }
        else {  // isHoldingBall == true
            // ready to shoot ball, on Space Bar press
            if (Input.GetButton("Jump")) {
                isHoldingBall = false;
                isBallShot = true;
                ShootBall();
                ballBeingHeld = null;
                return;
            }
        }
    }

    void GenerateBall()
    {
        // Debug.Log("ENTER BallShooter.GenerateBall()");
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
        // Debug.Log("ENTER BallShooter.ShootBall()");
        Rigidbody ballHeldRb = ballBeingHeld.GetComponent<Rigidbody>();
        ballHeldRb.velocity = Vector3.zero;  // clear first

        // determine angle
        Vector3 arrowDir = transformArrowRef.position - transform.position;
        // Debug.Log($"ShootBall() - arrowDir={arrowDir}");
        ballHeldRb.AddForce(Vector3.Normalize(arrowDir) * BallShootForce, ForceMode.Impulse);
    }
}
