using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[CustomEditor(typeof(ChainMaker))]
public class ChainMakerEditor : Editor
{
    public override void OnInspectorGUI()
    {
#if UNITY_EDITOR
        DrawDefaultInspector();
        serializedObject.Update();
        ChainMaker _target = (ChainMaker)target;

        PrefabStage prefabStage = PrefabStageUtility.GetPrefabStage(_target.gameObject);


        using (new EditorGUI.DisabledScope(Application.isPlaying || (prefabStage == null)))
        {
            if (GUILayout.Button("Add One Link"))
            {
                _target.AddOneLink();
                serializedObject.Update();
                serializedObject.SetIsDifferentCacheDirty();
                EditorUtility.SetDirty(_target);
            }

            if (GUILayout.Button("Add Links"))
            {
                _target.AddLinks();
                serializedObject.Update();
                serializedObject.SetIsDifferentCacheDirty();
                EditorUtility.SetDirty(_target);
            }

            if (GUILayout.Button("Recreate Links"))
            {
                _target.RecreateLinks();
                serializedObject.Update();
                serializedObject.SetIsDifferentCacheDirty();
                EditorUtility.SetDirty(_target);
            }
        }

        if (prefabStage == null)
        {
            GUIStyle guiStyle = new GUIStyle(EditorStyles.textField);
            guiStyle.normal.textColor = Color.red;
            GUILayout.Label("Can only modifiy links in Prefab Editor mode", guiStyle);
        }
#endif
    }
}
