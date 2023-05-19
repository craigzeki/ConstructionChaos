using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

//[CustomEditor(typeof(ObjectiveObjectInstance), true)]
//public class ObjectiveObjectInstanceEditor : Editor
//{
//    SerializedProperty _objectSO;
//    SerializedProperty _objectColour;

//    private static GUIContent _objectPrompt = new GUIContent("Objective Object", "Provide an ObjectiveObject asset");

//    public override void OnInspectorGUI()
//    {
//        serializedObject.Update();
//        ObjectiveObjectInstance _target = (ObjectiveObjectInstance)target;
//        _objectSO = serializedObject.FindProperty("objectiveObject");
//        EditorGUILayout.PropertyField(_objectSO, _objectPrompt);
//        _objectColour = serializedObject.FindProperty("objectiveColour");
//        EditorGUILayout.PropertyField(_)
//    }

//}
