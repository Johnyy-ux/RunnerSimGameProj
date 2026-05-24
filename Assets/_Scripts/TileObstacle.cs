using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileObstacle : MonoBehaviour
{
    [Header("Fallback Prefabs (if theme doesn't provide)")]
    public List<GameObject> standardObstacles = new();
    public List<GameObject> sphereWalls = new();
    public List<GameObject> cubeWalls = new();
    public GameObject coinPrefab;

    [Header("Spawn Points (must have exactly 3)")]
    public Transform[] spawnPoints = new Transform[3];

    private readonly List<GameObject> spawnedObjects = new List<GameObject>();
    private TileManager tileManager;

    // ── Public ───────────────────────────────────────────────────────────────
    public void Initialize(TileManager manager, bool isRestTile)
    {
        tileManager = manager;
        ClearSpawnedObjects();

        if (isRestTile)
        {
            SpawnRestTile();
            return;
        }

        switch (GetCurrentPhase())
        {
            case GamePhase.Tutorial:
            case GamePhase.Rest:
                SpawnRestTile();
                break;
            case GamePhase.Standard:
                SpawnStandard();
                break;
            case GamePhase.ShapeChallenge:
                SpawnShapeGate();
                break;
        }
    }

    public void ClearSpawnedObjects()
    {
        foreach (var obj in spawnedObjects)
            if (obj != null) Destroy(obj);

        spawnedObjects.Clear();
    }

    // ── Theme Prefabs ────────────────────────────────────────────────────────
    private List<GameObject> GetStandardObstacles()
    {
        var theme = ThemeManager.Instance?.ActiveTheme;
        return theme?.standardObstacles?.Count > 0 ? theme.standardObstacles : standardObstacles;
    }

    private List<GameObject> GetSphereWalls()
    {
        var theme = ThemeManager.Instance?.ActiveTheme;
        return theme?.sphereWalls?.Count > 0 ? theme.sphereWalls : sphereWalls;
    }

    private List<GameObject> GetCubeWalls()
    {
        var theme = ThemeManager.Instance?.ActiveTheme;
        return theme?.cubeWalls?.Count > 0 ? theme.cubeWalls : cubeWalls;
    }

    private GameObject GetCoinPrefab()
    {
        var theme = ThemeManager.Instance?.ActiveTheme;
        return theme?.coinPrefab != null ? theme.coinPrefab : coinPrefab;
    }

    // ── Phase Logic ──────────────────────────────────────────────────────────
    private GamePhase GetCurrentPhase()
    {
        var lm = LevelManager.Instance;
        if (lm == null || lm.distanceTravelled < 100f)
            return GamePhase.Tutorial;

        float cycle = (lm.distanceTravelled - 100f) % 500f;
        if (cycle < 200f) return GamePhase.Standard;
        if (cycle < 400f) return GamePhase.ShapeChallenge;
        return GamePhase.Rest;
    }

    private float GetDifficulty() =>
        LevelManager.Instance == null ? 0f : Mathf.Clamp01(LevelManager.Instance.distanceTravelled / 3000f);

    // ── Spawn Patterns ───────────────────────────────────────────────────────
    private void SpawnRestTile()
    {
        SpawnCoinInRandomLane();
        ReportAllLanesSafe();
    }

    private void SpawnStandard()
    {
        var obstacles = GetStandardObstacles();
        int safeLane = ChooseSafeLane();
        int maxObstacles = GetDifficulty() > 0.5f ? 2 : 1;
        int placed = 0;

        for (int i = 0; i < 3; i++)
        {
            if (i == safeLane)
            {
                Spawn(i, GetCoinPrefab());
                tileManager?.ReportSafeLane(i);
            }
            else if (placed < maxObstacles && obstacles.Count > 0)
            {
                Spawn(i, obstacles[Random.Range(0, obstacles.Count)]);
                tileManager?.ReportObstacleInLane(i);
                placed++;
            }
            else
            {
                tileManager?.ReportSafeLane(i);
            }
        }
    }

    private void SpawnShapeGate()
    {
        bool useSphere = Random.value > 0.5f;
        var gates = useSphere ? GetSphereWalls() : GetCubeWalls();

        if (gates.Count == 0)
        {
            SpawnStandard();
            return;
        }

        int gateLane = Random.Range(0, 3);
        Spawn(gateLane, gates[Random.Range(0, gates.Count)]);
        tileManager?.ReportSafeLane(gateLane);

        var obstacles = GetStandardObstacles();
        for (int i = 0; i < 3; i++)
        {
            if (i == gateLane || obstacles.Count == 0) continue;
            Spawn(i, obstacles[Random.Range(0, obstacles.Count)]);
            tileManager?.ReportObstacleInLane(i);
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────
    private void SpawnCoinInRandomLane()
    {
        int lane = Random.Range(0, 3);
        Spawn(lane, GetCoinPrefab());
        tileManager?.ReportSafeLane(lane);
    }

    private int ChooseSafeLane()
    {
        var forced = new List<int>();
        for (int i = 0; i < 3; i++)
            if (tileManager?.IsLaneForcedSafe(i) == true)
                forced.Add(i);

        return forced.Count > 0 ? forced[Random.Range(0, forced.Count)] : Random.Range(0, 3);
    }

    private void ReportAllLanesSafe()
    {
        for (int i = 0; i < 3; i++)
            tileManager?.ReportSafeLane(i);
    }

    private void Spawn(int laneIndex, GameObject prefab)
    {
        if (prefab == null || laneIndex >= spawnPoints.Length || spawnPoints[laneIndex] == null)
            return;

        GameObject instance = Instantiate(prefab, spawnPoints[laneIndex].position, Quaternion.identity, transform);
        spawnedObjects.Add(instance);
    }
}
