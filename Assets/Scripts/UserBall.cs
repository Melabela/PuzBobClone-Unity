using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserBall : MonoBehaviour
{
    [SerializeField] GameObject gridGameObj;
    private GridPositions gridScript;

    [SerializeField] float SPEED_MULT = 6.0f;
    [SerializeField] float DIAMETER = 1.0f;

    // absolute bounds, edges, not centers
    [SerializeField] float X_MIN = 0f;
    [SerializeField] float X_MAX = 8f;
    [SerializeField] float Y_MIN = 0f;
    [SerializeField] float Y_MAX = 1 + (10 * 0.8660f);

    // actual bounds, accounting for diameter to center
    private float centerXMin;
    private float centerXMax;
    private float centerYMin;
    private float centerYMax;

    // last vars for highlighting
    private Vector2Int lastHighlightPos;
    private BallInfo lastHighlightBall;

    // Start is called before the first frame update
    void Start()
    {
        gridScript = gridGameObj.GetComponent<GridPositions>();

        float radius = DIAMETER / 2;
        centerXMin = X_MIN + radius;
        centerXMax = X_MAX - radius;
        centerYMin = Y_MIN + radius;
        centerYMax = Y_MAX - radius;
    }

    // Update is called once per frame
    void Update()
    {
        MoveSelf();
        UpdateNearestHighlight();
    }

    void MoveSelf()
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
            Vector3 my_pos = transform.position;
            transform.position = new Vector3(newX, my_pos.y, my_pos.z);
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
            Vector3 my_pos = transform.position;
            transform.position = new Vector3(my_pos.x, newY, my_pos.z);
        }
    }

    void UpdateNearestHighlight()
    {
        Vector3 my_coord = transform.position;
        Vector2Int nearestPos = gridScript.GetClosestPositionForCenterCoord(my_coord);
        // Debug.Log($"my_coord = {my_coord}");
        // Debug.Log($"nearestPos = {nearestPos}");

        if ( (nearestPos != lastHighlightPos) || (lastHighlightPos == null) )
        {
            lastHighlightPos = nearestPos;  // update last Pos

            string ballName = $"Ball(Clone)_{nearestPos.x},{nearestPos.y}";
            Debug.Log($"ballName = {ballName}");
            GameObject nearestBallGameObj = GameObject.Find(ballName);
            BallInfo nearestBall = nearestBallGameObj.GetComponent<BallInfo>();

            if (lastHighlightBall) {
                // may be null, esp. on first time here
                lastHighlightBall.SetHighlight(false);
            }
            nearestBall.SetHighlight(true);
            lastHighlightBall = nearestBall;  // update last Ball
        }
    }

}
