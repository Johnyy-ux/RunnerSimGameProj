using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkinButtonUI : MonoBehaviour
{
    public SkinItem skin;

    [Header("UI Elements")]
    public TextMeshProUGUI priceText;
    public Button buyButton;

    [Header("Lock Animation")]
    public GameObject closedLock;
    public GameObject openLock;
    public RectTransform lockContainer; // Объект, в котором лежат оба замка

    private CanvasGroup lockGroup;

    public void SetUpButton(SkinItem newSkin)
    {
        skin = newSkin;
        if (lockGroup == null) lockGroup = lockContainer.gameObject.GetComponent<CanvasGroup>();
        if (lockGroup == null) lockGroup = lockContainer.gameObject.AddComponent<CanvasGroup>();

        UpdateVisual();
        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(OnButtonClick);
    }

    public void UpdateVisual()
    {
        bool isUnlocked = InventoryManager.Instance.IsSkinUnlocked(skin);

        if (isUnlocked)
        {
            priceText.text = "ВЫБРАТЬ";
            priceText.color = Color.green;
            closedLock.SetActive(false);
            openLock.SetActive(false);
        }
        else
        {
            priceText.text = skin.price.ToString() + " $";
            priceText.color = Color.white;
            closedLock.SetActive(true);
            openLock.SetActive(false);
            lockContainer.localScale = Vector3.one;
            lockContainer.anchoredPosition = Vector2.zero;
            lockGroup.alpha = 1;
        }
    }

    void OnButtonClick()
    {
        if (InventoryManager.Instance.IsSkinUnlocked(skin))
        {
            InventoryManager.Instance.SelectSkin(skin);
        }
        else
        {
            if (InventoryManager.Instance.TryBuySkin(skin))
            {
                StartCoroutine(UnlockSequence());
            }
        }
    }

    IEnumerator UnlockSequence()
    {
        buyButton.interactable = false;

        // 1. Увеличение
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 10f;
            lockContainer.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.3f, t);
            yield return null;
        }

        // 2. Подмена
        closedLock.SetActive(false);
        openLock.SetActive(true);
        yield return new WaitForSeconds(0.15f);

        // 3. Падение и затухание
        t = 0;
        Vector2 startPos = lockContainer.anchoredPosition;
        Vector2 targetPos = startPos + new Vector2(0, -120f); // Падает вниз на 120 единиц

        while (t < 1f)
        {
            t += Time.deltaTime * 3f;
            lockContainer.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            lockGroup.alpha = 1f - t;
            yield return null;
        }

        UpdateVisual();
        buyButton.interactable = true;
    }
}

