using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 実際に描画されているオブジェクト（Renderer.isVisible）を対象に、
/// 頂点数（描画負荷）を測定し、トップ10をログ出力する
/// 
/// ～使い方～
/// このスクリプトを任意の GameObject にアタッチ
/// シーンを Playモードで再生
/// 該当オブジェクトの Inspector にある「右クリック → Log Top 10...」を実行
/// 実際に描画されているオブジェクトの描画コスト（頂点数）トップ10が Console に出力される
/// </summary>
/// 
public class VisibleVertexCostAnalyzer : MonoBehaviour
{
    [ContextMenu("Log Top 10 Visible Renderers by Vertex Cost")]
    public void LogTopVisibleVertexObjects()
    {
        Renderer[] renderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);
        var vertexInfoList = new List<(GameObject obj, int vertexCount)>();

        foreach (Renderer renderer in renderers)
        {
            if (!renderer.isVisible)
                continue; // Frustum + Occlusion カリング後の描画対象のみ

            Mesh mesh = null;

            if (renderer is SkinnedMeshRenderer smr)
            {
                mesh = smr.sharedMesh;
            }
            else if (renderer is MeshRenderer)
            {
                var filter = renderer.GetComponent<MeshFilter>();
                if (filter != null)
                    mesh = filter.sharedMesh;
            }

            if (mesh == null)
                continue;

            int triangleCount = 0;

            // Read/Write 非対応でも落ちないように try-catch
            try
            {
                for (int i = 0; i < mesh.subMeshCount; i++)
                {
                    triangleCount += mesh.GetTriangles(i).Length / 3;
                }
            }
            catch
            {
                // fallback: 正確ではないが、vertexCount から推定
                triangleCount = mesh.vertexCount / 3;
            }

            int approxVertexCount = triangleCount * 3;

            vertexInfoList.Add((renderer.gameObject, approxVertexCount));
        }

        var top10 = vertexInfoList
            .OrderByDescending(x => x.vertexCount)
            .Take(10)
            .ToList();

        Debug.Log("=== Top 10 Actually Visible Objects by Estimated Vertex Count ===");
        for (int i = 0; i < top10.Count; i++)
        {
            var entry = top10[i];
            Debug.Log($"{i + 1}. {entry.obj.name} - {entry.vertexCount} vertices (Path: {GetGameObjectPath(entry.obj)})");
        }
    }

    private static string GetGameObjectPath(GameObject obj)
    {
        string path = obj.name;
        while (obj.transform.parent != null)
        {
            obj = obj.transform.parent.gameObject;
            path = obj.name + "/" + path;
        }
        return path;
    }
}
