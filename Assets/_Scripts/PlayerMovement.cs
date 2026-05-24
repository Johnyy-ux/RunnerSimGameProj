using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float startSpeed = 7f;
    public float speedIncrease = 0.1f;
    public float maxSpeed = 30f;
    public float sideSpeed = 15f;
    public float laneDistance = 3f;

    [Header("Sphere Control")]
    public float sphereMaxSpeed = 18f;
    [Range(0.1f, 1f)] public float sphereAccelerationScale = 0.6f;

    public float CurrentForwardSpeed { get; private set; }
    public int CurrentLane { get; private set; } = 1;

    private Rigidbody rb;
    private PlayerShapeController shapeController;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        shapeController = GetComponent<PlayerShapeController>();
        CurrentForwardSpeed = startSpeed;
    }

    private void FixedUpdate()
    {
        if (!IsGameActive())
        {
            rb.velocity = Vector3.zero;
            return;
        }

        MoveForward();
        MoveSideways();
    }

    private void MoveForward()
    {
        float accelScale = (shapeController.CurrentShape == PlayerShape.Sphere) ? sphereAccelerationScale : 1f;

        CurrentForwardSpeed += speedIncrease * accelScale * Time.deltaTime;
        CurrentForwardSpeed = Mathf.Min(CurrentForwardSpeed, maxSpeed);

        float speedCap = (shapeController.CurrentShape == PlayerShape.Sphere) ? sphereMaxSpeed : maxSpeed;
        float finalSpeed = Mathf.Min(CurrentForwardSpeed * shapeController.GetCurrentMultiplier(), speedCap);

        rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y, finalSpeed);
    }

    private void MoveSideways()
    {
        float targetX = (CurrentLane - 1) * laneDistance;
        float xVelocity = (targetX - rb.position.x) / Time.fixedDeltaTime;
        xVelocity = Mathf.Clamp(xVelocity, -sideSpeed, sideSpeed);

        rb.velocity = new Vector3(xVelocity, rb.velocity.y, rb.velocity.z);
        rb.angularVelocity = Vector3.zero;
    }

    public void ChangeLane(int direction)
    {
        CurrentLane = Mathf.Clamp(CurrentLane + direction, 0, 2);
    }

    public void Jump() => shapeController.PerformJump(rb);

    private bool IsGameActive() =>
        LevelManager.Instance == null || LevelManager.Instance.isGameStarted;

    // Для камеры и других систем
    public float GetNormalizedSpeed() => shapeController.GetNormalizedSpeed(CurrentForwardSpeed);
}

public enum PlayerShape
{
    Cube,
    Sphere
}