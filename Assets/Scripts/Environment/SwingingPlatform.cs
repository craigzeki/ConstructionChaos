using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingingPlatform : MonoBehaviour
{
    [SerializeField] private Transform _rightChainWorldConnectionPoint;
    [SerializeField] private Transform _leftChainWorldConnectionPoint;

    //[SerializeField] private Transform _leftConnectionPoint;
    //[SerializeField] private Transform _rightConnectionPoint;
    //[SerializeField] private Transform _leftChainPoint;
    //[SerializeField] private Transform _rightChainnPoint;
    //[SerializeField] private float _separationThreshold = 0.05f;

    private Vector3 _leftChainOrigin = Vector3.zero;
    private Vector3 _rightChainOrigin = Vector3.zero;

    //private float _distance = 0;
    //private Vector3 _delta = Vector2.zero;

    private void Awake()
    {
        if(_rightChainWorldConnectionPoint != null) _rightChainOrigin = _rightChainWorldConnectionPoint.position;
        if (_leftChainWorldConnectionPoint != null) _leftChainOrigin = _leftChainWorldConnectionPoint.position;
    }

    private void Update()
    {
        _leftChainWorldConnectionPoint.position = _leftChainOrigin;
        _rightChainWorldConnectionPoint.position = _rightChainOrigin;
    }

    //private void FixedUpdate()
    //{
    //    if(_leftChainPoint == null) return;
    //    if (_leftConnectionPoint == null) return;

    //    _distance = Vector2.Distance(_leftChainPoint.position, _leftConnectionPoint.position);
    //    if(_distance >= _separationThreshold)
    //    {
    //        AttemptCorrection(_leftConnectionPoint, _leftChainPoint);
    //    }

    //    if(_rightConnectionPoint == null) return;
    //    if(_rightChainnPoint == null) return;
    //    _distance = Vector2.Distance(_leftChainPoint.position, _leftConnectionPoint.position);
    //    if (_distance >= _separationThreshold)
    //    {
    //        AttemptCorrection(_rightConnectionPoint, _rightChainnPoint);
    //    }
    //}

    //private void AttemptCorrection(Transform pointToCorrect, Transform connectedPoint)
    //{
    //    _delta.x = pointToCorrect.position.x - connectedPoint.position.x;
    //    _delta.y = pointToCorrect.position.y - connectedPoint.position.y;
    //    transform.position -= _delta;
    //}

}
