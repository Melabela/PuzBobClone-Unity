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
        // prototype - set as random color
        float r = Random.Range(0.0f, 1.0f);
        float g = Random.Range(0.0f, 1.0f);
        float b = Random.Range(0.0f, 1.0f);
        color = new Color(r, g, b);

        // update color
        Material m_material = GetComponent<Renderer>().material;
        m_material.color = color;
    }

    // Update is called once per frame
    void Update()
    {
    }
}
