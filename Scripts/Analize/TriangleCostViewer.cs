using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
/// <summary>
/// トライアングルトップ10を表示
/// スクリプトを空のGameObjectにアタッチ
/// </summary>
/// 
public class TriangleCostViewer : MonoBehaviour
{




    [Header("Settings")]
    [SerializeField] float updateInterval = 1f;
    //
    private float timer;


    [Header("GUISettings")]
    [SerializeField] Vector2 startPosition = new Vector2(10, 10);
    [SerializeField] int fontSize = 20;
    EasyInfoGUI easyInfoGUI = new EasyInfoGUI();

    private void Awake()
    {
        easyInfoGUI.startPosition = startPosition;
        easyInfoGUI.fontSize = fontSize;
    }





    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= updateInterval)
        {
            timer = 0f;
            UpdateTriangleCost();
        }
    }

    void OnGUI()
    {
        easyInfoGUI.Show();
    }

    void UpdateTriangleCost()
    {
        var entries = new List<(string name, int tris)>();

        var renderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);

        foreach (var renderer in renderers)
        {
            if (!renderer.enabled || !renderer.gameObject.activeInHierarchy)
                continue;

            Mesh mesh = GetMeshFromRenderer(renderer);
            if (mesh == null)
                continue;

            int estimatedTris = EstimateTriangleCount(mesh);
            entries.Add((renderer.name, estimatedTris));
        }

        var top10 = entries.OrderByDescending(e => e.tris).Take(10).ToList();

        string output = "<b>Top 10 Estimated Triangle Costs</b>\n";
        foreach (var entry in top10)
        {
            output += $"{entry.tris} tris (estimated):{entry.name}: \n";
        }

        easyInfoGUI.message = output;
    }

    Mesh GetMeshFromRenderer(Renderer renderer)
    {
        if (renderer is SkinnedMeshRenderer smr)
            return smr.sharedMesh;
        if (renderer is MeshRenderer)
            return renderer.GetComponent<MeshFilter>()?.sharedMesh;
        return null;
    }

    int EstimateTriangleCount(Mesh mesh)
    {
        if (mesh == null) return 0;

        return mesh.vertexCount / 2; // 安全な推定
    }
}
