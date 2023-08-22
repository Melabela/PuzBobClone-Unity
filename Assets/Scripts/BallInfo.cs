using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallInfo : MonoBehaviour
{
    static public int BALL_MIN_ID = 1;  // inclusive
    static public int BALL_MAX_ID = 6;  // inclusive

    Dictionary<int, string> ballIdToColorName = new Dictionary<int, string>()
    {
        [1] = "red",
        [2] = "orange",
        [3] = "yellow",
        [4] = "green",
        [5] = "blue",  
        [6] = "purple"
    };

    Dictionary<string, Color> ballColorNameToColor = new Dictionary<string, Color>()
    {
        ["red"]    = new Color(220, 10, 10),
        ["orange"] = new Color(230, 100, 0),
        ["yellow"] = new Color(240, 240, 100),
        ["green"]  = new Color(0, 200, 0),
        ["blue"]   = new Color(40, 40, 230),
        ["purple"] = new Color(100, 0, 220)
    };

    int my_id;

    public void SetId(int id)
    {
        Debug.Log($"BallInfo.SetId({id})");
        my_id = id;
        UpdateColor();
    }

    // Start is called before the first frame update
    void Start()
    {
        my_id = 0;
        UpdateColor();
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
            var color_name = ballIdToColorName[my_id];
            var color = ballColorNameToColor[color_name];
            SetMaterialColor(color);
        }
        else
        {
            // init state
        }

    }

    void SetMaterialColor(Color color)
    {
        // update color
        Material m_material = GetComponent<Renderer>().material;
        m_material.color = color;
    }

}
