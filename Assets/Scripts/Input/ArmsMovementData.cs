using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Container for Arms Movement Input Data
/// </summary>
public class ArmsMovementData
{
    /// <summary>
    /// Action Map: Gameplay<br/>
    /// 2D array representing horizontal and vertical axis for arms movement
    /// </summary>
    public Vector2 ArmsControllerInput = Vector2.zero;
    /// <summary>
    /// Action Map: Gameplay<br/>
    /// Boolean flag indicating if last input was from a mouse controller
    /// </summary>
    public bool IsMouseController = false;
    /// <summary>
    /// Action Map: Gameplay<br/>
    /// True if the gamepad stick controlling the arms centred
    /// </summary>
    public bool ArmsStickReleased = true;
}
