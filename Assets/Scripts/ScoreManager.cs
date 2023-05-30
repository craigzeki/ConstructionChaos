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

    [SerializeField] private List<float> _streakMultipliers;
    [SerializeField] private List<float> _streakPercentages;
    [SerializeField] private List<Color> _streakColours;

    private float _currentMultiplier;
    private float _currentPercentage;

    private float _streakIncreaseAnimationTime = 0.1f;

    private void Start()
    {
        GameUIManager.Instance?.SetupStreakBar(_streakPercentages, _streakColours);
    }

    public void AddPlayerScore(NetPlayerData playerData, uint score)
    {
        uint objectiveDifficulty = score;

        _currentMultiplier = CalculateCurrentMultiplier();
        score *= (uint)_currentMultiplier;
        playerData.RoundScore += score;

        IncreaseStreak(objectiveDifficulty);

        Debug.Log("Obj Point: " + score);
        Debug.Log("Total Points: " + playerData.RoundScore);
    }

    private float CalculateCurrentMultiplier()
    {
        float totalPercentage = 1f;
        for (int i = _streakPercentages.Count - 1; i >= 0; i--)
        {
            totalPercentage -= _streakPercentages[i];
            if (_currentPercentage >= totalPercentage)
            {
                // If the current multiplier is the first one, return the current value plus 1
                float topVal = i == _streakPercentages.Count - 1 ? _streakMultipliers[i] + 1f : _streakMultipliers[i + 1];

                // Calculate the float value between _multipliers[i] and _multipliers[i + 1] based on the _currentPercentage
                float multiplierValue = Mathf.Lerp(_streakMultipliers[i], topVal, (_currentPercentage - totalPercentage) / _streakPercentages[i]) - 1f;
                multiplierValue = Mathf.Round(multiplierValue * 10f) / 10f;
                return Mathf.Max(multiplierValue, 1f);
            }
        }
        return 1f;
    }

    private void IncreaseStreak(uint objectiveDifficulty)
    {
        // Increase the streak bar based on the difficulty of the objective
        _currentPercentage = Mathf.Clamp01((objectiveDifficulty / 200f) + _currentPercentage);
        _currentMultiplier = CalculateCurrentMultiplier();
        GameUIManager.Instance?.UpdateStreakBar(_currentMultiplier, _currentPercentage);
        print("Increase Streak: " + _currentPercentage);
    }

    public void DecreaseStreak(uint objectiveDifficulty)
    {
        // TIME = DISTANCE / SPEED
        float animationTime = _currentPercentage / (objectiveDifficulty / 3200f);
        print("Decrease Streak: " + _currentPercentage + " in " + animationTime + " seconds");
        // Wait until all other LeanTweens have finished
        LeanTween.value(gameObject, _currentPercentage, 0f, animationTime).setOnUpdate((float value) =>
        {
            _currentPercentage = value;
            _currentMultiplier = CalculateCurrentMultiplier();
            GameUIManager.Instance?.UpdateStreakBar(_currentMultiplier, _currentPercentage);
        });
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
