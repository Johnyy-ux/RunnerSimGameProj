using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;
    public List<SkinItem> allSkins;

    [Header("UI")]
    public GameObject skinButtonPrefab;
    public Transform container;

    void Awake() => Instance = this;

    void Start() => SpawnButtons();

    public void SpawnButtons()
    {
        foreach (Transform child in container) Destroy(child.gameObject);

        foreach (var skin in allSkins)
        {
            GameObject go = Instantiate(skinButtonPrefab, container);
            go.GetComponent<SkinButtonUI>().SetUpButton(skin);
        }
    }

    public bool IsSkinUnlocked(SkinItem skin)
    {
        // Первый скин всегда открыт
        if (allSkins.IndexOf(skin) == 0) return true;
        return PlayerPrefs.GetInt("Skin_" + skin.name, 0) == 1;
    }

    public bool TryBuySkin(SkinItem skin)
    {
        int currentCoins = PlayerPrefs.GetInt("TotalCoins", 0);

        if (currentCoins >= skin.price && !IsSkinUnlocked(skin))
        {
            currentCoins -= skin.price;
            PlayerPrefs.SetInt("TotalCoins", currentCoins);
            PlayerPrefs.SetInt("Skin_" + skin.name, 1);
            PlayerPrefs.Save();

            LevelManager.Instance.UpdateMenuCoinDisplay();
            return true;
        }
        return false;
    }

    public Material GetSavedSkinMaterial()
    {
        // Достаем индекс выбранного скина (по умолчанию 0)
        int selectedIndex = PlayerPrefs.GetInt("SelectedSkinIndex", 0);

        // Проверяем, что индекс в пределах списка и список не пуст
        if (allSkins != null && allSkins.Count > 0)
        {
            if (selectedIndex >= 0 && selectedIndex < allSkins.Count)
            {
                return allSkins[selectedIndex].skinMaterial;
            }
            return allSkins[0].skinMaterial; // Если индекс кривой, даем первый скин
        }

        Debug.LogError("Список allSkins пуст в InventoryManager!");
        return null;
    }

    public void SelectSkin(SkinItem skin)
    {
        int index = allSkins.IndexOf(skin);
        PlayerPrefs.SetInt("SelectedSkinIndex", index);
        PlayerPrefs.Save();

        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null) player.ApplySkin(skin.skinMaterial);
    }
}
