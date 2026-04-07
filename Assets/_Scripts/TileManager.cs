using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    public GameObject[] tilePrefabs;
    public Transform playerTransform;

    public float tileLength = 10f;
    public int numberOfTiles = 6;
    public int backBuffer = 2; // Сколько плиток оставляем за спиной

    private float spawnZ = 0f;
    private List<GameObject> activeTiles = new List<GameObject>();

    void Start()
    {
        if (playerTransform == null) playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        // Начальный спавн: создаем цепочку плиток
        for (int i = 0; i < numberOfTiles; i++)
        {
            // Первая плитка (i=0) всегда пустая (индекс 0)
            SpawnTile(i == 0 ? 0 : Random.Range(0, tilePrefabs.Length));
        }
    }

    void Update()
    {
        if (playerTransform == null) return;

        // Логика спавна: если игрок проехал достаточно, ставим новую плитку
        // Используем старую добрую проверку через spawnZ
        if (playerTransform.position.z > spawnZ - (numberOfTiles * tileLength))
        {
            SpawnTile(Random.Range(0, tilePrefabs.Length));

            // Удаляем только если плиток стало больше, чем (видимые + буфер сзади)
            if (activeTiles.Count > numberOfTiles + backBuffer)
            {
                DeleteTile();
            }
        }
    }

    public void SpawnTile(int tileIndex)
    {
        GameObject go = Instantiate(tilePrefabs[tileIndex], Vector3.forward * spawnZ, Quaternion.identity, transform);
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
