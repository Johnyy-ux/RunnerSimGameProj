using System.Collections;
using UnityEngine;

public class UIWindowAnimator : MonoBehaviour
{
    public float duration = 0.5f;

    // Метод вызывается автоматически, когда мы делаем SetActive(true)
    void OnEnable()
    {
        transform.localScale = Vector3.zero;
        StartCoroutine(AnimateIn());
    }

    IEnumerator AnimateIn()
    {
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;

            // Формула "Elastic Out" (эффект пружинистого появления)
            float s = t == 1f ? 1f : 1f - Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * 10f - 0.75f) * (2f * Mathf.PI) / 3f);

            transform.localScale = new Vector3(s, s, s);
            yield return null;
        }
        transform.localScale = Vector3.one;
    }
}
