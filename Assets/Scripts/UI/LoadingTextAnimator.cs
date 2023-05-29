using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LoadingTextAnimator : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _loadingText;
    [SerializeField] private float _waitTime = 0.5f;
    private string _originalText;
    private int _dotsIndex = 0;

    private void OnEnable()
    {
        if (_loadingText == null)
            _loadingText = GetComponent<TextMeshProUGUI>();
        _originalText = _loadingText.text;
        StartCoroutine(AnimateLoop());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        _loadingText.text = _originalText;
    }

    private IEnumerator AnimateLoop()
    {
        ShowDot();
        _dotsIndex++;
        yield return new WaitForSeconds(_waitTime);

        if (_dotsIndex > 2)
        {
            _dotsIndex = 0;
            HideAllDots();
            yield return new WaitForSeconds(_waitTime);
        }

        StartCoroutine(AnimateLoop());
    }

    private void ShowDot()
    {
        // Find the first instance of "00" in the text
        int index = _loadingText.text.IndexOf("00");

        if (index < 0)
            return;

        // Remove "00" from the text
        _loadingText.text = _loadingText.text.Remove(index, 2);
    }

    private void HideAllDots()
    {
        _loadingText.text = _loadingText.text.Replace("<#FFFFFF>", "<#FFFFFF00>");
    }
}
