using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicCamera : MonoBehaviour
{
    [Header("Targeting")]
    public Transform target;
    public float smoothTime = 0.12f;

    private Vector3 currentVelocity;
    private Vector3 offset;
    private PlayerController playerController;

    [Header("FOV")]
    public Camera cam;
    public float baseFOV = 60f;
    public float maxFOV = 75f;
    public float fovLerpSpeed = 4f;

    private bool isDeadSequence = false; // Блокировка обычного следования

    void Start()
    {
        if (target != null)
        {
            playerController = target.GetComponent<PlayerController>();
            offset = transform.position - target.position;
        }
        if (cam == null) cam = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        // Если проигрывается сцена смерти или нет цели — не двигаемся в обычном режиме
        if (isDeadSequence || target == null || playerController == null) return;

        Vector3 targetPos = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref currentVelocity, smoothTime);

        float speedFactor = playerController.GetNormalizedSpeed();
        float targetFOV = Mathf.Lerp(baseFOV, maxFOV, speedFactor);
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * fovLerpSpeed);

        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, 0, 0);
    }

    // Метод, который вызовет скрипт смерти
    public void StartDeathSequence(Vector3 impactPoint)
    {
        if (isDeadSequence) return;
        StartCoroutine(DeathSequenceCoroutine(impactPoint));
    }

    IEnumerator DeathSequenceCoroutine(Vector3 impactPoint)
    {
        isDeadSequence = true;

        Vector3 startPos = transform.position;
        Vector3 targetBasePos = impactPoint + offset;
        // Точка перелета (инерция)
        Vector3 overshootPos = targetBasePos + Vector3.forward * 5f;

        // 1. Пролет вперед (Инерция) - ПЛАВНО
        float elapsed = 0;
        float duration = 0.6f; // Длительность пролета
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime; // Используем unscaled, чтобы замедление времени не тормозило камеру
            float t = elapsed / duration;
            // Функция для плавного замедления (Ease Out)
            t = Mathf.Sin(t * Mathf.PI * 0.5f);

            transform.position = Vector3.Lerp(startPos, overshootPos, t);
            yield return null;
        }

        yield return new WaitForSecondsRealtime(0.15f); // Маленькая пауза в пике

        // 2. Возврат назад - ОЧЕНЬ ПЛАВНО
        elapsed = 0;
        duration = 0.8f; // Возврат чуть дольше
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            // Функция для плавного начала и конца (SmoothStep)
            t = t * t * (3f - 2f * t);

            transform.position = Vector3.Lerp(overshootPos, targetBasePos, t);
            yield return null;
        }

        // 3. ФИНАЛ: Камера прилетела, теперь показываем UI
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.ShowGameOverUI();
        }
    }
}
