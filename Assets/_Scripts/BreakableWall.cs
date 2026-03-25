using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableWall : MonoBehaviour
{
    private Rigidbody[] bricks;
    private bool isShattered = false;

    [Header("Настройки разрушения")]
    public float explosionForce = 500f;
    public float explosionRadius = 5f;
    public GameObject explosionParticles;

    void Start()
    {
        bricks = GetComponentsInChildren<Rigidbody>();

        foreach (var rb in bricks)
        {
            rb.isKinematic = true; // Замораживаем до удара
            rb.interpolation = RigidbodyInterpolation.Interpolate; // Для плавности обломков
        }
    }

    public void Shatter(Vector3 hitPoint)
    {
        if (isShattered) return;
        isShattered = true;

        if (explosionParticles != null)
        {
            Instantiate(explosionParticles, hitPoint, Quaternion.identity);
        }

        foreach (var rb in bricks)
        {
            rb.isKinematic = false;
            rb.AddExplosionForce(explosionForce, hitPoint, explosionRadius);

            // Плавное удаление кирпичей через 3 секунды
            Destroy(rb.gameObject, 3f);
        }

        Destroy(gameObject, 3.1f);
    }
}
