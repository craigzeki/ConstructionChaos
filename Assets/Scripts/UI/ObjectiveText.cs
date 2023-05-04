using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(TextMeshProUGUI))]
public class ObjectiveText : MonoBehaviour
{
    [SerializeField, Range(0, 1)] private float _screenWidthFactor = 0.8f;
    [SerializeField, Range(0, 1)] private float _screenHeightFactor = 0.2f;
    [SerializeField] private float _timeBeforeShrink = 2f;
    [SerializeField] private float _timeToAnimate = 1f;

    private TextMeshProUGUI _objectiveText;
    private RectTransform _rectTransform;
    private Coroutine _triggerAnimCoroutine;
    private Vector2 _originalSize;
    private Vector3 _originalPosition;
    private Vector2 _centreSize;
    private Vector3 _centrePosition;

    private void Awake()
    {
        _objectiveText = GetComponent<TextMeshProUGUI>();
        _rectTransform = GetComponent<RectTransform>();
        
        //calculate centre of the screen and add any padding
        _centreSize = new Vector2(Screen.width * _screenWidthFactor, Screen.height * _screenHeightFactor);
        _centrePosition = new Vector3((Screen.width / 2f) - (_centreSize.x / 2f), (Screen.height / 2) - (_centreSize.y / 2f), 0);

        if (_rectTransform != null)
        {
            _originalPosition = _rectTransform.position;
            _originalSize = _rectTransform.sizeDelta;

            //move the text to the centre of the screen
            _rectTransform.position = _centrePosition;
            _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _centreSize.x);
            _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _centreSize.y);

            //hide until requested
            _rectTransform.gameObject.SetActive(false);
        }

    }


    IEnumerator TriggerAnim(RectTransform rectTransform, float delay)
    {
        if(rectTransform != null)
        {
            //show the text in the centre of the screen for a period of time
            yield return new WaitForSeconds(delay);
            //animate the text
            DoAnim(rectTransform);
            //indicate the coroutune is done
            _triggerAnimCoroutine = null;
        }
        
    }

    private void DoAnim(RectTransform rectTransform)
    {
        if (rectTransform == null) return;

        //shrink and move
        LeanTween.size(rectTransform, _originalSize, _timeToAnimate).setEaseOutBack();
        LeanTween.move(rectTransform, _originalPosition, _timeToAnimate).setEaseInOutQuad();
    }

    public void SetObjective(string objective)
    {
        //set the text and show
        _objectiveText.text = objective != null ? objective : "No Objective";
        _rectTransform.gameObject.SetActive(true);

        //stop any running coroutine and start again
        if (_triggerAnimCoroutine != null) StopCoroutine(_triggerAnimCoroutine);
        _triggerAnimCoroutine = StartCoroutine(TriggerAnim(_rectTransform, _timeBeforeShrink));
    }

    public void ResetObjective()
    {
        if (_rectTransform != null)
        {
            //move back to the centre of the screen and disable
            _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _centreSize.x);
            _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _centreSize.y);
            _rectTransform.position = _centrePosition;
            _rectTransform.gameObject.SetActive(false);
        }
        
    }
}
