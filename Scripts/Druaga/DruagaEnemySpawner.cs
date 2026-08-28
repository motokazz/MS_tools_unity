using UnityEngine;
using System;
using System.Collections.Generic;

public class DruagaEnemySpawner : MonoBehaviour
{
    public MazeGenerator_simple mazeGenerator;
    public GameObject slimePrefab;
    public int slimeCount = 5;
    public float cellSize = 1f;

    void OnEnable()
    {
        mazeGenerator.OnMazeGenerated += SpawnSlimes;
    }

    void OnDisable()
    {
        mazeGenerator.OnMazeGenerated -= SpawnSlimes;
    }

    void SpawnSlimes(int[,] maze)
    {
        int width = maze.GetLength(0);
        int height = maze.GetLength(1);

        List<Vector2Int> floorCells = new List<Vector2Int>();

        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                if (maze[x, y] == 1)
                {
                    floorCells.Add(new Vector2Int(x, y));
                }
            }
        }

        for (int i = 0; i < slimeCount && floorCells.Count > 0; i++)
        {
            int index = UnityEngine.Random.Range(0, floorCells.Count);
            Vector2Int cell = floorCells[index];
            floorCells.RemoveAt(index);

            Vector3 pos = new Vector3(
                cell.x * cellSize,
                0,
                cell.y * cellSize
            );

            Instantiate(slimePrefab, pos, Quaternion.identity);
        }
    }
}
