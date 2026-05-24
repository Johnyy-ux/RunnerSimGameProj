using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiquidDeath : MonoBehaviour
{
    [Header("Death Effects")]
    public GameObject paintBlobPrefab;
    public float squashSpeed = 15f;
    [Range(0.5f, 3f)] public float splatRadius = 1.5f;

    private bool isDead = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (isDead || !collision.gameObject.CompareTag("Obstacle"))
            return;

        isDead = true;

        ContactPoint contact = collision.contacts[0];
        Color playerColor = GetPlayerColor();

        FreezePlayer();
        CreateSplat(contact, playerColor);
        StartCoroutine(SquashAndHide(contact.normal));

        // Camera death sequence
        var dynCam = Camera.main?.GetComponent<DynamicCamera>();
        if (dynCam != null)
            dynCam.StartDeathSequence(contact.point);
        else
            LevelManager.Instance?.ShowGameOver();
    }

    private Color GetPlayerColor()
    {
        var rend = GetComponentInChildren<Renderer>();
        return rend != null ? rend.material.color : Color.white;
    }

    private void FreezePlayer()
    {
        if (TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
        }
    }

    private void CreateSplat(ContactPoint contact, Color color)
    {
        if (paintBlobPrefab == null) return;

        int pieces = Random.Range(6, 10);
        Quaternion rotation = Quaternion.LookRotation(contact.normal);

        for (int i = 0; i < pieces; i++)
        {
            Vector2 randomPoint = Random.insideUnitCircle * splatRadius;
            Vector3 offset = rotation * new Vector3(randomPoint.x, randomPoint.y, 0f);

            Vector3 spawnPos = contact.point + contact.normal * 0.01f + offset;

            GameObject blob = Instantiate(paintBlobPrefab, spawnPos, rotation);
            float scale = Random.Range(0.4f, 1.2f);
            blob.transform.localScale = new Vector3(scale, scale, paintBlobPrefab.transform.localScale.z);
            blob.transform.Rotate(Vector3.forward, Random.Range(0, 360));

            var rend = blob.GetComponentInChildren<Renderer>();
            if (rend != null) rend.material.color = color;

            blob.transform.SetParent(contact.otherCollider.transform);
        }
    }

    private IEnumerator SquashAndHide(Vector3 wallNormal)
    {
        Vector3 startScale = transform.localScale;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * squashSpeed;
            transform.localScale = Vector3.Lerp(startScale, new Vector3(1.8f, 1.8f, 0.001f), t);
            yield return null;
        }

        // Hide visuals
        foreach (var rend in GetComponentsInChildren<Renderer>())
            rend.enabled = false;

        if (TryGetComponent<Collider>(out Collider col))
            col.enabled = false;
    }
}
