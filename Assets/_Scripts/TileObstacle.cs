using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileObstacle : MonoBehaviour
{
    public List<GameObject> standardObstacles;
    public List<GameObject> shapeWalls;
    public GameObject coinPrefab;
    public Transform[] spawnPoints;

    void Start()
    {
        if (LevelManager.Instance == null) return;

        Random.InitState((int)transform.position.z);

        // Определяем фазу плитки на основе её собственного Z, а не позиции игрока
        GamePhase myPhase;
        float z = transform.position.z;

        if (z < 100f) myPhase = GamePhase.Tutorial;
        else
        {
            float cyclePos = (z - 100f) % 500f;
            if (cyclePos < 200f) myPhase = GamePhase.Standard;
            else if (cyclePos < 400f) myPhase = GamePhase.ShapeChallenge;
            else myPhase = GamePhase.Rest;
        }

        GenerateByPhase(myPhase);
    }

    private void GenerateByPhase(GamePhase phase)
    {
        int safeLane = Random.Range(0, 3);

        switch (phase)
        {
            case GamePhase.Tutorial:
                // В туториале только монетки в центре
                if (Random.value > 0.5f) Spawn(1, coinPrefab);
                break;

            case GamePhase.Rest:
                // В фазе отдыха много монет и нет преград
                Spawn(safeLane, coinPrefab);
                if (Random.value > 0.7f) Spawn((safeLane + 1) % 3, coinPrefab);
                break;

            case GamePhase.ShapeChallenge:
                // Только стены с отверстиями
                SpawnShapeChallenge(safeLane);
                break;

            case GamePhase.Standard:
                // Обычный геймплей с блоками
                SpawnStandardLayout(safeLane);
                break;
        }
    }

    private void SpawnShapeChallenge(int mainLane)
    {
        if (shapeWalls.Count > 0)
        {
            Spawn(mainLane, shapeWalls[Random.Range(0, shapeWalls.Count)]);
        }
        // Перекрываем остальные пути обычными блоками
        for (int i = 0; i < 3; i++)
        {
            if (i != mainLane && standardObstacles.Count > 0)
                Spawn(i, standardObstacles[0]);
        }
    }

    private void SpawnStandardLayout(int safeLane)
    {
        // Прогрессивная сложность от дистанции
        float chance = Mathf.Clamp(0.4f + (transform.position.z / 4000f), 0.4f, 0.8f);

        for (int i = 0; i < 3; i++)
        {
            if (i == safeLane)
            {
                if (Random.value > 0.8f) Spawn(i, coinPrefab);
            }
            else if (Random.value < chance && standardObstacles.Count > 0)
            {
                Spawn(i, standardObstacles[Random.Range(0, standardObstacles.Count)]);
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
