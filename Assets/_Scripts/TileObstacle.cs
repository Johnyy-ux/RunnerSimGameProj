using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileObstacle : MonoBehaviour
{
    [System.Serializable]
    public struct BiomePool
    {
        public EnvironmentType type;
        public List<GameObject> standardObstacles;
        public List<GameObject> shapeObstacles;
    }

    public List<BiomePool> biomes;
    public GameObject weakWallPrefab;
    public GameObject coinPrefab;
    public Transform[] spawnPoints;

    private List<GameObject> currentStandardPool;
    private List<GameObject> currentShapePool;

    void Start()
    {
        if (LevelManager.Instance == null) return;

        // ГЛАВНАЯ ФИШКА: 
        // Создаем уникальный Seed, который зависит от Номера Уровня И Позиции плитки.
        // Благодаря этому плитка на 100-м метре 1-го уровня ВСЕГДА будет одинаковой.
        int seed = LevelManager.Instance.currentLevel * 1000 + (int)transform.position.z;
        Random.InitState(seed);

        SelectPoolByBiome();
        GenerateByPhase(LevelManager.Instance.currentPhase);
    }

    private void SelectPoolByBiome()
    {
        EnvironmentType currentType = LevelManager.Instance.GetCurrentLocation();

        // Ищем нужный пул в списке биомов
        BiomePool selected = biomes.Find(b => b.type == currentType);

        // Если нашли — берем, если нет — берем первый попавшийся (защита от пустых списков)
        currentStandardPool = (selected.standardObstacles != null && selected.standardObstacles.Count > 0)
            ? selected.standardObstacles : biomes[0].standardObstacles;

        currentShapePool = (selected.shapeObstacles != null && selected.shapeObstacles.Count > 0)
            ? selected.shapeObstacles : biomes[0].shapeObstacles;
    }

    private void GenerateByPhase(GamePhase phase)
    {
        int safeLane = Random.Range(0, 3);

        switch (phase)
        {
            case GamePhase.Tutorial:
                Spawn(safeLane, coinPrefab);
                break;
            case GamePhase.Rest:
                if (Random.value > 0.7f) Spawn(safeLane, coinPrefab);
                break;
            case GamePhase.ShapeChallenge:
                bool wantWeakWall = (LevelManager.Instance.currentLevel > 2 && Random.value > 0.6f);
                SpawnChallenge(safeLane, !wantWeakWall);
                break;
            case GamePhase.Standard:
                SpawnStandardLayout(safeLane);
                break;
        }
    }

    private void SpawnChallenge(int mainLane, bool isShapeWall)
    {
        GameObject challengePrefab = isShapeWall ?
            currentShapePool[Random.Range(0, currentShapePool.Count)] : weakWallPrefab;

        Spawn(mainLane, challengePrefab);

        for (int i = 0; i < 3; i++)
        {
            if (i != mainLane && currentStandardPool.Count > 0)
            {
                Spawn(i, currentStandardPool[0]);
            }
        }
    }

    private void SpawnStandardLayout(int safeLane)
    {
        // Теперь сложность можно привязать к номеру уровня напрямую
        float obstacleChance = Mathf.Clamp(0.4f + (LevelManager.Instance.currentLevel * 0.02f), 0.4f, 0.85f);

        for (int i = 0; i < 3; i++)
        {
            if (i == safeLane)
            {
                if (Random.value > 0.5f) Spawn(i, coinPrefab);
            }
            else
            {
                if (Random.value < obstacleChance)
                {
                    Spawn(i, currentStandardPool[Random.Range(0, currentStandardPool.Count)]);
                }
            }
        }
    }

    private void Spawn(int index, GameObject prefab)
    {
        if (prefab != null && index < spawnPoints.Length)
        {
            Instantiate(prefab, spawnPoints[index].position, Quaternion.identity, transform);
        }
    }
}
