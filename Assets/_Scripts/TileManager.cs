using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    [Header("Tile Pools")]
    public GameObject[] proceduralTiles; // Плитки со скриптом TileObstacle
    public GameObject[] challengeTiles;  // Заранее собранные сложные плитки

    [Header("Settings")]
    public Transform playerTransform;
    public float tileLength = 10f;
    public int numberOfTiles = 6;
    public int backBuffer = 2;

    [Range(0, 1)]
    public float challengeChance = 0.15f; // Шанс появления ручного челленджа

    private float spawnZ = 0f;
    private List<GameObject> activeTiles = new List<GameObject>();

    void Start()
    {
        if (playerTransform == null) playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        for (int i = 0; i < numberOfTiles; i++)
        {
            // Первые 3 плитки всегда пустые (продукт из proceduralTiles[0] или просто пустые)
            SpawnTile(i < 3);
        }
    }

    void Update()
    {
        if (playerTransform == null) return;

        if (playerTransform.position.z > spawnZ - (numberOfTiles * tileLength))
        {
            SpawnTile(false);
            if (activeTiles.Count > numberOfTiles + backBuffer)
            {
                DeleteTile();
            }
        }
    }

    public void SpawnTile(bool forceEmpty)
    {
        GameObject prefab;

        if (forceEmpty)
        {
            prefab = proceduralTiles[0]; // Предполагаем, что индекс 0 — пустая плитка
        }
        else
        {
            // Выбираем между рандомом и челленджем
            if (Random.value < challengeChance && challengeTiles.Length > 0)
            {
                prefab = challengeTiles[Random.Range(0, challengeTiles.Length)];
            }
            else
            {
                prefab = proceduralTiles[Random.Range(0, proceduralTiles.Length)];
            }
        }

        GameObject go = Instantiate(prefab, Vector3.forward * spawnZ, Quaternion.identity, transform);
        activeTiles.Add(go);
        spawnZ += tileLength;
    }

    private void DeleteTile()
    {
        if (activeTiles.Count > 0)
        {
            Destroy(activeTiles[0]);
            activeTiles.RemoveAt(0);
        }
    }
}
