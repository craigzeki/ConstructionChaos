using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Container for Arms Movement Input Data
/// </summary>
[System.Serializable]
public struct ArmsMovementData : INetworkSerializable
{
    /// <summary>
    /// Action Map: Gameplay<br/>
    /// 2D array representing horizontal and vertical axis for arms movement
    /// </summary>
    public Vector2 ArmsControllerInput;

    /// <summary>
    /// Action Map: Gameplay<br/>
    /// Float representing the rotation of the arms
    /// </summary>
    public float ArmRotation;

    /// <summary>
    /// Action Map: Gameplay<br/>
    /// Boolean flag indicating if last input was from a mouse controller
    /// </summary>
    public bool IsMouseController;

    /// <summary>
    /// Action Map: Gameplay<br/>
    /// True if the gamepad stick controlling the arms centred
    /// </summary>
    public bool ArmsStickReleased;

    public ArmsMovementData(Vector2 armsControllerInput, float armRotation, bool isMouseController, bool armsStickReleased)
    {
        ArmsControllerInput = armsControllerInput;
        ArmRotation = armRotation;
        IsMouseController = isMouseController;
        ArmsStickReleased = armsStickReleased;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ArmsControllerInput);
        serializer.SerializeValue(ref ArmRotation);
        serializer.SerializeValue(ref IsMouseController);
        serializer.SerializeValue(ref ArmsStickReleased);
    }
}
