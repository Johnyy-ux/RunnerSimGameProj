using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    private PlayerMovement movement;
    private Vector2 swipeStart;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        if (!LevelManager.Instance.isGameStarted) return;

        HandleMouseSwipe(); // Для тестирования на ПК
        // HandleTouchSwipe(); // можно раскомментировать позже
    }

    private void HandleMouseSwipe()
    {
        if (Input.GetMouseButtonDown(0))
            swipeStart = Input.mousePosition;

        if (Input.GetMouseButtonUp(0))
        {
            Vector2 delta = (Vector2)Input.mousePosition - swipeStart;

            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y)) // Horizontal swipe
            {
                if (delta.x > 40f) movement.ChangeLane(1);
                else if (delta.x < -40f) movement.ChangeLane(-1);
            }
            else if (delta.y > 60f) // Vertical swipe up
            {
                movement.Jump();
            }
        }
    }

    // Можно добавить позже для лучшей мобильной поддержки
    // private void HandleTouchSwipe() { ... }
}
