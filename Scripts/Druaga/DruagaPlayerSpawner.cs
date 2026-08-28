using UnityEngine;
using System;
using System.Collections.Generic;

public class DruagaPlayerSpawner : MonoBehaviour
{
    public MazeGenerator mazeGenerator;
    public MS_PlayerController playerController;
    public float cellSize = 1f;

    [Header("Spawn Settings")]
    public Vector2Int startCell = new Vector2Int(1, 1);
    public int safeRadius = 3;

    GameObject currentPlayer;

    void Awake()
    {
        if (!mazeGenerator)
            mazeGenerator = FindObjectOfType<MazeGenerator>();
    }

    void OnEnable()
    {
        mazeGenerator.OnMazeGenerated += SpawnPlayer;
    }

    void OnDisable()
    {
        mazeGenerator.OnMazeGenerated -= SpawnPlayer;
    }

    private void Start()
    {
    }

    void SpawnPlayer(int[,] maze)
    {
        // 既存プレイヤー削除（再生成対応）
        //if (currentPlayer)
        //    Destroy(currentPlayer);

        Vector2Int spawnCell = FindValidStartCell(maze);

        Vector3 pos = new Vector3(
            spawnCell.x * cellSize,
            0,
            spawnCell.y * cellSize
        );

        //currentPlayer = Instantiate(playerPrefab, pos, Quaternion.identity);

        playerController.SetTransform(pos);//PlayerInputRefferenceのバグ対応
    }

    Vector2Int FindValidStartCell(int[,] maze)
    {
        int width = maze.GetLength(0);
        int height = maze.GetLength(1);

        // 基本は (1,1) 付近
        for (int r = 0; r <= safeRadius; r++)
        {
            for (int dx = -r; dx <= r; dx++)
            {
                for (int dy = -r; dy <= r; dy++)
                {
                    
                    //int x = startCell.x + dx;
                    //int y = startCell.y + dy;

                    int x = UnityEngine.Random.Range(startCell.x,width) + dx;
                    int y = UnityEngine.Random.Range(startCell.y, height) + dy;

                    if (x <= 0 || y <= 0 || x >= width - 1 || y >= height - 1)
                        continue;

                    if (maze[x, y] == 1)
                        return new Vector2Int(x, y);
                }
            }
        }

        // 念のための保険
        return startCell;
    }
}
