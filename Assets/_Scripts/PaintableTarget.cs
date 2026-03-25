using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintableTarget : MonoBehaviour
{
    // Текстура, на которой мы будем рисовать (создается в памяти)
    private RenderTexture paintTexture;
    private Material wallMaterial;

    // Имя свойства в шейдере, куда мы запишем текстуру краски
    private static readonly int PaintTexID = Shader.PropertyToID("_PaintTex");

    void Start()
    {
        // 1. Получаем материал стены
        wallMaterial = GetComponentInChildren<Renderer>().material;

        // 2. Создаем новую текстуру в памяти (холст)
        // Размер 512x512 обычно достаточен для мобилок
        paintTexture = new RenderTexture(512, 512, 0, RenderTextureFormat.ARGB32);
        paintTexture.Create();

        // 3. Передаем эту текстуру в шейдер стены
        wallMaterial.SetTexture(PaintTexID, paintTexture);

        // Очищаем холст (делаем его прозрачным)
        ClearPaint();
    }

    // Метод, который рисует кляксу в указанной UV-точке
    public void PaintAt(Vector2 uv, float radius, Color color, Texture2D brushShape)
    {
        // Это "магия" Unity - рисование одной текстуры поверх другой в памяти ГПУ
        RenderTexture.active = paintTexture; // Начинаем рисовать на холсте
        GL.PushMatrix(); // Сохраняем матрицу трансформации
        GL.LoadPixelMatrix(0, paintTexture.width, paintTexture.height, 0); // Настраиваем координаты

        // Высчитываем, где рисовать на текстуре
        Rect rect = new Rect(uv.x * paintTexture.width - radius, (1 - uv.y) * paintTexture.height - radius, radius * 2, radius * 2);

        // Рисуем кляксу цветом игрока
        Graphics.DrawTexture(rect, brushShape, new Rect(0, 0, 1, 1), 0, 0, 0, 0, color, wallMaterial);

        GL.PopMatrix(); // Восстанавливаем матрицу
        RenderTexture.active = null; // Закончили рисовать
    }

    public void ClearPaint()
    {
        RenderTexture.active = paintTexture;
        GL.Clear(true, true, Color.clear);
        RenderTexture.active = null;
    }

    void OnDestroy()
    {
        if (paintTexture != null) paintTexture.Release();
    }
}
