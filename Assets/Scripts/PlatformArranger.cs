using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;


/// <summary>
/// Automatically spaces the end platforms to match the size of the middle one.
/// </summary>
public class PlatformArranger : MonoBehaviour
{
    /// <summary>
    /// The middle platform
    /// </summary>
    [SerializeField] private GameObject _platformMid;
    /// <summary>
    /// The left end platform
    /// </summary>
    [SerializeField] private GameObject _platformEndL;
    /// <summary>
    /// The right end platform
    /// </summary>
    [SerializeField] private GameObject _platformEndR;

    /// <summary>
    /// The middle platform's box collider, used to calculate the bounds
    /// </summary>
    private BoxCollider2D _midCollider;
    /// <summary>
    /// The left end platform's platform's box collider, used to calculate the bounds
    /// </summary>
    private BoxCollider2D _endLCollider;
    /// <summary>
    /// The right end platform's box collider, used to calculate the bounds
    /// </summary>
    private BoxCollider2D _endRCollider;

    private void Awake()
    {
        if (_platformMid == null) return;
        if (_platformEndL == null) return;
        if (_platformEndR == null) return;

        _midCollider = _platformMid.GetComponent<BoxCollider2D>();
        _endRCollider = _platformEndR.GetComponent<BoxCollider2D>();
        _endLCollider = _platformEndL.GetComponent<BoxCollider2D>();

        UpdateEndPositions();
        
    }

    public void UpdateEndPositions()
    {
        if (_midCollider == null) return;

        if (_endLCollider == null) return;
        float midXLeft = _midCollider.bounds.extents.x;
        _platformEndL.transform.localPosition = new Vector3
                                                    (_platformMid.transform.localPosition.x - midXLeft - _endLCollider.bounds.extents.x,
                                                    _platformMid.transform.localPosition.y,
                                                    _platformMid.transform.localPosition.z
                                                    );

        if (_endRCollider == null) return;
        float midXRight = _midCollider.bounds.extents.x;

        _platformEndR.transform.localPosition = new Vector3
                                                    (_platformMid.transform.localPosition.x + midXRight + _endRCollider.bounds.extents.x,
                                                    _platformMid.transform.localPosition.y,
                                                    _platformMid.transform.localPosition.z
                                                    );
    }



    private void OnDrawGizmos()
    {
        if (Application.IsPlaying(this)) return;
        Debug.Log("DrawGizmo");
        Awake();


        

    }
}
