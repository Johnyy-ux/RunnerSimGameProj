using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShapeController : MonoBehaviour
{
    [Header("Jumping")]
    public float cubeJumpForce = 12f;
    public float sphereJumpForce = 15f;
    public float cubeGravityScale = 3.66f;
    public float sphereGravityScale = 2f;
    public float groundCheckDistance = 0.9f;
    public LayerMask groundLayer;

    [Header("Shape & Models")]
    public PlayerShape currentShape = PlayerShape.Cube;
    public GameObject cubeModel;
    public GameObject sphereModel;

    [Header("Shape Transition")]
    public float shapeTransitionSpeed = 2f;

    private const float SPHERE_MULT = 1.3f;
    private const float CUBE_MULT = 0.85f;
    private float currentMultiplier = CUBE_MULT;

    private bool isGrounded;
    private Rigidbody rb;
    private PlayerMovement movement;

    public PlayerShape CurrentShape => currentShape;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        movement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        if (!IsGameActive()) return;

        CheckGrounded();
        ApplyCustomGravity();
        SmoothMultiplier();
    }

    private void CheckGrounded()
    {
        isGrounded = Physics.Raycast(transform.position + Vector3.up * 0.2f,
                                   Vector3.down, groundCheckDistance, groundLayer);
    }

    private void ApplyCustomGravity()
    {
        if (isGrounded) return;

        float g = (currentShape == PlayerShape.Cube) ? cubeGravityScale : sphereGravityScale;
        rb.AddForce(Vector3.down * g * 9.81f, ForceMode.Acceleration);
    }

    private void SmoothMultiplier()
    {
        float target = (currentShape == PlayerShape.Sphere) ? SPHERE_MULT : CUBE_MULT;
        currentMultiplier = Mathf.MoveTowards(currentMultiplier, target, shapeTransitionSpeed * Time.deltaTime);
    }

    public void PerformJump(Rigidbody rb)
    {
        if (!isGrounded) return;

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        float force = (currentShape == PlayerShape.Sphere) ? sphereJumpForce : cubeJumpForce;
        rb.AddForce(Vector3.up * force, ForceMode.Impulse);
    }

    public float GetCurrentMultiplier() => currentMultiplier;

    public float GetNormalizedSpeed(float currentForwardSpeed)
    {
        float real = currentForwardSpeed * currentMultiplier;
        float min = movement.startSpeed * CUBE_MULT;
        float max = movement.maxSpeed * SPHERE_MULT;
        return Mathf.Clamp01((real - min) / (max - min));
    }

    public void SetShape(int shapeIndex)
    {
        currentShape = (PlayerShape)shapeIndex;

        if (cubeModel) cubeModel.SetActive(currentShape == PlayerShape.Cube);
        if (sphereModel) sphereModel.SetActive(currentShape == PlayerShape.Sphere);

        gameObject.tag = (currentShape == PlayerShape.Cube) ? "PlayerCube" : "PlayerSphere";
    }

    public void ApplySkin(Material mat)
    {
        if (cubeModel) cubeModel.GetComponent<Renderer>().material = mat;
        if (sphereModel) sphereModel.GetComponent<Renderer>().material = mat;
    }

    private bool IsGameActive() => LevelManager.Instance == null || LevelManager.Instance.isGameStarted;
}
