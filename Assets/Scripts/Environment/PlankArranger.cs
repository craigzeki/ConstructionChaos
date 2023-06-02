using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
/// <summary>
/// Automatically spaces the end platforms to match the size of the middle one.
/// </summary>
public class PlankArranger : MonoBehaviour
{
    /// <summary>
    /// The middle platform
    /// </summary>
    [SerializeField] protected GameObject _platformMid;

    /// <summary>
    /// The left end platform
    /// </summary>
    [SerializeField] protected GameObject _platformEndL;

    /// <summary>
    /// The right end platform
    /// </summary>
    [SerializeField] protected GameObject _platformEndR;

    /// <summary>
    /// The middle platform's box collider, used to calculate the bounds
    /// </summary>
    protected BoxCollider2D _midCollider;

    /// <summary>
    /// The left end platform's platform's box collider, used to calculate the bounds
    /// </summary>
    protected BoxCollider2D _endLCollider;

    /// <summary>
    /// The right end platform's box collider, used to calculate the bounds
    /// </summary>
    protected BoxCollider2D _endRCollider;

    protected BoxCollider2D _mainCollider;

    protected virtual void Awake()
    {
        if (_platformMid == null) return;
        if (_platformEndL == null) return;
        if (_platformEndR == null) return;

        _midCollider = _platformMid.GetComponent<BoxCollider2D>();
        _endRCollider = _platformEndR.GetComponent<BoxCollider2D>();
        _endLCollider = _platformEndL.GetComponent<BoxCollider2D>();
        _mainCollider = GetComponent<BoxCollider2D>();

        UpdateEndPositions();
    }

    public virtual void UpdateEndPositions()
    {
        if (_midCollider == null) return;

        if (_endLCollider == null) return;
        float midXLeft = _midCollider.bounds.extents.x;
        _platformEndL.transform.localPosition = new Vector3
        (
            _platformMid.transform.localPosition.x - midXLeft - _endLCollider.bounds.extents.x,
            _platformMid.transform.localPosition.y,
            _platformMid.transform.localPosition.z
        );

        if (_endRCollider == null) return;
        float midXRight = _midCollider.bounds.extents.x;

        _platformEndR.transform.localPosition = new Vector3
        (
            _platformMid.transform.localPosition.x + midXRight + _endRCollider.bounds.extents.x,
            _platformMid.transform.localPosition.y,
            _platformMid.transform.localPosition.z
        );

        _mainCollider.offset= Vector3.zero;
        //_mainCollider.size = new Vector2(((_midCollider.size.x * _platformMid.transform.localScale.x) + (_endLCollider.size.x * _platformEndL.transform.localScale.x) + (_endRCollider.size.x * _platformEndR.transform.localScale.x)), _midCollider.size.y * _platformMid.transform.localScale.y);
        _mainCollider.size = new Vector2((_endLCollider.bounds.extents.x * 2) + (_midCollider.bounds.extents.x * 2) + (_endRCollider.bounds.extents.x * 2), (_midCollider.bounds.extents.y * 2));
    }

    protected virtual void OnDrawGizmos()
    {
        if (Application.IsPlaying(this)) return;
        //Debug.Log("DrawGizmo");
        Awake();
    }
}