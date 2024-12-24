# Unity Week 5: Two-Dimensional Scene-Building and Path-Finding

In this project, we created a random environment and ensured the player could reach 100 tiles. The base code was provided during class, and we modified it to achieve our goal.

## Game Flow
1. **Random Map Generation**: At the start of the game, a random map is generated and sorted.
2. **Player Placement**: 
   - The player is placed in a random location.
   - If the player spawns on an inaccessible tile, they are moved to another random location until a valid position is found.
   - Once placed, we check if the player can reach at least 100 tiles from their position.
   - If not, the placement process is repeated until the requirement is met.

## Features Added for Our Goal

### Player Placement Logic
The function `TryPlacePlayer()` ensures the player is positioned on a valid tile with at least 100 reachable tiles around them:
```csharp
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
```
### Counting Reachable Tiles
The `CountFreeTilesAround()` function uses BFS to calculate how many tiles the player can reach in a given area:
```csharp
private int CountFreeTilesAround(float centerX, float centerY, int areaSize)
{
    int freeTileCount = 0;

    for (float x = centerX - areaSize; x <= centerX + areaSize; x++)
    {
        for (float y = centerY - areaSize; y <= centerY + areaSize; y++)
        {
            if (x >= 0 && x < gridSize && y >= 0 && y < gridSize && caveGenerator.GetMap()[(int)x, (int)y] == 0)
            {
                // Use BFS to check if the tile is reachable
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
```

Links:

[Itch.io](https://elyasafko.itch.io/week-6-tilemap)
