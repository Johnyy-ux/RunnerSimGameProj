using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicCamera : MonoBehaviour
{
    [Header("Targeting")]
    public Transform target;
    public float smoothTime = 0.12f;

    [Header("FOV")]
    public Camera cam;
    public float baseFOV = 60f;
    public float maxFOV = 75f;
    public float fovLerpSpeed = 4f;

    private Vector3 currentVelocity;
    private Vector3 offset;
    private PlayerMovement playerMovement;

    private bool isDeadSequence = false;

    private void Start()
    {
        if (target != null)
        {
            playerMovement = target.GetComponent<PlayerMovement>();
            offset = transform.position - target.position;
        }

        if (cam == null)
            cam = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        if (isDeadSequence || target == null || playerMovement == null)
            return;

        // Smooth follow
        Vector3 targetPos = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref currentVelocity, smoothTime);

        // Dynamic FOV based on speed
        float normalizedSpeed = playerMovement.GetNormalizedSpeed();
        float targetFOV = Mathf.Lerp(baseFOV, maxFOV, normalizedSpeed);
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * fovLerpSpeed);

        // Keep camera level (no Z rotation)
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, 0f, 0f);
    }

    // ── Death Sequence ───────────────────────────────────────────────────────
    public void StartDeathSequence(Vector3 impactPoint)
    {
        if (isDeadSequence) return;
        StartCoroutine(DeathSequenceCoroutine(impactPoint));
    }

    private IEnumerator DeathSequenceCoroutine(Vector3 impactPoint)
    {
        isDeadSequence = true;

        Vector3 startPos = transform.position;
        Vector3 targetBasePos = impactPoint + offset;
        Vector3 overshootPos = targetBasePos + Vector3.forward * 5f;

        // 1. Inertia overshoot
        yield return SmoothMove(startPos, overshootPos, 0.6f, EaseOutSine);

        yield return new WaitForSecondsRealtime(0.15f);

        // 2. Pull back to impact
        yield return SmoothMove(overshootPos, targetBasePos, 0.8f, SmoothStep);

        LevelManager.Instance?.ShowGameOver();
    }

    private IEnumerator SmoothMove(Vector3 start, Vector3 end, float duration, System.Func<float, float> easing)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = easing(t);
            transform.position = Vector3.Lerp(start, end, eased);
            yield return null;
        }
    }

    private float EaseOutSine(float t) => Mathf.Sin(t * Mathf.PI * 0.5f);
    private float SmoothStep(float t) => t * t * (3f - 2f * t);
}
