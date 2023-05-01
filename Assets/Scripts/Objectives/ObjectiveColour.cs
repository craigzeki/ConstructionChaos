using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A colour that an objective object can be.
/// </summary>
[CreateAssetMenu(fileName = "ObjectiveColour", menuName = "ScriptableObjects/ObjectiveColour", order = 1)]
public class ObjectiveColour : ScriptableObject
{
    /// <summary>
    /// The friendly string of the colour.
    /// </summary>
    [SerializeField]
    private string friendlyString;
    public string FriendlyString => friendlyString;

    /// <summary>
    /// The actual colour of the colour.
    /// </summary>
    [SerializeField]
    private Color colour;
    public Color Colour => colour;
}
