using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapeVisuals : MonoBehaviour
{
    [Header("References")]
    public Transform sphereModel;
    public Transform cubeModel;

    [Header("Sphere Settings")]
    public float sphereRadius = 0.5f;
    private float sphereRotationX;

    [Header("Cube Settings")]
    [Range(0, 20)] public float tiltAmount = 12f;
    [Range(0, 20)] public float bounceIntensity = 0.05f;
    [Range(0, 20)] public float animationSpeed = 10f;

    private Rigidbody rb;
    private Vector3 initialCubeScale;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (cubeModel != null) initialCubeScale = cubeModel.localScale;
    }

    void Update()
    {
        // Определяем, какая фигура сейчас активна (можно связать с твоим Enum)
        if (sphereModel.gameObject.activeSelf)
        {
            AnimateSphere();
        }
        else if (cubeModel.gameObject.activeSelf)
        {
            AnimateCube();
        }
    }

    private void AnimateSphere()
    {
        // Реалистичное качение: угол поворота зависит от пройденного пути
        // Формула: $$\Delta\theta = \frac{\Delta d}{r} \cdot \frac{180}{\pi}$$
        float velocity = rb.velocity.z;
        float distanceMoved = velocity * Time.deltaTime;
        float rotationDegree = (distanceMoved / sphereRadius) * Mathf.Rad2Deg;

        sphereRotationX += rotationDegree;
        sphereModel.localRotation = Quaternion.Euler(sphereRotationX, 0, 0);
    }

    private void AnimateCube()
    {
        float horizontalInput = Input.GetAxis("Horizontal"); // Или твоя переменная управления

        // 1. Наклон при повороте (Инерция)
        float targetTilt = -horizontalInput * tiltAmount;
        float currentTilt = Mathf.LerpAngle(cubeModel.localEulerAngles.z, targetTilt, Time.deltaTime * animationSpeed);

        // 2. "Дыхание" куба (Squash and Stretch)
        // Создает ощущение органики, а не просто мертвого пластика
        float breathe = Mathf.Sin(Time.time * animationSpeed) * bounceIntensity;
        Vector3 targetScale = new Vector3(
            initialCubeScale.x + breathe,
            initialCubeScale.y - breathe,
            initialCubeScale.z + breathe
        );

        cubeModel.localRotation = Quaternion.Euler(0, 0, currentTilt);
        cubeModel.localScale = Vector3.Lerp(cubeModel.localScale, targetScale, Time.deltaTime * animationSpeed);
    }
}
