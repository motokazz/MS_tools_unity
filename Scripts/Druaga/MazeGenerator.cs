using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 迷路生成
/// 空のGameObject作成
/// MazeGenerator をアタッチ WallPrefab（Cubeとか）FloorPrefab（Plane or Quad）
/// Width / Height は奇数推奨 21 × 21 31 × 31 など
/// 再生 > 自動生成！
/// </summary>
public class MazeGenerator : MonoBehaviour
{
    [Header("Maze Size (odd numbers recommended)")]
    public int width = 21;
    public int height = 21;

    [Header("Prefabs")]
    public GameObject wallPrefab;
    public GameObject floorPrefab;

    [Header("Cell Size")]
    public float cellSize = 1f;

    int[,] maze; // 0 = wall, 1 = floor
    public event Action<int[,]> OnMazeGenerated;

    Vector2Int[] directions =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

    void Start()
    {
        Generate();
        Build();
        // ★ ここで通知
        OnMazeGenerated?.Invoke(maze);
    }

    void Generate()
    {
        maze = new int[width, height];

        // 全部壁で初期化
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                maze[x, y] = 0;

        // 開始地点（奇数座標）
        Dig(1, 1);
    }

    void Dig(int x, int y)
    {
        maze[x, y] = 1;

        List<Vector2Int> dirs = new List<Vector2Int>(directions);
        Shuffle(dirs);

        foreach (var dir in dirs)
        {
            int nx = x + dir.x * 2;
            int ny = y + dir.y * 2;

            if (IsInRange(nx, ny) && maze[nx, ny] == 0)
            {
                // 壁を壊す
                maze[x + dir.x, y + dir.y] = 1;
                Dig(nx, ny);
            }
        }
    }

    bool IsInRange(int x, int y)
    {
        return x > 0 && y > 0 && x < width - 1 && y < height - 1;
    }

    void Shuffle(List<Vector2Int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int r = UnityEngine.Random.Range(i, list.Count);
            (list[i], list[r]) = (list[r], list[i]);
        }
    }

    void Build()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // 迷路の配列上の (1, 1) がワールド座標の (0, 0, 0) になるように調整
                Vector3 pos = new Vector3((x - 1) * cellSize, 0, (y - 1) * cellSize);

                GameObject instance;

                if (maze[x, y] == 0)
                {
                    instance = Instantiate(wallPrefab, pos, Quaternion.identity, transform);
                }
                else
                {
                    instance = Instantiate(floorPrefab, pos, Quaternion.identity, transform);
                }

                // CellSizeに合わせてPrefabをスケーリング
                instance.transform.localScale = new Vector3(cellSize, cellSize, cellSize);
            }
        }
    }

    public void Regenerate()
    {
        // 再生成時に前の迷路が残らないよう、子オブジェクトを削除する処理を追加（推奨）
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        Generate();
        Build();
        OnMazeGenerated?.Invoke(maze);
    }
}