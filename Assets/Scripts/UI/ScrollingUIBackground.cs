using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.UI;

public class ScrollingUIBackground : MonoBehaviour
{
    [SerializeField] private List<ScrollableImage> _images;
    [SerializeField] private CanvasGroup _bgImagesCanvasGroup;
    [SerializeField] private int _startImage = 0;
    [SerializeField] private bool _automaticSwitching = false;
    [SerializeField] private float _automaticSwitchTime = 20f;
    [SerializeField] private bool _automaticIsRandom = false;

    private const float DEFAULT_FADE_TIME = 0.75f;

    private int _currentIndex = 0;
    private int _supercededIndex = 0;
    private float _supercededFadeTime = DEFAULT_FADE_TIME;
    private bool _isSuperceded = false;
    private Coroutine _automaticCoroutine;

    private void Awake()
    {
        if (Screen.width <= 0) return; // divide by 0 error check

        float ratio = (float)Screen.height / (float)Screen.width;

        // setup images to tile the same and preserve aspect ratio and set all to false
        foreach(ScrollableImage image in _images)
        {
            image?.SetInitialRectData(ratio);
            image?.RawImage.gameObject.SetActive(false);
        }

        
    }

    private void Start()
    {
        if(_automaticSwitching)
        {
            _automaticCoroutine = StartCoroutine(DoAutomaticSwitching());
        }
        else
        {
            // set to first image
            _currentIndex = _startImage;
            SwitchImage(_startImage, false, DEFAULT_FADE_TIME, false);
        }
    }

    IEnumerator DoAutomaticSwitching()
    {
        // Use index so that we can start from a mid-point in the list

        int index = _startImage;
        SwitchImage(index, false, DEFAULT_FADE_TIME, false);
        index++;

        while (true)
        {
            for(int i = index; i < _images.Count; i++)
            {
                yield return new WaitForSeconds(_automaticSwitchTime);
                SwitchImage(i, _automaticIsRandom, DEFAULT_FADE_TIME, false);
                
            }
            index = 0;
        }
    }

    private void OnEnable()
    {
        Start();
    }

    private void OnDisable()
    {
        if(_automaticCoroutine != null)
        {
            StopCoroutine(_automaticCoroutine);
            _automaticCoroutine = null;
        }
    }

    public void StartAutomaticSwitching(float automaticSwitchTime, bool automaticIsRandom)
    {
        if(_automaticCoroutine == null)
        {
            _automaticSwitching = true;
            _automaticIsRandom = automaticIsRandom;
            _automaticSwitchTime = automaticSwitchTime;
            _automaticCoroutine = StartCoroutine(DoAutomaticSwitching());
        }
    }

    public void SwitchImage(int index, bool random = false, float fadeTime = DEFAULT_FADE_TIME, bool cancelAutomatic = true)
    {
        if (_bgImagesCanvasGroup == null) return;

        if (random)
        {
            index = Random.Range((int)0, (int)_images.Count);
        }

        if(index >= _images.Count) return;

        if (cancelAutomatic)
        {
            _automaticSwitching = false;
            if (_automaticCoroutine != null)
            {
                StopCoroutine(_automaticCoroutine);
                _automaticCoroutine = null;
            }
        }
        

        if (LeanTween.isTweening(_bgImagesCanvasGroup.gameObject))
        {
            // we are already switching image, either switch the index at the point of faded out, or start a new fade once this one is complete
            _isSuperceded = true;
            _supercededFadeTime = fadeTime;
            _supercededIndex = index;
        }
        else
        {
            FadeOutIn(index, fadeTime);
        }
    }

    private void FadeOutIn(int index, float fadeTime)
    {
        if (_images[_currentIndex].RawImage?.gameObject.activeSelf == false)
        {
            FadeIn(index, fadeTime);
        }
        else
        {
            _bgImagesCanvasGroup.LeanAlpha(0, fadeTime).setEaseInOutQuad().setOnComplete(() => { FadeIn(index, fadeTime); });
        }
    }

    private void FadeIn(int index, float fadeTime)
    {
        if(_isSuperceded)
        {
            index = _supercededIndex;
            fadeTime = _supercededFadeTime;
            _isSuperceded = false;
        }
        _images[_currentIndex].RawImage?.gameObject.SetActive(false);
        _images[index].RawImage?.gameObject.SetActive(true);
        _currentIndex = index;
        _bgImagesCanvasGroup.LeanAlpha(1, fadeTime).setEaseInOutQuad().setOnComplete(() => { });
    }

    private void CheckFinalSuperceded()
    {
        if(_isSuperceded)
        {
            FadeOutIn(_supercededIndex, _supercededFadeTime);
            _isSuperceded = false;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        foreach(ScrollableImage image in _images)
        {
            image?.DoImageScrollFixedUpdate();
        }
    }
}
