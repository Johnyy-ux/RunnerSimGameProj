using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileObstacle : MonoBehaviour
{
    [Header("Obstacle Prefabs")]
    public List<GameObject> standardObstacles; // Обычные блоки
    public GameObject lowObstaclePrefab;      // Преграда для прыжка
    public List<GameObject> sphereWalls; // Сюда кидаешь все вариации ворот для шара
    public List<GameObject> cubeWalls;   // Сюда — для куба
    public GameObject coinPrefab;

    public Transform[] spawnPoints; // 3 точки спавна

    void Start()
    {
        if (LevelManager.Instance == null) return;

        Random.InitState((int)transform.position.z);

        float z = transform.position.z;
        GamePhase myPhase = DeterminePhase(z);

        // Интегрируем "Остывание": каждая 6-я плитка — фаза отдыха
        if ((int)(z / 10) % 6 == 0)
        {
            GenerateRest();
        }
        else
        {
            GenerateByPhase(myPhase);
        }
    }

    private GamePhase DeterminePhase(float z)
    {
        if (z < 100f) return GamePhase.Tutorial;
        float cyclePos = (z - 100f) % 500f;
        if (cyclePos < 200f) return GamePhase.Standard;
        if (cyclePos < 400f) return GamePhase.ShapeChallenge;
        return GamePhase.Rest;
    }

    private void GenerateByPhase(GamePhase phase)
    {
        switch (phase)
        {
            case GamePhase.Tutorial:
                Spawn(1, coinPrefab);
                break;

            case GamePhase.Rest:
                GenerateRest();
                break;

            case GamePhase.ShapeChallenge:
                SpawnShapePattern();
                break;

            case GamePhase.Standard:
                SpawnRandomPattern();
                break;
        }
    }

    // --- ПАТТЕРНЫ ГЕНЕРАЦИИ ---

    private void SpawnRandomPattern()
    {
        int rand = Random.Range(0, 3);
        if (rand == 0) SpawnJumpPattern();
        else SpawnStandardLayout();
    }

    private void SpawnStandardLayout()
    {
        int safeLane = Random.Range(0, 3);
        float chance = Mathf.Clamp(0.4f + (transform.position.z / 5000f), 0.4f, 0.8f);

        for (int i = 0; i < 3; i++)
        {
            if (i == safeLane) Spawn(i, coinPrefab);
            else if (Random.value < chance) Spawn(i, standardObstacles[Random.Range(0, standardObstacles.Count)]);
        }
    }

    private void SpawnJumpPattern()
    {
        int jumpLane = Random.Range(0, 3);
        Spawn(jumpLane, lowObstaclePrefab);
        // Добавляем монетки над или за преградой, чтобы направить игрока
        Spawn((jumpLane + 1) % 3, coinPrefab);
    }

    private void SpawnShapePattern()
    {
        int targetLane = Random.Range(0, 3);

        // Выбираем тип формы (0 - шар, 1 - куб)
        bool isSphere = Random.value > 0.5f;
        GameObject selectedGate;

        if (isSphere)
        {
            selectedGate = sphereWalls[Random.Range(0, sphereWalls.Count)];
        }
        else
        {
            selectedGate = cubeWalls[Random.Range(0, cubeWalls.Count)];
        }

        Spawn(targetLane, selectedGate);

        // Закрываем остальные пути "непробиваемыми" блоками из standardObstacles
        for (int i = 0; i < 3; i++)
        {
            if (i != targetLane && standardObstacles.Count > 0)
            {
                Spawn(i, standardObstacles[0]);
            }
        }
    }

    private void GenerateRest()
    {
        // Просто дорожка из монет
        int lane = Random.Range(0, 3);
        Spawn(lane, coinPrefab);
    }

    private void Spawn(int index, GameObject prefab)
    {
        if (prefab != null && index < spawnPoints.Length)
        {
            Instantiate(prefab, spawnPoints[index].position, Quaternion.identity, transform);
        }
    }
}
