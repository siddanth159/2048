using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreTracker : MonoBehaviour
{

    private int score;
    public static ScoreTracker scoreTracker;
    public Text ScoreText;
    public Text HighScoreText;

    public int Score
    {
        get
        {
            return score;
        }
        set
        {
            score = value;
            ScoreText.text = score.ToString();
            if(PlayerPrefs.GetInt("HighScore")<=score)
            {
                HighScoreText.text = score.ToString();
                PlayerPrefs.SetInt("HighScore", score);
            }
        }
    }

    private void Awake()
    {
        scoreTracker = this;
        if (!PlayerPrefs.HasKey("HighScore")) PlayerPrefs.SetInt("HighScore", 0);
        ScoreText.text = "0";
        HighScoreText.text = PlayerPrefs.GetInt("HighScore").ToString();
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
