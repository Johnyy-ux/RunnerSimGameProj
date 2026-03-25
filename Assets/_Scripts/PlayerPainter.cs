using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPainter : MonoBehaviour
{
    [Header("Настройки рисования")]
    public Texture2D brushShape; // Чёрно-белая текстура кляксы (мягкий круг или пятно)
    public float paintRadius = 20f; // Размер кляксы на стене

    private Color playerColor;

    void Start()
    {
        // 1. Автоматически берем цвет скина игрока из его материала
        // Предполагаем, что MeshRenderer находится на том же объекте или в дочернем
        playerColor = GetComponentInChildren<Renderer>().material.color;
    }

    void OnCollisionEnter(Collision collision)
    {
        // 2. Проверяем, что ударились в препятствие с возможностью рисования
        PaintableTarget target = collision.gameObject.GetComponent<PaintableTarget>();
        if (target != null)
        {
            // Берем первую точку контакта
            ContactPoint contact = collision.contacts[0];

            // 3. Главная магия: Рейкаст для определения UV-координат
            // Физический движок Unity умеет возвращать UV в точке удара, если колайдер - MeshCollider.
            // Если ты используешь BoxCollider на стене, нам нужно сделать небольшой рейкаст "внутрь" удара.

            RaycastHit hit;
            // Делаем рейкаст от игрока в сторону стены
            if (Physics.Raycast(contact.point + contact.normal * 0.1f, -contact.normal, out hit, 1f))
            {
                if (hit.collider.gameObject == collision.gameObject && hit.lightmapCoord != Vector2.zero)
                {
                    // Мы нашли UV-координаты!
                    Vector2 uv = hit.lightmapCoord; // Или hit.textureCoord, зависит от настройки импорта модели

                    // 4. Рисуем на стене цветом игрока
                    target.PaintAt(uv, paintRadius, playerColor, brushShape);

                    Debug.Log("Стена испачкана красиво!");
                }
            }

            // После рисования вызываем GameOver (как обычно)
            // LevelManager.Instance.GameOver();
        }
    }
}
