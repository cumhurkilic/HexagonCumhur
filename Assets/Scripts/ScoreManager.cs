using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{

    public Text scoreText;
    public Text highScoreText;
    public Text movementText;
    public int score;
    public int movement;
    private GameData gameData;
    // Start is called before the first frame update
    void Start()
    {
        gameData = FindObjectOfType<GameData>();
    }

    // Update is called once per frame
    void Update()
    {
        scoreText.text = score.ToString();
        movementText.text = movement.ToString();
    }

    /*
     * Increase score after destroy objects. 
    */
    public void IncreaseScore(int amountToIncrease)
    {
        score += amountToIncrease;
        if(gameData != null)
        {
            int highScore = gameData.saveData.highScore;
            if(score > highScore)
            {
                gameData.saveData.highScore = score;
            }
            gameData.Save();
        }
        
    }
    /*
     * Set number of movements.
    */
    public void SetMovement(int movement_)
    {
        movement = movement_;
    }
    /*
     * Set number of High Score of the game.
     * Have a problem not used.
    */
    public void setHighScore(int score)
    {
        highScoreText.text = "" + score;
    }
}
