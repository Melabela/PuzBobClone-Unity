using System;  // for Math
using System.Collections;
using System.Collections.Generic;
using System.Text;  // for StringBuilder
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
    // -- exposed & configurable variables --
    [SerializeField] int GRID_ROWS;
    [SerializeField] int GRID_COLS;
    [SerializeField] float GRID_UNIT_SIZE;  // diameter
    // z-coordinate to use, when moving between gridPos -> 3d-coords
    [SerializeField] float Z_COORD = 0.0f;  // set constant
    // prefab used to generate balls
    [SerializeField] GameObject ballPrefab;

    // -- internal variables --
    // minimum number of connected (adjacent) balls, needed to "pop" the chain
    const int MIN_CONNECTED_BALLS_FOR_POP = 3;

    float half_unit;    // radius
    float height_unit;  // "orange stacking" row height

    // create arrays for grid storage, indexed as (x, y)
    int maxAllowedXPos;  // inclusive
    int[,] gridBallIds;
    GameObject[,] gridBallObjs;

    // Start is called before the first frame update
    void Start()
    {
        GridUnitInit();
        GridStorageInit();
        FillGridWithBalls(3);
    }

    // Update is called once per frame
    void Update()
    {
    }

    void GridUnitInit()
    {
        half_unit = GRID_UNIT_SIZE * 0.5f;

        // Pythagoras: a^2 + b^2 = c^2
        //   b = 0.5;  c = 1.0;  _(for unit grid)_
        //   a = sqrt( 1.0^2 - 0.5^2 ) = sqrt( 0.75 ) = 0.8660
        height_unit = (float) Math.Sqrt(
            Math.Pow(GRID_UNIT_SIZE, 2) - Math.Pow(half_unit, 2)
            );
    }

    void GridStorageInit()
    {
        maxAllowedXPos = (GRID_COLS - 1) * 2;  // e.g. 8 cols -> x==14
        gridBallIds = new int[maxAllowedXPos + 1, GRID_ROWS];
        gridBallObjs = new GameObject[maxAllowedXPos + 1, GRID_ROWS];
    }

    bool IsOdd(int n)
    {
        return (n & 0x1) == 1;
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
        // reverse calculations in above fn
        float pX = (coord.x / half_unit) - 1;
        float pY = (coord.y - half_unit) / height_unit;
        // Debug.Log($"half_unit={half_unit}, height_unit={height_unit}");

        // round to nearest integer position
        int ipY = (int)Math.Round(pY);
        int ipX;
        // depending on row, X-pos needs to align odd or even!
        if (IsOdd(ipY)) {
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
        if ( (pos.x < 0) || (pos.x > maxAllowedXPos) ) {
            return false;
        }
        if ( (pos.y < 0) || (pos.y >= GRID_ROWS) ) {
            return false;
        }

        // check 2. - column is odd/even depending on the row
        // - for odd  row (y):  col values (x) should be odd  as well
        // - for even row (y):  col values (x) should be even as well
        if (IsOdd(pos.y) != IsOdd(pos.x)) {
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
            gridPos + new Vector2Int(-1, +1),  // left-upper
            gridPos + new Vector2Int(+1, +1),  // right-upper
            gridPos + new Vector2Int(-2,  0),  // fullLeft-sameRow
            gridPos + new Vector2Int(+2,  0),  // fullRight-sameRow
            gridPos + new Vector2Int(-1, -1),  // left-lower
            gridPos + new Vector2Int(+1, -1)   // right-lower
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

    void FillGridWithBalls(int rowsToFill)
    {
        int maxY = Math.Min(GRID_ROWS, rowsToFill);
        for (int y = 0; y < maxY; y++)
        {
            bool bIsRowOdd = IsOdd(y);

            // odd row has one less ball, than even
            int xAcross = bIsRowOdd ? (GRID_COLS - 1) : GRID_COLS;
            for (int xBall = 0; xBall < xAcross; xBall++)
            {
                // if odd-row, offset xPos by 1 to make it odd as well
                int realXPos = (xBall * 2) + (bIsRowOdd ? 1 : 0);

                Vector2Int ballPosn = new Vector2Int(realXPos, y);
                Vector3 ballCoords = GetCenterCoordForPosition(ballPosn);
                // Debug.Log($"Pos x,y=({realXPos}, {y})");
                // Debug.Log($"Coord x,y=({ballCoords.x}, {ballCoords.y})");

                GameObject newBall = Instantiate(ballPrefab, ballCoords,
                                            ballPrefab.transform.rotation);
                newBall.name += $"_{realXPos},{y}";  // append (x,y) pos to name for identification

                // mark ball on map
                int ballId = newBall.GetComponent<BallInfo>().GetId();
                MarkBallInGrid(ballPosn, ballId, newBall);
            }
        }
    }

    public bool HasBallInGrid(Vector2Int posInGrid)
    {
        if (!IsPosWithinGrid(posInGrid)) {
            Debug.LogWarning($"HasBallInGrid() - ignoring call w/ invalid posInGrid={posInGrid}");
            return false;
        }

        return gridBallIds[posInGrid.x, posInGrid.y] > 0;
    }

    void ClearBallInGrid(Vector2Int posInGrid)
    {
        if (!IsPosWithinGrid(posInGrid)) {
            Debug.LogWarning($"ClearBallInGrid() - ignoring call w/ invalid posInGrid={posInGrid}");
            return;  // exit early
        }

        var ballIdAtPos = gridBallIds[posInGrid.x, posInGrid.y];
        if (ballIdAtPos > 0) {
            var ballObjAtPos = gridBallObjs[posInGrid.x, posInGrid.y];
            if (ballObjAtPos) {
                Destroy(ballObjAtPos);
            }

            // Debug.Log($"ClearBallInGrid() called w/ posInGrid={posInGrid}");
            gridBallIds[posInGrid.x, posInGrid.y] = 0;
            gridBallObjs[posInGrid.x, posInGrid.y] = null;
        }
    }

    public void MarkBallInGrid(Vector2Int posInGrid, int ballId, GameObject ballObj)
    {
        if (!IsPosWithinGrid(posInGrid)) {
            Debug.LogWarning($"MarkBallInGrid() - ignoring call w/ invalid posInGrid={posInGrid}");
            return;  // exit early
        }

        // Debug.Log($"MarkBallInGrid() called w/ posInGrid={posInGrid}, ballId={ballId}, ballObj={ballObj}");
        gridBallIds[posInGrid.x, posInGrid.y] = ballId;
        gridBallObjs[posInGrid.x, posInGrid.y] = ballObj;

        // DEBUG ONLY!
        // string gridDebug = ShowBallIdsInGrid();
        // Debug.Log($"ShowBallIdsInGrid():\n{gridDebug}");
    }

    // e.g. use for debugging
    string ShowBallIdsInGrid()
    {
        // (x + 6)... 6 = two chars for row#, two pipe(|)-chars for edge,
        //                one for zeroth elem, and one for new-line
        int estimRowStrLen = maxAllowedXPos + 6;
        // 3 extra rows = for bottomLine, & two-digit x-pos
        int estimFullStrLen = estimRowStrLen * (GRID_ROWS + 3);

        StringBuilder sbRow = new StringBuilder(estimRowStrLen);
        StringBuilder sbGrid = new StringBuilder(estimFullStrLen);

        for (int y = GRID_ROWS - 1; y >= 0; y--)
        {
            bool bIsRowOdd = IsOdd(y);
            sbRow.Clear();

            string oddRowSpace = bIsRowOdd ? " " : "";
            // include row number at left
            sbRow.AppendFormat("{0,2}|{1}", y, oddRowSpace);  // "rr|" or "rr| "

            // odd row has one less ball, than even
            int xAcross = bIsRowOdd ? (GRID_COLS - 1) : GRID_COLS;
            for (int xBall = 0; xBall < xAcross; xBall++)
            {
                // if odd-row, offset xPos by 1 to make it odd as well
                int realXPos = (xBall * 2) + (bIsRowOdd ? 1 : 0);
                string spaceBtwn = (xBall > 0) ? " " : "";  // AFTER first ball, add space between

                int ballId = gridBallIds[realXPos, y];
                string ballStr = (ballId > 0) ? ballId.ToString() : ".";
                sbRow.AppendFormat("{0}{1}", spaceBtwn, ballStr);
            }

            sbRow.AppendFormat("{0}|", oddRowSpace);  // "|" or " |"
            // add constructed row to full string
            sbGrid.AppendLine(sbRow.ToString());
        }

        // add bottom line
        sbRow.Clear();
        string bottomLine = new string('-', maxAllowedXPos + 1);  // repeat char (1st param), for count (2nd)
        sbRow.AppendFormat("  +{0}+", bottomLine);  // two row digits, corner-char, horiz-line, and corner
        sbGrid.AppendLine(sbRow.ToString());

        // add col numbers at bottom
        //  first row:  012 ... 891111 ...
        // second row:            0123 ...
        sbRow.Clear();
        sbRow.Append("   ");  // two row digits, and |
        for (int x = 0; x <= maxAllowedXPos; x++) {
            sbRow.Append((x < 10) ? x : (x / 10));
        }
        sbGrid.AppendLine(sbRow.ToString());

        sbRow.Clear();
        sbRow.Append("   ");  // two row digits, and |
        for (int x = 0; x <= maxAllowedXPos; x++) {
            sbRow.Append((x < 10) ? " " : (x % 10));
        }
        sbGrid.AppendLine(sbRow.ToString());

        // FINALLY, output set of lines assembled
        return sbGrid.ToString();
    }

    // rough algorithm
    // 0. create a list to keep track of matching id (== color) positions
    // 1. start at init position, and mark it if match
    // 2. get 6 hexagonal neighbors, and foreach
    //      a) if match, mark and call recursively to those positions
    //      b) if not match, just return
    //      NOTE: either way, mark cloned-grid, to indicate we've checked that position
    // 3. with 2. should have done depth-first search to all adjacently connected positions
    // 4. if list.length > MIN_NUM_CONNECTED_TO_POP, then okay to pop the chain
    List<Vector2Int> CheckForChainedIds(int checkForId, Vector2Int checkFromPos)
    {
        // clone of id grid, to mark when walking neighbors
        int[,] gridBallIdsCloneToMark = (int[,])gridBallIds.Clone();
        // list of found positions
        List<Vector2Int> matchingPositions = new List<Vector2Int>();

        FindIdAtGridPosAndNeighbors(checkForId, checkFromPos,
                                    ref gridBallIdsCloneToMark, matchingPositions);
        Debug.Log($"CheckForChainedIds() - recursive check done from {checkFromPos}, id={checkForId}... got {matchingPositions.Count} matches");
        if (matchingPositions.Count > 1) {
            string debugStrMatchPos = string.Join(", ", matchingPositions.ConvertAll(pos => pos.ToString()).ToArray());
            Debug.Log($"CheckForChainedIds() - matchingPositions=[{debugStrMatchPos}]");
        }

        if (matchingPositions.Count >= MIN_CONNECTED_BALLS_FOR_POP) {
            // return positions only if >= minimum_count required
            return matchingPositions;
        } else {
            // else, return empty list
            return new List<Vector2Int>();
        }
    }

    bool FindIdAtGridPosAndNeighbors(int checkForId,            // id to find
                                     Vector2Int thisPos,        // current grid position to check
                                     ref int[,] gridBallIdsToMark,  // cloned ballId grid, to check/mark when walking
                                     List<Vector2Int> matchPositions)  // store of found positions
    {
        // check id at thisPos
        int idAtPos = gridBallIds[thisPos.x, thisPos.y];
        bool bMatch = idAtPos == checkForId;

        // mark off current position, either way
        gridBallIdsToMark[thisPos.x, thisPos.y] = -1;  // negative ids not normally used
        // Debug.Log($"FindIdAtGridPosAndNeighbors() - at {thisPos}, bMatch={bMatch}");

        if (bMatch) {
            // add this position to found list
            matchPositions.Add(thisPos);

            // get neighbor positions
            var neighborPositions = GetNeighboringPositions(thisPos);
            // recursively call neighbor positions that are left
            foreach (var neighbPos in neighborPositions) {
                // ignore already walked neighbors positions (-1),
                //  to avoid repeat checks or going into cycles
                if (gridBallIdsToMark[neighbPos.x, neighbPos.y] >= 0) {
                    FindIdAtGridPosAndNeighbors(checkForId, neighbPos,
                                                ref gridBallIdsToMark, matchPositions);
                }
            }
        }

        return bMatch;
    }

    public void CheckAndPopBalls(int ballId, Vector2Int ballGridPos)
    {
        // and check if it causes any clearing, for that color, from that position
        var ballPosListToPop = CheckForChainedIds(ballId, ballGridPos);
        foreach (var popBallPos in ballPosListToPop) {
            ClearBallInGrid(popBallPos);
        }
    }

}
