using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMenuTransition : MonoBehaviour
{
    public RectTransform leftPanel;   // Левая красная полоса
    public RectTransform rightPanel;  // Правая красная полоса
    public RectTransform topUI;       // Монетки и Level

    public float duration = 0.5f;

    public void PlayStartAnimation()
    {
        StartCoroutine(TransitionRoutine());
    }

    IEnumerator TransitionRoutine()
    {
        float elapsed = 0;

        // Начальные и конечные позиции (уводим за экран)
        Vector2 leftTarget = new Vector2(-leftPanel.rect.width - 100, leftPanel.anchoredPosition.y);
        Vector2 rightTarget = new Vector2(rightPanel.rect.width + 100, rightPanel.anchoredPosition.y);
        Vector2 topTarget = new Vector2(topUI.anchoredPosition.x, topUI.rect.height + 100);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;

            // Плавная кривая
            float curve = t * t * (3f - 2f * t);

            leftPanel.anchoredPosition = Vector2.Lerp(leftPanel.anchoredPosition, leftTarget, curve);
            rightPanel.anchoredPosition = Vector2.Lerp(rightPanel.anchoredPosition, rightTarget, curve);
            topUI.anchoredPosition = Vector2.Lerp(topUI.anchoredPosition, topTarget, curve);

            yield return null;
        }

        // Выключаем совсем после анимации
        gameObject.SetActive(false);
    }
}
