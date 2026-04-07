using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiquidDeath : MonoBehaviour
{
    [Header("Настройки")]
    public GameObject paintBlobPrefab;
    public float squashSpeed = 15f;
    [Range(0.5f, 3.0f)] public float splatRadius = 1.5f;

    private bool isDead = false;

    void OnCollisionEnter(Collision collision)
    {
        if (isDead || !collision.gameObject.CompareTag("Obstacle")) return;
        isDead = true;

        Color currentColor = Color.white;
        Renderer playerRend = GetComponentInChildren<Renderer>();
        if (playerRend != null) currentColor = playerRend.material.color;

        if (TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
        }

        ContactPoint contact = collision.contacts[0];

        // Создаем кляксу
        CreateSplat(contact, currentColor);

        // Запускаем расплющивание
        StartCoroutine(SquashAndHide(contact.normal));

        // НАХОДИМ КАМЕРУ И ЗАПУСКАЕМ ПРОЛЕТ
        // Ищем скрипт на главной камере
        DynamicCamera dynCam = Camera.main.GetComponent<DynamicCamera>();
        if (dynCam != null)
        {
            dynCam.StartDeathSequence(contact.point);
        }
        else
        {
            // Если скрипт камеры не найден (на всякий случай), вызываем GameOver сразу
            if (LevelManager.Instance != null) LevelManager.Instance.ShowGameOver();
        }
    }

    void CreateSplat(ContactPoint contact, Color color)
    {
        int pieces = Random.Range(6, 10);
        Quaternion rotation = Quaternion.LookRotation(contact.normal);
        float flatZ = paintBlobPrefab.transform.localScale.z;

        for (int i = 0; i < pieces; i++)
        {
            Vector2 randomPoint = Random.insideUnitCircle * splatRadius;
            Vector3 offset = rotation * new Vector3(randomPoint.x, randomPoint.y, 0);
            Vector3 spawnPos = contact.point + contact.normal * 0.01f + offset;

            GameObject blob = Instantiate(paintBlobPrefab, spawnPos, rotation);

            float s = Random.Range(0.4f, 1.2f);
            blob.transform.localScale = new Vector3(s, s, flatZ);
            blob.transform.Rotate(Vector3.forward, Random.Range(0, 360));

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
            transform.localScale = Vector3.Lerp(startScale, new Vector3(1.8f, 1.8f, 0.001f), t);
            yield return null;
        }

        foreach (var rend in GetComponentsInChildren<Renderer>()) rend.enabled = false;
        if (GetComponent<Collider>()) GetComponent<Collider>().enabled = false;
    }
}
