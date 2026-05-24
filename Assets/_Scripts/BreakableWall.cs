using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableWall : MonoBehaviour
{
    [Header("Destruction Settings")]
    public float explosionForce = 500f;
    public float explosionRadius = 5f;
    public GameObject explosionParticles;

    private Rigidbody[] bricks;
    private bool isShattered = false;

    private void Awake()
    {
        bricks = GetComponentsInChildren<Rigidbody>();
        foreach (var rb in bricks)
        {
            rb.isKinematic = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }
    }

    public void Shatter(Vector3 hitPoint)
    {
        if (isShattered) return;
        isShattered = true;

        if (explosionParticles != null)
            Instantiate(explosionParticles, hitPoint, Quaternion.identity);

        foreach (var rb in bricks)
        {
            rb.isKinematic = false;
            rb.AddExplosionForce(explosionForce, hitPoint, explosionRadius);
            Destroy(rb.gameObject, 3f);
        }

        Destroy(gameObject, 3.1f);
    }
}
