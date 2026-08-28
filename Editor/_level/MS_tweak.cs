using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class TransformTweakTool : EditorWindow
{
    private float value = 0.005f;
    private bool useLocalSpace = false;

    [MenuItem("MS_Tools/Level/Transform Tweak Tool")]
    public static void ShowWindow()
    {
        GetWindow<TransformTweakTool>("Transform Tweak");
    }

    private void OnGUI()
    {
        GUILayout.Label("Transform Tweak Tool", EditorStyles.boldLabel);

        value = EditorGUILayout.FloatField("value",value);
        useLocalSpace = GUILayout.Toggle(useLocalSpace, "Use Local Space");

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("+tx"))
        {
            Vector3 v = new Vector3(value, 0, 0);
            MoveSelectedObjects(v);
        }

        if (GUILayout.Button("+ty"))
        {
            Vector3 v = new Vector3(0, value, 0);
            MoveSelectedObjects(v);
        }

        if (GUILayout.Button("+tz"))
        {
            Vector3 v = new Vector3(0,0,value);
            MoveSelectedObjects(v);
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("-tx"))
        {
            Vector3 v = new Vector3(-value, 0, 0);
            MoveSelectedObjects(v);
        }

        if (GUILayout.Button("-ty"))
        {
            Vector3 v = new Vector3(0, -value, 0);
            MoveSelectedObjects(v);
        }

        if (GUILayout.Button("-tz"))
        {
            Vector3 v = new Vector3(0, 0, -value);
            MoveSelectedObjects(v);
        }
        GUILayout.EndHorizontal();

    }

    private void MoveSelectedObjects(Vector3 v)
    {
        foreach (var obj in Selection.transforms)
        {
            Undo.RecordObject(obj, "Transform Tweak Move");

            if (useLocalSpace)
            {
                obj.position += obj.TransformDirection(v);
            }
            else
            {
                obj.position += v;
            }

            EditorUtility.SetDirty(obj);
        }
    }
}
