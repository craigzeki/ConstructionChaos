using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUIManager : MonoBehaviour
{
    // Singleton
    public static GameUIManager Instance;

    // UI Element References
    [SerializeField] private TextMeshProUGUI _timerText, _objectiveText;

    [SerializeField] private GameObject _groupObjectiveRequirementsParent, _groupObjectiveRequirementPrefab, _groupObjectiveCountdownPrefab;

    [SerializeField] private Sprite _objectiveRequirementDoneSprite, _objectiveRequirementNotDoneSprite;

    [SerializeField] private Image _streakBarImage;

    [SerializeField] private GameObject _streakBarColourPartPrefab;

    [SerializeField] private TextMeshProUGUI _streakText;

    private GameObject CountdownObject;
    private TextMeshProUGUI _groupObjectiveCountdownText;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    /// <summary>
    /// Updates the objective UI text to display the current objective
    /// </summary>
    /// <param name="objectiveString">The string to display</param>
    public void SetObjectiveText(string objectiveString)
    {
        _objectiveText.text = objectiveString;
    }

    /// <summary>
    /// Updates the timer UI text to display the remaining time for this round
    /// </summary>
    /// <param name="seconds">The number of seconds remaining</param>
    public void UpdateTimerUI(uint seconds)
    {
        string minutes = Mathf.Floor(seconds / 60).ToString("00");
        string secondsString = (seconds % 60).ToString("00");
        _timerText.text = minutes + ":" + secondsString;
    }

    public void UpdateGroupObjectiveCountdown(string seconds)
    {
        if (CountdownObject == null)
        {
            SpawnGroupObjectiveCountdown();
        }
        _groupObjectiveCountdownText.text = $"Countdown: {seconds}";
    }

    private void SpawnGroupObjectiveCountdown()
    {
        CountdownObject = Instantiate(_groupObjectiveCountdownPrefab, _groupObjectiveRequirementsParent.transform);
        _groupObjectiveCountdownText = CountdownObject.GetComponent<TextMeshProUGUI>();
    }

    public void CancelGroupObjectiveCountdown()
    {
        Destroy(CountdownObject);
        CountdownObject = null;
        _groupObjectiveCountdownText = null;
    }

    public void HideGroupObjectiveCountdown()
    {
        if (CountdownObject == null) return;
        CountdownObject.SetActive(false);
    }

    public void ShowGroupObjectiveCountdown()
    {
        if (CountdownObject == null) return;
        CountdownObject.SetActive(true);
    }

    /// <summary>
    /// Creates a prefab for each of the provided requirements for the group objective
    /// </summary>
    /// <param name="objectiveRequirements">The list of strings for the requirements to display</param>
    public void SetupGroupObjectiveUI(List<string> objectiveRequirements)
    {
        // Clear the current requirements
        foreach (Transform child in _groupObjectiveRequirementsParent.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (string requirement in objectiveRequirements)
        {
            GameObject requirementObject = Instantiate(_groupObjectiveRequirementPrefab, _groupObjectiveRequirementsParent.transform);
            requirementObject.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "0/" + requirement;
        }
    }

    /// <summary>
    /// Updates an individual requirement for the group objective to be either complete or not
    /// </summary>
    /// <param name="index">The index of the requirement to update</param>
    /// <param name="isComplete">Whether the requirement is complete or not</param>
    public void UpdateGroupObjectiveRequirement(int index, int newValue, bool isComplete = false)
    {
        Transform requirementTransform = _groupObjectiveRequirementsParent.transform.GetChild(index);

        requirementTransform.GetChild(0).GetComponent<Image>().sprite = isComplete ? _objectiveRequirementDoneSprite : _objectiveRequirementNotDoneSprite;

        string requirementText = requirementTransform.GetChild(1).GetComponent<TextMeshProUGUI>().text;

        int slashIndex = requirementText.IndexOf('/');

        requirementText = newValue + requirementText.Substring(slashIndex);

        requirementTransform.GetChild(1).GetComponent<TextMeshProUGUI>().text = requirementText;
    }

    public void SetupStreakBar(List<float> percentages, List<Color> colours)
    {
        float totalPercentage = 1f;
        for (int i = percentages.Count - 1; i >= 0; i--)
        {
            GameObject colourPart = Instantiate(_streakBarColourPartPrefab, _streakBarImage.transform);
            Image colourPartImage = colourPart.GetComponent<Image>();
            colourPartImage.color = colours[i];
            colourPartImage.fillAmount = totalPercentage;
            totalPercentage = Mathf.Clamp01(totalPercentage - percentages[i]);
        }
    }

    public void UpdateStreakBar(float currentMultiplier, float currentPercentage)
    {
        string multiplierString = currentMultiplier.ToString("0.0");
        _streakText.text = $"{multiplierString}x";
        LeanTween.cancel(_streakBarImage.gameObject);
        LeanTween.value(_streakBarImage.gameObject, _streakBarImage.fillAmount, currentPercentage, 0.1f).setEase(LeanTweenType.easeOutCubic).setOnUpdate((float val) =>
        {
            _streakBarImage.fillAmount = val;
        });
    }
}
