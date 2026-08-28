using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Net.NetworkInformation;
using UnityEngine.UI;
using UnityEditor.SceneManagement;

public class MS_renamer : EditorWindow
{

	string source;
	string search;
	string replace;
    string prefix;
    string suffix;


    [MenuItem("MS_Tools/Assets/MS_renamer")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(MS_renamer));

    }
	void OnGUI()
	{

		// EditorGUILayoutの使用例.
		EditorGUILayout.LabelField("MS_renamer");
        GUILayout.BeginVertical();


        // Prefix
        prefix = EditorGUILayout.TextField("AddPrefix", prefix);
        if (GUILayout.Button("AddPrefix", GUILayout.Width(100), GUILayout.Height(30)))
        {
            AddPrefix();
        }


        // Replace
        search = EditorGUILayout.TextField("SearchWord",search);
		replace = EditorGUILayout.TextField("Replace", replace);
        if(GUILayout.Button("Replace",GUILayout.Width ( 100),GUILayout.Height(30)))
        {
            Renamer();
        }

        // ToLowerCase
        if (GUILayout.Button("ToLowerCase", GUILayout.Width(100), GUILayout.Height(30)))
        {
            ToLowerCase();
        }

        // Suffix
        suffix = EditorGUILayout.TextField("AddSuffix", suffix);
        if (GUILayout.Button("AddSuffix", GUILayout.Width(100), GUILayout.Height(30)))
        {
            AddSuffix();
        }

        GUILayout.Label("RenameByBaseAssetName");
        // RenameByBaseAssetName
        if (GUILayout.Button("RenameByBaseAssetName", GUILayout.Width(200), GUILayout.Height(30)))
        {
            RenameByBaseAssetName();
        }

        GUILayout.Label("RenameByMeshName");
        // RenameByBaseAssetName
        if (GUILayout.Button("RenameByMeshName", GUILayout.Width(200), GUILayout.Height(30)))
        {
            RenameByMeshName();
        }

        GUILayout.EndVertical();



        void Renamer()
        {
			Object[] GOS = Selection.objects;
			foreach (Object GO in GOS)
            {
				string path = AssetDatabase.GetAssetPath(GO);
				ReplaceAssetName(path, search, replace);
            }
			GameObject[] gameObjects = Selection.gameObjects;
			foreach(GameObject GO in gameObjects)
            {
				GO.name = GO.name.Replace(search, replace);
            }
        }

        void ToLowerCase()
        {
            Object[] GOS = Selection.objects;
            foreach (Object GO in GOS)
            {
                string path = AssetDatabase.GetAssetPath(GO);
                Lower(path);
            }
            GameObject[] gameObjects = Selection.gameObjects;
            foreach (GameObject GO in gameObjects)
            {
                GO.name = GO.name.ToLower();
            }
        }

        void AddSuffix()
        {
            Object[] GOS = Selection.objects;
            foreach (Object GO in GOS)
            {
                string path = AssetDatabase.GetAssetPath(GO);

                var fileName = System.IO.Path.GetFileNameWithoutExtension(path);
                var renameFileName = fileName+suffix;
                AssetDatabase.RenameAsset(path, renameFileName);
            }
            GameObject[] gameObjects = Selection.gameObjects;
            foreach (GameObject GO in gameObjects)
            {
                GO.name = GO.name+suffix;
            }
        }

        void AddPrefix()
        {
            Object[] GOS = Selection.objects;
            foreach (Object GO in GOS)
            {
                string path = AssetDatabase.GetAssetPath(GO);

                var fileName = System.IO.Path.GetFileNameWithoutExtension(path);
                var renameFileName = prefix+fileName;
                AssetDatabase.RenameAsset(path, renameFileName);
            }
            GameObject[] gameObjects = Selection.gameObjects;
            foreach (GameObject GO in gameObjects)
            {
                GO.name = GO.name + suffix;
            }
        }

        void RenameByBaseAssetName()
        {
            GameObject[] gameObjects = Selection.gameObjects;
            foreach(GameObject GO in gameObjects)
            {
                if(PrefabUtility.GetCorrespondingObjectFromSource(GO) != null)
                {
                    GO.name = PrefabUtility.GetCorrespondingObjectFromSource(GO).name;
                }
            }
        }

        void RenameByMeshName()
        {
            GameObject[] gameObjects = Selection.gameObjects;
            foreach (GameObject GO in gameObjects)
            {
                if (GO.GetComponent<MeshFilter>() != null)
                {
                    var meshFilter = GO.GetComponent<MeshFilter>();
                    GO.name = meshFilter.sharedMesh.name;
                }
            }
            SaveEditedPrefab();
        }



        void SaveEditedPrefab()
        {
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage == null)
            {
                //Debug.LogWarning("現在、Prefab編集モードではありません。");
                return;
            }
            else
            {
                // 変更を保存
                EditorSceneManager.MarkSceneDirty(prefabStage.scene);
                AssetDatabase.SaveAssets();
                EditorSceneManager.SaveScene(prefabStage.scene);
            }
        }






        ////////
        // core
        ////////
        static void ReplaceAssetName(string targetPass, string searchWord, string replaceWord)
		{
			var fileName = System.IO.Path.GetFileNameWithoutExtension(targetPass);
			var renameFileName = fileName.Replace(searchWord, replaceWord);
			AssetDatabase.RenameAsset(targetPass, renameFileName);
		}

        static void Lower(string targetPass)
        {
            var fileName = System.IO.Path.GetFileNameWithoutExtension(targetPass);
            var renameFileName = fileName.ToLower();
            AssetDatabase.RenameAsset(targetPass, renameFileName);
        }


    }
}
