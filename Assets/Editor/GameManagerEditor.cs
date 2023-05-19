using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CustomEditor(typeof(GameManager))]
public class GameManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        #if UNITY_EDITOR
        DrawDefaultInspector();

        using (new EditorGUI.DisabledScope(!Application.isPlaying))
        {
            if (GUILayout.Button("Load Next Round"))
            {
                GameManager.Instance.LoadRoundButton();
            }
        }
        #endif
    }
}
