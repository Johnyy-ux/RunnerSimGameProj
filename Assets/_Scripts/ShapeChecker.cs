using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapeChecker : MonoBehaviour
{
    [Tooltip("Какой тег игрока должен быть, чтобы пройти? (PlayerSphere или PlayerCube)")]
    public string requiredTag;

    private void OnTriggerEnter(Collider other)
    {
        // Проверяем, что это вообще игрок
        if (other.CompareTag("PlayerCube") || other.CompareTag("PlayerSphere"))
        {
            // Если тег НЕ совпадает с нужным
            if (!other.CompareTag(requiredTag))
            {
                Debug.Log("Форма не совпадает! Требовалось: " + requiredTag);
                if (LevelManager.Instance != null)
                {
                    LevelManager.Instance.GameOver();
                }
            }
        }
    }
}
