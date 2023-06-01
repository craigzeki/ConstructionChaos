using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ScoreManager : NetworkBehaviour
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

    private float _currentMultiplier = 1f;
    private float _currentPercentage = 0f;

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

        if (IsServer)
        {
            IncreaseStreakClientRpc(objectiveDifficulty, playerData.ClientRpcParams);
        }

        Debug.Log("Objective Difficulty: " + objectiveDifficulty);
        Debug.Log("Current Multiplier: " + _currentMultiplier);
        Debug.Log("Obj Points: " + score);
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

    [ClientRpc]
    private void IncreaseStreakClientRpc(uint objectiveDifficulty, ClientRpcParams clientRpcParams = default)
    {
        IncreaseStreak(objectiveDifficulty);
    }

    private void IncreaseStreak(uint objectiveDifficulty)
    {
        // Increase the streak bar based on the difficulty of the objective
        _currentPercentage = Mathf.Clamp01((objectiveDifficulty / 200f) + _currentPercentage);
        _currentMultiplier = CalculateCurrentMultiplier();
        LeanTween.cancel(gameObject);
        GameUIManager.Instance.UpdateStreakBar(_currentMultiplier, _currentPercentage);
        print("Increase Streak: " + _currentPercentage);
    }

    [ClientRpc]
    public void DecreaseStreakClientRpc(uint objectiveDifficulty, ClientRpcParams clientRpcParams = default)
    {
        DecreaseStreak(objectiveDifficulty);
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
            GameUIManager.Instance.UpdateStreakBar(_currentMultiplier, _currentPercentage);
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
