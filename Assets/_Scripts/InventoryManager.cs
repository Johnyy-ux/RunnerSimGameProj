using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("All Skins")]
    public List<SkinItem> allSkins = new();

    [Header("UI")]
    public GameObject skinButtonPrefab;
    public Transform container;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start() => SpawnAllSkinButtons();

    // ── UI Generation ────────────────────────────────────────────────────────
    public void SpawnAllSkinButtons()
    {
        // Clear old buttons
        foreach (Transform child in container)
            Destroy(child.gameObject);

        foreach (var skin in allSkins)
        {
            if (skin == null) continue;
            GameObject button = Instantiate(skinButtonPrefab, container);
            var ui = button.GetComponent<SkinButtonUI>();
            if (ui != null) ui.SetUpButton(skin);
        }
    }

    // ── Skin Logic ───────────────────────────────────────────────────────────
    public bool IsSkinUnlocked(SkinItem skin)
    {
        if (skin == null) return false;
        if (allSkins.Count > 0 && allSkins[0] == skin) return true; // Первый скин всегда бесплатный
        return PlayerPrefs.GetInt("Skin_" + skin.name, 0) == 1;
    }

    public bool TryBuySkin(SkinItem skin)
    {
        if (skin == null || IsSkinUnlocked(skin)) return false;

        int currentCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        if (currentCoins < skin.price) return false;

        // Purchase
        PlayerPrefs.SetInt("TotalCoins", currentCoins - skin.price);
        PlayerPrefs.SetInt("Skin_" + skin.name, 1);
        PlayerPrefs.Save();

        LevelManager.Instance?.UpdateCoinDisplay(showTotal: true);
        return true;
    }

    public void SelectSkin(SkinItem skin)
    {
        if (skin == null) return;

        int index = allSkins.IndexOf(skin);
        if (index < 0) return;

        PlayerPrefs.SetInt("SelectedSkinIndex", index);
        PlayerPrefs.Save();

        // Обновляем скин на игроке (новая система)
        var shapeController = FindFirstObjectByType<PlayerShapeController>();
        shapeController?.ApplySkin(skin.skinMaterial);
    }

    public Material GetSavedSkinMaterial()
    {
        if (allSkins == null || allSkins.Count == 0)
        {
            Debug.LogError("InventoryManager: allSkins list is empty!");
            return null;
        }

        int index = PlayerPrefs.GetInt("SelectedSkinIndex", 0);
        index = Mathf.Clamp(index, 0, allSkins.Count - 1);

        return allSkins[index].skinMaterial;
    }
}
