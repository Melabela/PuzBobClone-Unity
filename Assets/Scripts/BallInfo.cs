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
    GameObject ballDropperObj;
    // GameObject gridObj;

    // internal vars / state
    int my_id;
    bool bDetectCollision;

    // Start is called before the first frame update
    void Start()
    {
        ballDropperObj = GameObject.Find("BallDropper");
        // gridObj = GameObject.Find("Grid");

        my_id = Random.Range(BALL_MIN_ID, BALL_MAX_ID + 1);
        // Debug.Log($"BallInfo.Start() - my_id={my_id}");
        UpdateColor();

        bDetectCollision = true;
    }

    // Update is called once per frame
    void Update()
    {
    }

    void UpdateColor()
    {
        if ((my_id >= BALL_MIN_ID) && (my_id <= BALL_MAX_ID))
        {
            // active state
            var color_name = ballIndexToColorName[my_id];
            var material_name = ballColorNameToMaterialName[color_name];
            var color_material = Resources.Load<Material>(material_name);

            // update material
            Renderer ballRenderer = GetComponent<Renderer>();
            ballRenderer.material = color_material;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (bDetectCollision) {
            Debug.Log("ENTER BallInfo.OnCollisionEnter");

            // ONLY process FIRST collision, for now
            bDetectCollision = false;

            // stop movement & forces
            Rigidbody ballRb = GetComponent<Rigidbody>();
            ballRb.velocity = Vector3.zero;

            // notify dropper
            BallDropper ballDropperScript = ballDropperObj.GetComponent<BallDropper>();
            ballDropperScript.BallDropDone();
        }
    }
}
