using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainSpawner : MonoBehaviour
{
    [Header(" рыша поезда")]
    [Tooltip("“очки спавна на самой модели поезда")]
    public Transform[] roofSpawnPoints;

    [Header("„то спавним")]
    public GameObject coinPrefab;
    // —юда можно добавить lowObstaclePrefab, если захочешь ставить барьеры на крышу

    void Start()
    {
        //  ак только поезд по€вл€етс€ в мире, он сам решает, что заспавнить на крыше
        PopulateRoof();
    }

    private void PopulateRoof()
    {
        if (roofSpawnPoints.Length == 0) return;

        // ƒл€ начала просто спавним монетки с шансом 70% на каждой точке крыши
        foreach (Transform point in roofSpawnPoints)
        {
            if (Random.value > 0.3f)
            {
                Instantiate(coinPrefab, point.position, point.rotation, transform);
            }
        }
    }
}
