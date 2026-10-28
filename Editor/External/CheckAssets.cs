using UnityEditor;
using UnityEngine;

public class CheckClassType : Editor
{
    [MenuItem("MS_Tools/Assets/Check Select Object")]
    private static void CheckSelectObject()
    {
        Debug.Log(Selection.activeObject.GetType());
    }
}
