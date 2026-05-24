using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThemeManager : MonoBehaviour
{
    public static ThemeManager Instance { get; private set; }

    [Header("All Themes")]
    public List<ThemeData> allThemes = new();

    [Header("Road Renderers")]
    public List<Renderer> roadRenderers = new();

    public ThemeData ActiveTheme { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        LoadAndApplySavedTheme();
    }

    private void LoadAndApplySavedTheme()
    {
        int index = PlayerPrefs.GetInt("SelectedThemeIndex", 0);
        index = Mathf.Clamp(index, 0, allThemes.Count - 1);

        ApplyTheme(allThemes[index]);
    }

    public void ApplyTheme(ThemeData theme)
    {
        if (theme == null) return;

        ActiveTheme = theme;

        // Skybox
        if (theme.skyboxMaterial != null)
        {
            RenderSettings.skybox = theme.skyboxMaterial;
            DynamicGI.UpdateEnvironment();
        }

        // Road
        if (theme.roadMaterial != null)
        {
            foreach (var renderer in roadRenderers)
            {
                if (renderer != null)
                    renderer.material = theme.roadMaterial;
            }
        }
    }

    public void SelectTheme(ThemeData theme)
    {
        int index = allThemes.IndexOf(theme);
        if (index < 0) return;

        PlayerPrefs.SetInt("SelectedThemeIndex", index);
        PlayerPrefs.Save();

        ApplyTheme(theme);
    }

    public bool IsThemeUnlocked(ThemeData theme)
    {
        if (theme == null) return false;
        if (theme.isUnlockedByDefault || allThemes.Count > 0 && allThemes[0] == theme)
            return true;

        return PlayerPrefs.GetInt("Theme_" + theme.themeName, 0) == 1;
    }

    public bool TryBuyTheme(ThemeData theme)
    {
        if (theme == null || IsThemeUnlocked(theme)) return false;

        int coins = PlayerPrefs.GetInt("TotalCoins", 0);
        if (coins < theme.unlockCost) return false;

        PlayerPrefs.SetInt("TotalCoins", coins - theme.unlockCost);
        PlayerPrefs.SetInt("Theme_" + theme.themeName, 1);
        PlayerPrefs.Save();

        LevelManager.Instance?.UpdateCoinDisplay(showTotal: true);
        return true;
    }
}
