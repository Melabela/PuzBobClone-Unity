using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    // text object references
    [SerializeField] TextMeshProUGUI gameClearText;
    [SerializeField] TextMeshProUGUI gameOverText;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void ShowGameClearText(bool bActive)
    {
        gameClearText.gameObject.SetActive(bActive);
    }
   
    public void ShowGameOverText(bool bActive)
    {
        gameOverText.gameObject.SetActive(bActive);
    }

}