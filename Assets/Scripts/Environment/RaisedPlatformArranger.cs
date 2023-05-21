using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaisedPlatformArranger : PlatformArranger
{
    /// <summary>
    /// The top of the platform
    /// </summary>
    [SerializeField] private GameObject _platformTop;

    /// <summary>
    /// The top platform's box collider, used to calculate the bounds
    /// </summary>
    private BoxCollider2D _topCollider;

    /// <summary>
    /// The top platform's sprite renderer, used to adjust the sprite size
    private SpriteRenderer _topSpriteRenderer;

    /// <summary>
    /// The left end of the platform
    /// </summary>
    private SpriteRenderer _leftSpriteRenderer;

    /// <summary>
    /// The right end of the platform
    /// </summary>
    private SpriteRenderer _rightSpriteRenderer;

    /// <summary>
    /// The offset to add to the top platform's width
    /// </summary>
    private const float _topWidthOffset = 1.64f;

    protected override void Awake()
    {
        if (_platformTop == null) return;
        _topCollider = _platformTop.GetComponent<BoxCollider2D>();
        _topSpriteRenderer = _platformTop.GetComponent<SpriteRenderer>();
        base.Awake();
        _leftSpriteRenderer = _platformEndL.GetComponent<SpriteRenderer>();
        _rightSpriteRenderer = _platformEndR.GetComponent<SpriteRenderer>();
        UpdateEndPositions();
        UpdateSpriteSizes();
    }

    public override void UpdateEndPositions()
    {
        base.UpdateEndPositions();

        if (_topCollider == null) return;

        float midYTop = _midCollider.bounds.extents.y;

        _platformTop.transform.localPosition = new Vector3
        (
            _platformMid.transform.localPosition.x,
            _platformMid.transform.localPosition.y + midYTop + _topCollider.bounds.extents.y,
            _platformMid.transform.localPosition.z
        );
    }

    public void UpdateSpriteSizes()
    {
        if (_midCollider == null) return;
        if (_topSpriteRenderer == null) return;

        _topSpriteRenderer.size = new Vector2((_midCollider.bounds.size.x / _topSpriteRenderer.gameObject.transform.localScale.x) + _topWidthOffset, _topSpriteRenderer.size.y);

        if (_leftSpriteRenderer == null) return;

        _leftSpriteRenderer.size = new Vector2(_leftSpriteRenderer.size.x, _midCollider.bounds.size.y / _leftSpriteRenderer.gameObject.transform.localScale.y);

        if (_rightSpriteRenderer == null) return;

        _rightSpriteRenderer.size = new Vector2(_rightSpriteRenderer.size.x, _midCollider.bounds.size.y / _rightSpriteRenderer.gameObject.transform.localScale.y);
    }
}
