using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    // text object references
    [SerializeField] TextMeshProUGUI gameClearText;
    [SerializeField] TextMeshProUGUI gameOverText;
    [SerializeField] TextMeshProUGUI ballsPlayedText;
    [SerializeField] TextMeshProUGUI ballsPoppedText;

    int nBallsPlayed;
    int nBallsPopped;

    // Start is called before the first frame update
    void Start()
    {
        nBallsPlayed = 0;
        nBallsPopped = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButton("Cancel")) {
            // reload scene
            string currSceneName = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(currSceneName);
        }
    }

    public void ShowGameClearText(bool bActive)
    {
        gameClearText.gameObject.SetActive(bActive);
    }
   
    public void ShowGameOverText(bool bActive)
    {
        gameOverText.gameObject.SetActive(bActive);
    }

    public void AddBallsPlayed(int nBalls)
    {
        nBallsPlayed += nBalls;
        ballsPlayedText.SetText($"Balls Played:  {nBallsPlayed}");
    }

    public void AddBallsPopped(int nBalls)
    {
        nBallsPopped += nBalls;
        ballsPoppedText.SetText($"Balls Popped:  {nBallsPopped}");
    }

}