using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVisuals : MonoBehaviour
{
    [Header("Visual Settings")]
    public float sphereRadius = 0.5f;
    public float tiltAmount = 15f;
    public float tiltSpeed = 10f;

    private PlayerMovement movement;
    private PlayerShapeController shapeController;
    private GameObject cubeModel;
    private GameObject sphereModel;

    private float sphereRotation;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        shapeController = GetComponent<PlayerShapeController>();

        cubeModel = shapeController.cubeModel;
        sphereModel = shapeController.sphereModel;
    }

    private void Update()
    {
        if (!LevelManager.Instance.isGameStarted) return;

        AnimateShapeVisuals();
    }

    private void AnimateShapeVisuals()
    {
        if (shapeController.CurrentShape == PlayerShape.Sphere && sphereModel != null)
        {
            float dist = movement.CurrentForwardSpeed * 1.3f * Time.deltaTime;
            sphereRotation += (dist / sphereRadius) * Mathf.Rad2Deg;
            sphereModel.transform.localRotation = Quaternion.Euler(sphereRotation, 0f, 0f);
        }
        else if (shapeController.CurrentShape == PlayerShape.Cube && cubeModel != null)
        {
            // Tilt
            float targetX = (movement.CurrentLane - 1) * movement.laneDistance;
            float xDiff = targetX - transform.position.x;
            float tiltTarget = -xDiff * tiltAmount;

            float currentTilt = cubeModel.transform.localRotation.eulerAngles.z;
            if (currentTilt > 180) currentTilt -= 360;

            float tiltNow = Mathf.LerpAngle(currentTilt, tiltTarget, Time.deltaTime * tiltSpeed);

            // Light breathing
            float breathe = Mathf.Sin(Time.time * 8f) * 0.05f;
            cubeModel.transform.localScale = new Vector3(1 + breathe, 1 - breathe, 1 + breathe);
            cubeModel.transform.localRotation = Quaternion.Euler(0, 0, tiltNow);
        }
    }
}
