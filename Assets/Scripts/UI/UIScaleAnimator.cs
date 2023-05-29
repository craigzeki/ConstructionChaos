using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIScaleAnimator : MonoBehaviour
{
    [SerializeField] private Vector2 _smallScale = Vector2.one;
    [SerializeField] private Vector2 _largeScale = new Vector2(1.2f, 1.2f);
    [SerializeField] private float _scaleSpeed = 1f;
    private bool _scaleUp = true;

    private void OnEnable()
    {
        transform.localScale = _smallScale;
        _scaleUp = true;
        AnimateLoop();
    }

    private void OnDisable()
    {
        LeanTween.cancel(gameObject);
    }

    private void AnimateLoop()
    {
        LeanTween.scale(gameObject, _scaleUp ? _largeScale : _smallScale, _scaleSpeed).setEaseInOutSine().setOnComplete(AnimateLoop);
        _scaleUp = !_scaleUp;
    }
}
