using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollisionHandler : MonoBehaviour
{
    private PlayerShapeController shapeController;

    private void Awake()
    {
        shapeController = GetComponent<PlayerShapeController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Coin"))
        {
            LevelManager.Instance?.AddCoin();
            AudioManager.Instance?.PlaySFX(AudioManager.Instance.coinSound);
            Destroy(other.gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        string tag = collision.gameObject.tag;

        if (tag == "WeakObstacle" && shapeController.CurrentShape == PlayerShape.Cube)
        {
            BreakableWall bw = collision.transform.GetComponentInParent<BreakableWall>();
            if (bw != null)
            {
                bw.Shatter(collision.contacts[0].point);
                return;
            }
        }

        if (tag == "Obstacle")
        {
            ShapeWall wall = collision.gameObject.GetComponent<ShapeWall>();
            if (wall != null && wall.requiredShape == shapeController.CurrentShape)
                return;
            // Death handled by LiquidDeath or LevelManager
        }
    }
}
