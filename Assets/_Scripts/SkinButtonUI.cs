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
    public RectTransform lockContainer;

    private CanvasGroup lockCanvasGroup;

    public void SetUpButton(SkinItem newSkin)
    {
        skin = newSkin;
        lockCanvasGroup = lockContainer.GetComponent<CanvasGroup>()
                       ?? lockContainer.gameObject.AddComponent<CanvasGroup>();

        UpdateVisual();

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(OnButtonClick);
    }

    public void UpdateVisual()
    {
        bool isUnlocked = InventoryManager.Instance.IsSkinUnlocked(skin);

        if (isUnlocked)
        {
            priceText.text = "SELECT";
            priceText.color = Color.green;
            closedLock.SetActive(false);
            openLock.SetActive(false);
            lockCanvasGroup.alpha = 0f;
        }
        else
        {
            priceText.text = $"{skin.price}";
            priceText.color = Color.white;
            closedLock.SetActive(true);
            openLock.SetActive(false);
            lockCanvasGroup.alpha = 1f;
            lockContainer.localScale = Vector3.one;
            lockContainer.anchoredPosition = Vector2.zero;
        }
    }

    private void OnButtonClick()
    {
        if (InventoryManager.Instance.IsSkinUnlocked(skin))
        {
            InventoryManager.Instance.SelectSkin(skin);
        }
        else if (InventoryManager.Instance.TryBuySkin(skin))
        {
            StartCoroutine(UnlockSequence());
        }
    }

    private IEnumerator UnlockSequence()
    {
        buyButton.interactable = false;

        // Scale up animation
        yield return AnimateScale(lockContainer, 1f, 1.3f, 0.15f);

        // Swap lock icons
        closedLock.SetActive(false);
        openLock.SetActive(true);
        yield return new WaitForSeconds(0.15f);

        // Fall + fade out
        yield return AnimateFallAndFade();

        UpdateVisual();
        buyButton.interactable = true;
    }

    private IEnumerator AnimateScale(RectTransform rect, float startScale, float endScale, float duration)
    {
        float t = 0f;
        Vector3 start = Vector3.one * startScale;
        Vector3 end = Vector3.one * endScale;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            rect.localScale = Vector3.Lerp(start, end, t);
            yield return null;
        }
    }

    private IEnumerator AnimateFallAndFade()
    {
        float t = 0f;
        Vector2 startPos = lockContainer.anchoredPosition;
        Vector2 targetPos = startPos + new Vector2(0, -120f);

        while (t < 1f)
        {
            t += Time.deltaTime * 3f;
            lockContainer.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            lockCanvasGroup.alpha = 1f - t;
            yield return null;
        }
    }
}

