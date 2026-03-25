using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    public GameObject[] tilePrefabs;
    public GameObject finishTilePrefab; // Плитка с финишной линией
    public Transform playerTransform;

    public float tileLength = 10f;
    public int numberOfTiles = 6;

    private float spawnZ = 0f;
    private List<GameObject> activeTiles = new List<GameObject>();
    private bool finishSpawned = false;

    void Start()
    {
        if (playerTransform == null) playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        for (int i = 0; i < numberOfTiles; i++)
        {
            SpawnTile(i == 0 ? 0 : Random.Range(0, tilePrefabs.Length));
        }
    }

    void Update()
    {
        if (playerTransform == null || finishSpawned) return;

        if (playerTransform.position.z - tileLength > spawnZ - (numberOfTiles * tileLength))
        {
            // ПРОВЕРКА: Если следующая плитка уже за пределом дистанции уровня
            if (spawnZ < LevelManager.Instance.levelDistance)
            {
                SpawnTile(Random.Range(0, tilePrefabs.Length));

                if (activeTiles.Count > numberOfTiles + 2)
                {
                    DeleteTile();
                }
            }
            else
            {
                SpawnFinishTile();
            }
        }
    }

    public void SpawnTile(int tileIndex)
    {
        GameObject go = Instantiate(tilePrefabs[tileIndex], Vector3.forward * spawnZ, Quaternion.identity, transform);
        activeTiles.Add(go);
        spawnZ += tileLength;
    }

    private void SpawnFinishTile()
    {
        finishSpawned = true;

        // 1. Спавним плитку с финишной чертой
        GameObject go = Instantiate(finishTilePrefab, Vector3.forward * spawnZ, Quaternion.identity, transform);
        activeTiles.Add(go);
        spawnZ += tileLength;

        // 2. Спавним ещё 10 ПУСТЫХ плиток (обычно это индекс 0), чтобы была дорога после финиша
        for (int i = 0; i < 10; i++)
        {
            // Используем tilePrefabs[0], предполагая, что это пустая дорога без преград
            GameObject emptyTile = Instantiate(tilePrefabs[0], Vector3.forward * spawnZ, Quaternion.identity, transform);
            activeTiles.Add(emptyTile);
            spawnZ += tileLength;
        }

        Debug.Log("Финиш и запасная дорога построены!");
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
