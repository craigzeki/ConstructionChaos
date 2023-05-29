using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIRotationAnimator : MonoBehaviour
{
    [SerializeField] private float _minAngle = 0f;
    [SerializeField] private float _maxAngle = 360f;
    [SerializeField] private float _rotationSpeed = 1f;
    private bool _rotateClockwise = true;

    private void OnEnable()
    {
        transform.localRotation = Quaternion.Euler(0, 0, 0);
        _rotateClockwise = true;
        AnimateLoop();
    }

    private void OnDisable()
    {
        LeanTween.cancel(gameObject);
    }

    private void AnimateLoop()
    {
        LeanTween.rotateZ(gameObject, _rotateClockwise ? _maxAngle : _minAngle, _rotationSpeed).setEaseInOutSine().setOnComplete(AnimateLoop);
        _rotateClockwise = !_rotateClockwise;
    }
}
