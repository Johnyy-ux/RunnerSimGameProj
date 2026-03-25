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
        if (target == null || playerController == null) return;

        // Простое следование без лишних вращений
        Vector3 targetPos = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref currentVelocity, smoothTime);

        // FOV оставляем для динамики, но если и он бесит — можно закомментировать
        float speedFactor = playerController.GetNormalizedSpeed();
        float targetFOV = Mathf.Lerp(baseFOV, maxFOV, speedFactor);
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * fovLerpSpeed);

        // Фиксируем поворот, чтобы не было никаких наклонов
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, 0, 0);
    }
}
