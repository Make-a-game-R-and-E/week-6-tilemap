using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapCaveGenerator : MonoBehaviour
{
    [SerializeField] private Tilemap tilemap;

    [Tooltip("The tile that represents a wall (an impassable block)")]
    [SerializeField] private TileBase wallTile;

    [Tooltip("The tile that represents a floor (a passable block)")]
    [SerializeField] private TileBase floorTile;

    [Tooltip("The percent of walls in the initial random map")]
    [Range(0, 1)]
    [SerializeField] private float randomFillPercent = 0.5f;

    [Tooltip("Length and height of the grid")]
    [SerializeField] private int gridSize = 100;

    [Tooltip("How many steps do we want to simulate?")]
    [SerializeField] private int simulationSteps = 20;

    [Tooltip("For how long will we pause between each simulation step so we can look at the result?")]
    [SerializeField] private float pauseTime = 1f;

    [Tooltip("Player GameObject")]
    [SerializeField] private GameObject player;

    [Tooltip("Maximum number of iterations before BFS algorithm gives up on finding a path")]
    [SerializeField] private int maxIterations = 1000;

    [SerializeField] private AllowedTiles allowedTiles;

    private CaveGenerator caveGenerator;
    private TilemapGraph tilemapGraph = null;


    private void Start()
    {
        UnityEngine.Random.InitState(100);
        caveGenerator = new CaveGenerator(randomFillPercent, gridSize);
        caveGenerator.RandomizeMap();
        GenerateAndDisplayMap(caveGenerator.GetMap());
        tilemapGraph = new TilemapGraph(tilemap, allowedTiles.Get());

        SimulateCavePattern();
    }

    private async void SimulateCavePattern()
    {
        for (int i = 0; i < simulationSteps; i++)
        {
            await Awaitable.WaitForSecondsAsync(pauseTime);
            caveGenerator.SmoothMap();
            GenerateAndDisplayMap(caveGenerator.GetMap());
        }
        Debug.Log("Simulation completed!");
        TryPlacePlayer();
    }

    private void GenerateAndDisplayMap(int[,] map)
    {
        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                var position = new Vector3Int(x, y, 0);
                var tile = map[x, y] == 1 ? wallTile : floorTile;
                tilemap.SetTile(position, tile);
            }
        }
    }

    private void TryPlacePlayer()
    {
        int areaSize = 1;
        float x = UnityEngine.Random.Range(0, gridSize);
        float y = UnityEngine.Random.Range(0, gridSize);
        player.transform.position = new Vector3(x + 0.5f, y + 0.5f, 0); // Place the player at the random position

        while (true)
        {
            if (caveGenerator.GetMap()[(int)x, (int)y] == 0)
            {
                int reachableTiles = CountFreeTilesAround(x, y, areaSize);

                Debug.Log($"Free tiles in area size {areaSize}: {reachableTiles}");

                if (reachableTiles >= 100)
                {
                    Debug.Log("Player successfully placed.");
                    return;
                }

                areaSize++;
            }
            else
            {
                x = UnityEngine.Random.Range(0, gridSize);
                y = UnityEngine.Random.Range(0, gridSize);
                player.transform.position = new Vector3(x + 0.5f, y + 0.5f, 0);
                Debug.Log("Player placed at a new random position - " + "x:" + x + " Y:" + y);
            }
        }
    }

    private int CountFreeTilesAround(float centerX, float centerY, int areaSize)
    {
        int freeTileCount = 0;

        for (float x = centerX - areaSize; x <= centerX + areaSize; x++)
        {
            for (float y = centerY - areaSize; y <= centerY + areaSize; y++)
            {
                if (x >= 0 && x < gridSize && y >= 0 && y < gridSize && caveGenerator.GetMap()[(int)x, (int)y] == 0)
                {
                    // use bfs to check if the tile is reachable
                    Vector3Int startNode = tilemap.WorldToCell(new Vector3(centerX, centerY, 0));
                    Vector3Int endNode = tilemap.WorldToCell(new Vector3(x, y, 0));
                    List<Vector3Int> shortestPath = BFS.GetPath(tilemapGraph, startNode, endNode, maxIterations);
                    Debug.Log("shortestPath = " + string.Join(" , ", shortestPath));
                    if (shortestPath.Count >= 2)
                    {
                        freeTileCount++;
                    }
                }
            }
        }

        return freeTileCount;
    }
}
