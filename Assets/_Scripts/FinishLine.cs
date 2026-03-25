using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishLine : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Проверяем, что это игрок (любая из его форм)
        if (other.CompareTag("PlayerCube") || other.CompareTag("PlayerSphere"))
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.WinLevel(); // Вызываем победу
            }
        }
    }
}
