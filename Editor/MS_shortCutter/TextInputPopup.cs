using UnityEditor;
using UnityEngine;
using System;

public class TextInputPopup : EditorWindow
{
    string text = "";
    Action<string> onSubmit;

    public static void ShowAt(Vector2 screenPos, string title, Action<string> onSubmit)
    {
        var window = CreateInstance<TextInputPopup>();
        window.titleContent = new GUIContent(title);
        window.onSubmit = onSubmit;

        Rect rect = new Rect(screenPos, new Vector2(0, 0));

        // ★ 右クリック位置にドロップダウン表示
        window.ShowAsDropDown(rect, new Vector2(260, 80));
    }

    void OnGUI()
    {
        GUILayout.Label("Input:");
        GUI.SetNextControlName("input");
        text = EditorGUILayout.TextField(text);

        GUILayout.Space(8);

        if (GUILayout.Button("OK"))
        {
            onSubmit?.Invoke(text);
            Close();
        }

        EditorGUI.FocusTextInControl("input");
    }
}
