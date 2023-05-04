using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterInputData
{
    public ArmsMovementData ArmsMovementData = new ArmsMovementData();
    public bool JumpValue = false;
    public float MoveHorizontalAxis = 0f;
    public float MoveVerticalAxis = 0f;
    public bool IsGrabbingLeft = false;
    public bool IsGrabbingRight = false;
}
