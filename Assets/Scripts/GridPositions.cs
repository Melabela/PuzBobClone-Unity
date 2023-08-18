using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

/****
 * Positions Example
 * |x x x x x| (0, 4),  (2, 4),  (4, 4), ...
 * | x x x x |     (1, 3),  (3, 3),  (5, 3), ...
 * |x x x x x| (0, 2),  (2, 2),  (4, 2), ...
 * | x x x x |     (1, 1),  (3, 1),  (5, 1), ...
 * |x x x x x| (0, 0),  (2, 0),  (4, 0), ...
 *
 * Notes
 * - So even rows (y), will use even horiz positions (x)
 ****/
 
public class GridPositions : MonoBehaviour
{
    public int GRID_ROWS;
    public int GRID_COLS;
    public float GRID_UNIT_SIZE;  // diameter
    public float Z_COORD = 0.0f;  // set constant
    public GameObject ballPrefab;

    private float half_unit;    // radius
    private float height_unit;  // "orange stacking" rows

    // Start is called before the first frame update
    void Start()
    {
        GridInit();
        FillGridWithBalls();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void GridInit()
    {
        half_unit = GRID_UNIT_SIZE / 2.0f;

        // Pythagoras: a^2 + b^2 = c^2
        //   b = 0.5;  c = 1.0;  _(for unit grid)_
        //   a = sqrt( 1.0^2 - 0.5^2 ) = sqrt( 0.75 ) = 0.8660
        height_unit = (float) Math.Sqrt(
            Math.Pow(GRID_UNIT_SIZE, 2) - Math.Pow(half_unit, 2)
            );
    }

    private Vector3 GetCenterCoordForPosition(Vector2Int pos)
    {
        // X-coord:
        // - odd rows
        //   - pos =   1,   3,   5, ...
        //   -   x = 1.0, 2.0, 3.0, ...
        // - even rows
        //   - pos =   0,   2,   4, ...
        //   -   x = 0.5, 1.5, 2.5, ...
        float cX = (pos.x + 1) * half_unit;

        // Y-coord:
        // - Row 0:  0.5
        // - Row 1:  0.5 + height
        // - Row 2:  0.5 + height*2
        // -  etc.
        float cY = half_unit + (pos.y * height_unit);

        Vector3 coords = new Vector3(cX, cY, Z_COORD);
        return coords;
    }

    private Vector2Int GetClosestPositionForCenterCoord(Vector3 coord)
    {
        // ignore Z-coord.

        // reverse above calculations
        float pX = (coord.x / half_unit) - 1;
        float pY = (coord.y - half_unit) / height_unit;

        // round to nearest integer position
        int ipY = (int)Math.Round(pY);
        int ipX;
        float scaleDownX;

        // depending on row, X-pos needs to align odd or even!
        if ((ipY & 0x1) == 1) {
            // -- odd row
            // -- (e.g. pX = 6.4: -> 5.4 -> 2.7;  round = 3; -> 6 -> 7)
            // scale down to allowed positions
            scaleDownX = (pX - 1) / 2;
            // THEN round, and rescale back up
            ipX = ((int)Math.Round(scaleDownX) * 2) + 1;
        } else {
            // -- even row
            // -- (e.g. pX = 4.8: -> 2.4;  round = 2; -> 4)
            // scale down to allowed positions
            scaleDownX = pX / 2;
            // THEN round, and rescale back up
            ipX = (int)Math.Round(scaleDownX) * 2;
        }

        Vector2Int pos = new Vector2Int(ipX, ipY);
        return pos;
    }

    private void FillGridWithBalls()
    {
        for (int y = 0; y < GRID_ROWS; y++)
        {
            for (int x = 0; x < GRID_COLS; x++)
            {
                int realXPos = x * 2;
                if ((y & 0x1) == 1) {
                    // odd row...
                    // - has one less ball... exit loop iteration early, if at end
                    if (x == (GRID_COLS - 1)) {
                        continue;
                    }
                    // - also, offset xPos by 1, make odd to stagger
                    realXPos += 1;
                }

                Vector2Int ballPosn = new Vector2Int(realXPos, y);
                Vector3 ballCoords = GetCenterCoordForPosition(ballPosn);
                // Debug.Log($"Pos x,y=({realXPos}, {y})");
                // Debug.Log($"Coord x,y=({ballCoords.x}, {ballCoords.y})");

                UnityEngine.Object newBall = Instantiate(ballPrefab, ballCoords,
                                                ballPrefab.transform.rotation);
                newBall.name += $"_{realXPos}_{y}";  // append (x,y) pos to name for identification
            }
        }
    }

}
