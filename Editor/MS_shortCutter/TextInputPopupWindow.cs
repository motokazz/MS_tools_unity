using UnityEditor;
using UnityEngine;
using System;

public class TextInputPopupWindow : PopupWindowContent
{
    string text = "";
    Action<string> onSubmit;
    string label;

    public TextInputPopupWindow(string label, Action<string> onSubmit)
    {
        this.label = label;
        this.onSubmit = onSubmit;
    }

    public override Vector2 GetWindowSize()
    {
        return new Vector2(250, 70);
    }

    public override void OnGUI(Rect rect)
    {
        GUILayout.Label(label);

        GUI.SetNextControlName("inputField");
        text = EditorGUILayout.TextField(text);

        GUILayout.Space(8);

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("OK"))
        {
            Submit();
        }

        if (GUILayout.Button("Cancel"))
        {
            editorWindow.Close();
        }

        GUILayout.EndHorizontal();

        HandleKeyboard();

        EditorGUI.FocusTextInControl("inputField");
    }

    void HandleKeyboard()
    {
        Event e = Event.current;

        if (e.type == EventType.KeyDown)
        {
            if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
            {
                Submit();
                e.Use();
            }
            else if (e.keyCode == KeyCode.Escape)
            {
                editorWindow.Close();
                e.Use();
            }
        }
    }

    void Submit()
    {
        onSubmit?.Invoke(text);
        editorWindow.Close();
    }
}
