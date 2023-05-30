using SolidUtilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class ScrollableImage
{
    [SerializeField] public RawImage RawImage;
    [SerializeField] public float WidthQty;
    [SerializeField] public Vector2 ScrollRate;
    [SerializeField][ReadOnly] public Rect RectData;

    public void SetInitialRectData(float height2WidthRatio)
    {
        if (RawImage == null) return;

        RectData = new Rect(ScrollRate.x, ScrollRate.y, WidthQty, WidthQty * height2WidthRatio);
        RawImage.uvRect = RectData;
    }

    public void DoImageScrollFixedUpdate()
    {
        if (RawImage == null) return;
        if (RectData == null) return;

        RectData.position += (ScrollRate * Time.fixedDeltaTime);
        RawImage.uvRect = RectData;
    }
}
