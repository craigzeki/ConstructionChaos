using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingPlatformArranger : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _background, _topLeft, _topRight, _bottomLeft, _bottomRight, _top, _bottom, _gapFiller, _left, _right;

    private const float topYVal = 0.12f;
    private const float bottomYVal = 0.09f;

    private void Awake()
    {
        UpdateSprites();
    }

    private void UpdateSprites()
    {
        float xVal = (_background.size.x / 2) + 1;
        float yVal = (_background.size.y / 2) + 1;

        _topLeft.transform.localPosition = new Vector3(-xVal, yVal, 0);
        _topRight.transform.localPosition = new Vector3(xVal, yVal, 0);
        _bottomLeft.transform.localPosition = new Vector3(-xVal, -yVal, 0);
        _bottomRight.transform.localPosition = new Vector3(xVal, -yVal, 0);

        yVal = (_background.size.y / 2) + topYVal;

        _top.transform.localPosition = new Vector3(0, yVal, 0);
        _top.size = new Vector2(_background.size.x, 2);

        yVal = (_background.size.y / 2) + 1 + bottomYVal;

        _bottom.transform.localPosition = new Vector3(0, -yVal, 0);
        _bottom.size = new Vector2(_background.size.x, 2);
        _gapFiller.size = new Vector2(_background.size.x, 1);

        _left.transform.localPosition = new Vector3(-xVal, 0, 0);
        _left.size = new Vector2(2, _background.size.y);

        _right.transform.localPosition = new Vector3(xVal, 0, 0);
        _right.size = new Vector2(2, _background.size.y);
    }

    private void OnDrawGizmos()
    {
        Awake();
    }
}
