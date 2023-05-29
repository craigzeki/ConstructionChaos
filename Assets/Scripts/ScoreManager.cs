using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    private static ScoreManager _instance;

    public static ScoreManager Instance
    {
        get
        {
            if(_instance == null) _instance = FindObjectOfType<ScoreManager>();
            return _instance;
        }
    }

    public void AddPlayerScore(NetPlayerData playerData, uint score)
    {
        // TODO: Add streak modifier to this score before saving
        playerData.RoundScore += score;
        Debug.Log("Obj Point: " + score);
        Debug.Log("Total Points: " + playerData.RoundScore);
    }

    public void IncrementGameScores()
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.PlayerData == null) return;

        foreach(NetPlayerData playerData in GameManager.Instance.PlayerData.Values)
        {
            playerData.GameScore += playerData.RoundScore;
        }
    }
}
