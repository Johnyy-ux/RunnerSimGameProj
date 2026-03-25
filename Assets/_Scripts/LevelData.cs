using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Level ", menuName = "Game/LevelData")]
public class LevelData : ScriptableObject
{
    [Header("Main Settings")]
    public int levelNumber;
    public float distanceToFinish = 500f;
    public float baseSpeed = 15f;

    [Header("Visuals & Obstacles")]
    public EnvironmentType locationType; // Тип локации (Лес, Пустыня и т.д.)

    // Специфичные пулы для этого уровня (если хочешь уникальные препятствия)
    public GameObject[] customObstacles;
}

public enum EnvironmentType { Forest, Desert, Cyberpunk }
