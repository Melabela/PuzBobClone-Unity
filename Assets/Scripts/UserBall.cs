using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserBall : MonoBehaviour
{
    public float SPEED_MULT = 6.0f;
    public float DIAMETER = 1.0f;
    
    // absolute bounds, edges, not centers
    public float X_MIN = 0f;
    public float X_MAX = 8f;
    public float Y_MIN = 0f;
    public float Y_MAX = 1 + (10 * 0.8660f);

    // actual bounds, accounting for diameter to center
    float centerXMin;
    float centerXMax;
    float centerYMin;
    float centerYMax;

    // Start is called before the first frame update
    void Start()
    {
        float radius = DIAMETER / 2;
        centerXMin = X_MIN + radius;
        centerXMax = X_MAX - radius;
        centerYMin = Y_MIN + radius;
        centerYMax = Y_MAX - radius;        
    }

    // Update is called once per frame
    void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        if (horizontalInput != 0) {
            float newX = transform.position.x
                         + (horizontalInput * Time.deltaTime * SPEED_MULT);
            // keep in bounds
            if (newX < centerXMin) {
                newX = centerXMin;
            }
            if (newX > centerXMax) {
                newX = centerXMax;
            }
            Vector3 tr_pos = transform.position;
            transform.position = new Vector3(newX, tr_pos.y, tr_pos.z);
        }

        float verticalInput = Input.GetAxis("Vertical");
        if (verticalInput != 0) {
            float newY = transform.position.y
                         + (verticalInput * Time.deltaTime * SPEED_MULT);
            // keep in bounds
            if (newY < centerYMin) {
                newY = centerYMin;
            }
            if (newY > centerYMax) {
                newY = centerYMax;
            }
            Vector3 tr_pos = transform.position;
            transform.position = new Vector3(tr_pos.x, newY, tr_pos.z);
        }
    }
}
