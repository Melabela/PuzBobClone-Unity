using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallInfo : MonoBehaviour
{
    public int id;
    public Color color;

    // Start is called before the first frame update
    void Start()
    {
        // Color c = GenerateRandomColor();
        Color c = GetLightGreyColor();
        SetMaterialColor(c);
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void SetHighlight(bool enable)
    {
        if (enable)
        {
            Color c = GetLightRedColor();
            SetMaterialColor(c);
        }
        else
        {
            Color c = GetLightGreyColor();
            SetMaterialColor(c);
        }
    }

    void SetMaterialColor(Color color)
    {
        // update color
        Material m_material = GetComponent<Renderer>().material;
        m_material.color = color;
    }

    Color GenerateRandomColor()
    {
        float r = Random.Range(0.0f, 1.0f);
        float g = Random.Range(0.0f, 1.0f);
        float b = Random.Range(0.0f, 1.0f);
        color = new Color(r, g, b);
        return color;
    }

    Color GetLightGreyColor()
    {
        float r = 0.8f;
        float g = 0.8f;
        float b = 0.8f;
        color = new Color(r, g, b);
        return color;
    }

    Color GetLightRedColor()
    {
        float r = 0.8f;
        float g = 0.6f;
        float b = 0.6f;
        color = new Color(r, g, b);
        return color;
    }
}
