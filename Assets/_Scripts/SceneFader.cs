using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneFader : MonoBehaviour
{
    private CanvasGroup cg;

    void Awake()
    {
        cg = GetComponent<CanvasGroup>();
        cg.alpha = 1; // Начинаем с черного экрана
        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        float t = 1;
        while (t > 0)
        {
            t -= Time.deltaTime * 2f; // Скорость появления
            cg.alpha = t;
            yield return null;
        }
    }
}
