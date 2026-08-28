using UnityEditor;
using UnityEngine;

public class CheckClassType : Editor
{
    [MenuItem("Window/MS_Tools/Check Select Object")]
    private static void CheckSelectObject()
    {
        Debug.Log(Selection.activeObject.GetType());
    }
}
