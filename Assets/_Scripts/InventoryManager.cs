using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;
    public List<SkinItem> allSkins; // Закинь сюда все свои файлы скинов в инспекторе

    [Header("UI")]
    public GameObject skinButtonPrefab; // Префаб нашей кнопки
    public Transform container;         // Ссылка на Skins_Container

    void Awake() => Instance = this;

    void Start()
    {
        // При старте загружаем состояние: куплен скин или нет
        foreach (var skin in allSkins)
        {
            // По умолчанию первый скин (индекс 0) всегда куплен
            if (allSkins.IndexOf(skin) == 0) skin.isUnlocked = true;
            else
            {
                // Проверяем сохранение по имени скина (1 = куплен, 0 = нет)
                skin.isUnlocked = PlayerPrefs.GetInt("Skin_" + skin.name, 0) == 1;
            }
        }
        SpawnButtons();
    }

    void SpawnButtons()
    {
        // Очищаем контейнер на всякий случай
        foreach (Transform child in container) Destroy(child.gameObject);

        foreach (var skin in allSkins)
        {
            GameObject go = Instantiate(skinButtonPrefab, container);
            go.GetComponent<SkinButtonUI>().SetUpButton(skin);
        }
    }

    public Material GetSavedSkinMaterial()
    {
        // Достаем индекс выбранного скина (по умолчанию 0)
        int selectedIndex = PlayerPrefs.GetInt("SelectedSkinIndex", 0);

        // Проверяем, что индекс не вылетает за пределы списка
        if (selectedIndex >= 0 && selectedIndex < allSkins.Count)
        {
            return allSkins[selectedIndex].skinMaterial;
        }
        return allSkins[0].skinMaterial; // Если что-то не так, даем стандартный
    }

    public void BuySkin(SkinItem skin)
    {
        int currentCoins = PlayerPrefs.GetInt("TotalCoins", 0);

        if (currentCoins >= skin.price && !skin.isUnlocked)
        {
            currentCoins -= skin.price;
            PlayerPrefs.SetInt("TotalCoins", currentCoins);

            skin.isUnlocked = true;
            PlayerPrefs.SetInt("Skin_" + skin.name, 1);
            PlayerPrefs.Save();

            LevelManager.Instance.UpdateMenuCoinDisplay();
            Debug.Log("Куплен скин: " + skin.skinName);
        }
        else
        {
            Debug.Log("Недостаточно монет или уже куплено!");
        }
    }
}
