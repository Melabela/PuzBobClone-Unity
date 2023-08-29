using System;  // for Math
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/****
 * For GRID_ROWS = 5, and GRID_COLS = 5:
 *
 * Positions Example  (x, y)
 * |x x x x x| (0, 4),  (2, 4),  (4, 4),  (6, 4),  (8, 4)
 * | x x x x |     (1, 3),  (3, 3),  (5, 3),  (7, 3)
 * |x x x x x| (0, 2),  (2, 2),  (4, 2),  (6, 2),  (8, 2)
 * | x x x x |     (1, 1),  (3, 1),  (5, 1),  (7, 1)
 * |x x x x x| (0, 0),  (2, 0),  (4, 0),  (6, 0),  (8, 0)
 *
 * Notes
 * - So even rows (y), will use even horiz positions (x)
 ****/

public class GridPositions : MonoBehaviour
{
    [SerializeField] int GRID_ROWS;
    [SerializeField] int GRID_COLS;
    [SerializeField] float GRID_UNIT_SIZE;  // diameter
    [SerializeField] float Z_COORD = 0.0f;  // set constant
    [SerializeField] GameObject ballPrefab;

    private float half_unit;    // radius
    private float height_unit;  // "orange stacking" rows

    // Start is called before the first frame update
    void Start()
    {
        GridInit();
        // FillGridWithBalls();
    }

    // Update is called once per frame
    void Update()
    {
    }

    void GridInit()
    {
        half_unit = GRID_UNIT_SIZE / 2.0f;

        // Pythagoras: a^2 + b^2 = c^2
        //   b = 0.5;  c = 1.0;  _(for unit grid)_
        //   a = sqrt( 1.0^2 - 0.5^2 ) = sqrt( 0.75 ) = 0.8660
        height_unit = (float) Math.Sqrt(
            Math.Pow(GRID_UNIT_SIZE, 2) - Math.Pow(half_unit, 2)
            );
    }

    public Vector3 GetCenterCoordForPosition(Vector2Int pos)
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

    public Vector2Int GetClosestPositionForCenterCoord(Vector3 coord)
    {
        // ignore Z-coord.
        // reverse above calculations
        float pX = (coord.x / half_unit) - 1;
        float pY = (coord.y - half_unit) / height_unit;
        // Debug.Log($"half_unit={half_unit}, height_unit={height_unit}");

        // round to nearest integer position
        int ipY = (int)Math.Round(pY);
        int ipX;
        // depending on row, X-pos needs to align odd or even!
        if ((ipY & 0x1) == 1) {
            // -- odd row
            // -- (e.g. pX = 6.4: -> 5.4 -> 2.7;  round = 3; -> 6 -> 7)
            // scale down to allowed positions
            float scaleDownX = (pX - 1) / 2;
            // THEN round, and rescale back up
            ipX = ((int)Math.Round(scaleDownX) * 2) + 1;
        } else {
            // -- even row
            // -- (e.g. pX = 4.8: -> 2.4;  round = 2; -> 4)
            // scale down to allowed positions
            float scaleDownX = pX / 2;
            // THEN round, and rescale back up
            ipX = (int)Math.Round(scaleDownX) * 2;
        }

        Vector2Int pos = new Vector2Int(ipX, ipY);

        // additional check... closest position at edge maybe outside grid
        //  (esp. for odd rows)
        if (!IsPosWithinGrid(pos)) {
            pos = GetNearestValidPositionForCoord(pos, coord);
        }

        return pos;
    }

    bool IsPosWithinGrid(Vector2Int pos)
    {
        // check 1. - is within bounds
        int maxAllowedXPos = (GRID_COLS - 1) * 2;  // e.g. 8 cols -> x==14
        if ( (pos.x < 0) || (pos.x > maxAllowedXPos) ) {
            return false;
        }
        if ( (pos.y < 0) || (pos.y >= GRID_ROWS) ) {
            return false;
        }

        // check 2. - column is odd/even depending on the row
        // - for odd  row (y):  col values (x) should be odd  as well
        // - for even row (y):  col values (x) should be even as well
        if ((pos.y & 0x1) != (pos.x & 0x1)) {
            return false;
        }

        // passed all checks
        return true;
    }

    // since grid is in hex pattern, can have up to 6 neighbors
    //    x   x
    //  x   P   x
    //    x   x
    List<Vector2Int> GetNeighboringPositions(Vector2Int gridPos)
    {
        List<Vector2Int> neighbPos = new List<Vector2Int>(){
            gridPos + new Vector2Int(-1, +1),  // upper-left
            gridPos + new Vector2Int(+1, +1),  // upper-right
            gridPos + new Vector2Int(-2,  0),  // center-left
            gridPos + new Vector2Int(+2,  0),  // center-right
            gridPos + new Vector2Int(-1, -1),  // lower-left
            gridPos + new Vector2Int(-1, +1)   // lower-right
        };

        // filter and keep only positions which are in-bounds
        List<Vector2Int> neighbPosFiltered =
                neighbPos.Where(pos => IsPosWithinGrid(pos)).ToList();
        return neighbPosFiltered;
    }

    Vector2Int GetNearestValidPositionForCoord(Vector2Int computedPos, Vector3 refCoord)
    {
        List<Vector2Int> neighbPosToCheck = GetNeighboringPositions(computedPos);

        Vector2Int nearestPos = computedPos;
        float nearestDistance = float.MaxValue;
        foreach (Vector2Int pos in neighbPosToCheck)
        {
            Vector3 coordAtPos = GetCenterCoordForPosition(pos);
            float dist = Vector3.Distance(coordAtPos, refCoord);
            if (dist < nearestDistance) {
                nearestDistance = dist;
                nearestPos = pos;
            }
        }
        // Debug.Log($"GetNearestValidPositionForCoord() - computedPos={computedPos}, nearestPos={nearestPos}");
        return nearestPos;
    }

    void FillGridWithBalls()
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
                newBall.name += $"_{realXPos},{y}";  // append (x,y) pos to name for identification
            }
        }
    }
}
