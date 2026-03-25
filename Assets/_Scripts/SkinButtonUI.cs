using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkinButtonUI : MonoBehaviour
{
    public SkinItem skin; // Ссылка на данные скина
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI priceText;
    public Button buyButton;

    public void SetUpButton(SkinItem newSkin)
    {
        skin = newSkin;
        nameText.text = skin.skinName;

        UpdateVisual();

        // При нажатии пытаемся купить или выбрать
        buyButton.onClick.AddListener(OnButtonClick);
    }

    public void UpdateVisual()
    {
        if (skin.isUnlocked)
        {
            priceText.text = "Выбрать";
            priceText.color = Color.green;
        }
        else
        {
            priceText.text = skin.price.ToString() + " $";
            priceText.color = Color.white;
        }
    }

    void OnButtonClick()
    {
            if (skin.isUnlocked)
            {
                PlayerController player = FindFirstObjectByType<PlayerController>();
                if (player != null)
                {
                    player.ApplySkin(skin.skinMaterial);

                    // Находим номер этого скина в списке менеджера
                    int index = InventoryManager.Instance.allSkins.IndexOf(skin);
                    PlayerPrefs.SetInt("SelectedSkinIndex", index); // Сохраняем число
                    PlayerPrefs.Save();

                    Debug.Log($"Скин {skin.skinName} сохранен под индексом {index}");
                }
            }
            else
            {
                InventoryManager.Instance.BuySkin(skin);
                UpdateVisual();
            }
    }
}

