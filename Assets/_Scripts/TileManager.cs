using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    [Header("Tile Prefabs")]
    public GameObject[] proceduralTiles;
    public GameObject[] challengeTiles;

    [Header("Settings")]
    public Transform playerTransform;
    public float tileLength = 10f;
    public int tilesAhead = 8;
    [Range(0f, 1f)] public float challengeChance = 0.15f;

    private const int REST_INTERVAL = 6;
    private const int MAX_LANE_STREAK = 2;

    // Pooling
    private readonly Dictionary<int, Queue<GameObject>> proceduralPool = new();
    private readonly Dictionary<int, Queue<GameObject>> challengePool = new();
    private readonly List<ActiveTile> activeTiles = new();

    private float spawnZ = 0f;
    private int tilesSinceRest = 0;
    private readonly int[] laneStreak = new int[3];

    private struct ActiveTile
    {
        public GameObject go;
        public int poolIndex; // >=0 = procedural, <0 = challenge
    }

    void Start()
    {
        if (playerTransform == null)
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        PreWarmPools(4);

        // Initial safe tiles
        for (int i = 0; i < tilesAhead; i++)
            SpawnTile(forceEmpty: i < 3);
    }

    void Update()
    {
        if (playerTransform == null) return;

        while (playerTransform.position.z > spawnZ - (tilesAhead * tileLength))
        {
            SpawnTile(forceEmpty: false);
            RecycleOldestTiles();
        }
    }

    private void PreWarmPools(int count)
    {
        PreWarmPool(proceduralPool, proceduralTiles, count);
        PreWarmPool(challengePool, challengeTiles, count);
    }

    private void PreWarmPool(Dictionary<int, Queue<GameObject>> pool, GameObject[] prefabs, int count)
    {
        for (int i = 0; i < prefabs.Length; i++)
        {
            if (!pool.ContainsKey(i)) pool[i] = new Queue<GameObject>();
            for (int j = 0; j < count; j++)
                pool[i].Enqueue(CreateInactive(prefabs[i]));
        }
    }

    private GameObject CreateInactive(GameObject prefab)
    {
        GameObject go = Instantiate(prefab, Vector3.zero, Quaternion.identity, transform);
        go.SetActive(false);
        return go;
    }

    public void SpawnTile(bool forceEmpty)
    {
        bool isRestTile = false;
        int procIndex = 0;
        int challIndex = -1;

        if (!forceEmpty)
        {
            if (tilesSinceRest >= REST_INTERVAL)
            {
                isRestTile = true;
                ResetStreaks();
            }
            else if (Random.value < challengeChance && challengeTiles.Length > 0)
            {
                challIndex = Random.Range(0, challengeTiles.Length);
                tilesSinceRest++;
            }
            else
            {
                procIndex = Random.Range(0, proceduralTiles.Length);
                tilesSinceRest++;
            }
        }

        GameObject tile = GetTileFromPool(challIndex, procIndex);
        tile.transform.position = new Vector3(0, 0, spawnZ);
        tile.SetActive(true);

        if (!forceEmpty)
            tile.GetComponent<TileObstacle>()?.Initialize(this, isRestTile);

        int poolIndex = challIndex >= 0 ? -(challIndex + 1) : procIndex;
        activeTiles.Add(new ActiveTile { go = tile, poolIndex = poolIndex });

        spawnZ += tileLength;
    }

    private GameObject GetTileFromPool(int challIndex, int procIndex)
    {
        if (challIndex >= 0)
            return GetFromPool(challengePool, challIndex, challengeTiles);

        return GetFromPool(proceduralPool, procIndex, proceduralTiles);
    }

    private GameObject GetFromPool(Dictionary<int, Queue<GameObject>> poolDict, int index, GameObject[] prefabs)
    {
        if (!poolDict.ContainsKey(index) || poolDict[index].Count == 0)
            return CreateInactive(prefabs[index]);

        return poolDict[index].Dequeue();
    }

    private void RecycleOldestTiles()
    {
        while (activeTiles.Count > tilesAhead + 4)
        {
            ReturnToPool(activeTiles[0]);
            activeTiles.RemoveAt(0);
        }
    }

    private void ReturnToPool(ActiveTile tile)
    {
        tile.go.GetComponent<TileObstacle>()?.ClearSpawnedObjects();
        tile.go.SetActive(false);
        tile.go.transform.position = Vector3.zero;

        if (tile.poolIndex >= 0)
            proceduralPool[tile.poolIndex].Enqueue(tile.go);
        else
            challengePool[-(tile.poolIndex + 1)].Enqueue(tile.go);
    }

    // Lane streak system
    public void ReportObstacleInLane(int lane) => laneStreak[lane] = Mathf.Min(laneStreak[lane] + 1, MAX_LANE_STREAK + 1);
    public void ReportSafeLane(int lane) => laneStreak[lane] = 0;
    public bool IsLaneForcedSafe(int lane) => laneStreak[lane] >= MAX_LANE_STREAK;

    private void ResetStreaks()
    {
        System.Array.Clear(laneStreak, 0, laneStreak.Length);
        tilesSinceRest = 0;
    }
}
