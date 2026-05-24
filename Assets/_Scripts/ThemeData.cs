using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewTheme", menuName = "Game/Theme")]
public class ThemeData : ScriptableObject
{
    [Header("Identity")]
    public string themeName = "New Theme";
    public Sprite themePreviewIcon;

    [Header("Unlock")]
    public int unlockCost = 800;
    public bool isUnlockedByDefault = false;

    [Header("Environment")]
    public Material skyboxMaterial;
    public Color backgroundColor = Color.black; // если используешь solid color background

    [Header("Road & Tiles")]
    public Material roadMaterial;

    [Header("Obstacles & Props")]
    public List<GameObject> standardObstacles = new();
    public List<GameObject> sphereWalls = new();
    public List<GameObject> cubeWalls = new();
    public GameObject coinPrefab;

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(themeName))
            themeName = name;
    }
}
