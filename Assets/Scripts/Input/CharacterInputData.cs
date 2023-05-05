using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[System.Serializable]
public struct CharacterInputData : INetworkSerializable
{
    public ArmsMovementData ArmsMovementData;

    /// <summary>
    /// Action Map: Gameplay<br/>
    /// True if jump pressed
    /// </summary>
    public bool JumpValue;

    /// <summary>
    /// Action Map: Gameplay<br/>
    /// -1 to 1 based on horizontal input representing player movement
    /// </summary>
    public float MoveHorizontalAxis;

    /// <summary>
    /// Action Map: Gameplay<br/>
    /// -1 to 1 based on vertical input representing player movement
    /// </summary>
    public float MoveVerticalAxis;

    /// <summary>
    /// Action Map: Gameplay<br/>
    /// True if left hand grab button is pressed
    /// </summary>
    public bool IsGrabbingLeft;

    /// <summary>
    /// Action Map: Gameplay<br/>
    /// True of the right hand grab button is pressed
    /// </summary>
    public bool IsGrabbingRight;

    public CharacterInputData(ArmsMovementData armsMovementData, bool jumpValue, float moveHorizontalAxis, float moveVerticalAxis, bool isGrabbingLeft, bool isGrabbingRight)
    {
        ArmsMovementData = armsMovementData;
        JumpValue = jumpValue;
        MoveHorizontalAxis = moveHorizontalAxis;
        MoveVerticalAxis = moveVerticalAxis;
        IsGrabbingLeft = isGrabbingLeft;
        IsGrabbingRight = isGrabbingRight;
    }

    void INetworkSerializable.NetworkSerialize<T>(BufferSerializer<T> serializer)
    {
        serializer.SerializeValue(ref ArmsMovementData);
        serializer.SerializeValue(ref JumpValue);
        serializer.SerializeValue(ref MoveHorizontalAxis);
        serializer.SerializeValue(ref MoveVerticalAxis);
        serializer.SerializeValue(ref IsGrabbingLeft);
        serializer.SerializeValue(ref IsGrabbingRight);
    }
}
