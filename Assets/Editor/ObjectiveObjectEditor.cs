using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ObjectiveObjectEditor : Editor
{
    private static GUIContent _colourPrompt = new GUIContent("Colour", "Pick a colour for this object, or leave as 'any colour' for random");

    public override void OnInspectorGUI()
    {
        #if UNITY_EDITOR
        serializedObject.Update();
        SerializedProperty _colour = serializedObject.FindProperty("possibleColours");
        #endif
    }
}
