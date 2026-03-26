using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiquidDeath : MonoBehaviour
{
    [Header("Настройки")]
    public GameObject paintBlobPrefab; // Префаб, сплющенный по Z (например, 0.01)
    public float squashSpeed = 15f;    // Скорость сплющивания игрока
    [Range(0.5f, 3.0f)] public float splatRadius = 1.5f; // Радиус разлета

    private bool isDead = false;

    void OnCollisionEnter(Collision collision)
    {
        if (isDead || !collision.gameObject.CompareTag("Obstacle")) return;
        isDead = true;

        // 1. Берем цвет игрока прямо сейчас
        Color currentColor = Color.white;
        Renderer playerRend = GetComponentInChildren<Renderer>();
        if (playerRend != null) currentColor = playerRend.material.color;

        // 2. Останавливаем физику
        if (TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
        }

        ContactPoint contact = collision.contacts[0];

        // 3. Создаем ПЛОСКУЮ кляксу
        CreateSplat(contact, currentColor);

        // 4. Впечатываем шар
        StartCoroutine(SquashAndHide(contact.normal));

        if (LevelManager.Instance != null) LevelManager.Instance.GameOver();
    }

    void CreateSplat(ContactPoint contact, Color color)
    {
        int pieces = Random.Range(6, 10); // Сколько кусочков

        // Вращение, чтобы ось Z (синяя) смотрела ОТ стены
        Quaternion rotation = Quaternion.LookRotation(contact.normal);

        // ГЛАВНОЕ: Узнаем, какой масштаб по Z в префабе (например, 0.01)
        float flatZ = paintBlobPrefab.transform.localScale.z;

        for (int i = 0; i < pieces; i++)
        {
            // Случайная точка в круге
            Vector2 randomPoint = Random.insideUnitCircle * splatRadius;
            Vector3 offset = rotation * new Vector3(randomPoint.x, randomPoint.y, 0);

            // Точка спавна чуть-чуть перед стеной (0.01f), чтобы не было мерцания
            Vector3 spawnPos = contact.point + contact.normal * 0.01f + offset;

            GameObject blob = Instantiate(paintBlobPrefab, spawnPos, rotation);

            // ХАОС ФОРМЫ (КРУГИ):
            // Случайный размер по X и Y, а Z оставляем "плоским" из префаба
            float s = Random.Range(0.4f, 1.2f);

            // ИСПРАВЛЕННАЯ СТРОЧКА: Применяем плоский Z из префаба
            blob.transform.localScale = new Vector3(s, s, flatZ);

            // Случайный поворот "блина" по кругу
            blob.transform.Rotate(Vector3.forward, Random.Range(0, 360));

            // ПОКРАСКА
            Renderer rend = blob.GetComponentInChildren<Renderer>();
            if (rend != null) rend.material.color = color;

            blob.transform.SetParent(contact.otherCollider.transform);
        }
    }

    IEnumerator SquashAndHide(Vector3 wallNormal)
    {
        float t = 0;
        Vector3 startScale = transform.localScale;

        while (t < 1f)
        {
            t += Time.deltaTime * squashSpeed;
            // Делаем шар ОЧЕНЬ широким при ударе (эффект лепешки)
            transform.localScale = Vector3.Lerp(startScale, new Vector3(1.8f, 1.8f, 0.001f), t);
            yield return null;
        }

        // Прячем игрока
        foreach (var rend in GetComponentsInChildren<Renderer>()) rend.enabled = false;
        if (GetComponent<Collider>()) GetComponent<Collider>().enabled = false;
    }
}
