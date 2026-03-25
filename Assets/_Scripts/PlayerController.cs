using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum PlayerShape { Cube, Sphere}

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float startSpeed = 10f;
    public float speedIncrease = 0.1f;
    public float maxSpeed = 30f;
    public float sideSpeed = 10f;
    public float laneDistance = 3f;

    public float currentForwardSpeed;
    private int targetLane = 1;

    [Header("Shapes & Models")]
    public PlayerShape currentShape = PlayerShape.Cube;
    public GameObject cubeModel, sphereModel;
    public GameObject EffectPrefab;

    [Header("UI & Score")]
    public TMP_Text scoreText;
    private int coinsCollected = 0;

    [Header("Visual Juice")]
    public float sphereRadius = 0.5f;
    public float tiltAmount = 15f; // Сила наклона куба
    public float tiltSpeed = 10f;  // Скорость наклона
    private float currentSphereRotation = 0f;


    private Vector2 startTouchPosition;
    private Rigidbody rb;



    void Start()
    {
        rb = GetComponent<Rigidbody>();
        currentForwardSpeed = startSpeed;

        // Гарантируем правильные настройки при старте
        SetShape((int)currentShape);
        UpdateScoreUI();



        // Загружаем сохраненный материал
        if (InventoryManager.Instance != null)
        {
            Material savedMat = InventoryManager.Instance.GetSavedSkinMaterial();
            ApplySkin(savedMat);
        }
    }

    void Update()
    {

        // 1. ПРОВЕРКА: Если игра еще не нажата в меню — стоим на месте
        if (LevelManager.Instance != null && !LevelManager.Instance.isGameStarted)
            return;


        HandleSwipe();
        AnimateVisuals();

        if (currentForwardSpeed < maxSpeed)
        {
            // Ускорение можно сделать чуть быстрее в начале и медленнее в конце
            // Но оставим пока стабильное, просто увеличим коэффициент
            currentForwardSpeed += speedIncrease * Time.deltaTime;
        }
    }


    void FixedUpdate()
    {
        if (LevelManager.Instance != null && !LevelManager.Instance.isGameStarted)
        {
            rb.velocity = Vector3.zero;
            return;
        }

        // 1. Твои расчеты (оставляем как есть)
        float shapeMultiplier = (currentShape == PlayerShape.Sphere) ? 1.3f : 0.85f;
        float finalSpeed = currentForwardSpeed * shapeMultiplier;

        // 2. ВАЖНО: Ограничиваем скорость вперед твоим sideSpeed, 
        // чтобы повторить эффект твоего старого MoveTowards
        float cappedSpeed = Mathf.Min(finalSpeed, sideSpeed);

        // 3. Расчет движения по X (смена полос)
        float targetX = (targetLane - 1) * laneDistance;
        float xDiff = targetX - rb.position.x;
        // Рассчитываем скорость по X, чтобы успеть доехать за нужное время
        float xVelocity = xDiff / Time.fixedDeltaTime;
        // Ограничиваем боковую скорость, чтобы не было резких прыжков
        xVelocity = Mathf.Clamp(xVelocity, -sideSpeed, sideSpeed);

        // 4. ПРИМЕНЯЕМ VELOCITY (Это уберет "шаги" и телепорты)
        // Мы не используем MovePosition, мы даем импульс.
        rb.velocity = new Vector3(xVelocity, rb.velocity.y, cappedSpeed);

        // 5. Убираем вращение, чтобы шар не спотыкался о трение
        rb.angularVelocity = Vector3.zero;
    }

    private void HandleSwipe()
    {
        if (Input.GetMouseButtonDown(0)) startTouchPosition = Input.mousePosition;
        if (Input.GetMouseButtonUp(0))
        {
            float xDiff = Input.mousePosition.x - startTouchPosition.x;
            if (Mathf.Abs(xDiff) > 50)
            {
                if (xDiff > 0 && targetLane < 2) targetLane++;
                else if (xDiff < 0 && targetLane > 0) targetLane--;
            }
        }
    }

    private void AnimateVisuals()
    {
        if (currentShape == PlayerShape.Sphere && sphereModel != null)
        {
            // 1. ШАР: Качение вперед
            // Угол поворота рассчитывается через длину дуги: $$ \Delta\theta = \frac{\text{distance}}{\text{radius}} $$
            float distanceTravelled = (currentForwardSpeed * (currentShape == PlayerShape.Sphere ? 1.3f : 1f)) * Time.deltaTime;
            float rotationInput = (distanceTravelled / sphereRadius) * Mathf.Rad2Deg;

            currentSphereRotation += rotationInput;
            sphereModel.transform.localRotation = Quaternion.Euler(currentSphereRotation, 0, 0);
        }
        else if (currentShape == PlayerShape.Cube && cubeModel != null)
        {
            // 2. КУБ: Наклон при смене полосы
            // Считаем разницу между текущим X и целевым X полосы
            float targetX = (targetLane - 1) * laneDistance;
            float xDiff = targetX - transform.position.x;

            // Чем больше расстояние до цели, тем сильнее наклон
            float targetTilt = -xDiff * tiltAmount;

            // Плавное стремление к наклону
            float currentTilt = Mathf.LerpAngle(cubeModel.transform.localRotation.eulerAngles.z, targetTilt, Time.deltaTime * tiltSpeed);

            // "Дыхание" куба (если хочешь оставить)
            float breathe = Mathf.Sin(Time.time * 8f) * 0.05f;
            cubeModel.transform.localScale = new Vector3(1 + breathe, 1 - breathe, 1 + breathe);

            cubeModel.transform.localRotation = Quaternion.Euler(0, 0, currentTilt);
        }
    }

    public void SetShape(int shapeIndex)
    {
        currentShape = (PlayerShape)shapeIndex;

        // Визуал
        if (cubeModel) cubeModel.SetActive(currentShape == PlayerShape.Cube);
        if (sphereModel) sphereModel.SetActive(currentShape == PlayerShape.Sphere);

        // Теги для ShapeChecker
        switch (currentShape)
        {
            case PlayerShape.Cube: gameObject.tag = "PlayerCube"; break;
            case PlayerShape.Sphere: gameObject.tag = "PlayerSphere"; break;
            
        }
    }

    public void ApplySkin(Material newMaterial)
    {
        if (cubeModel) cubeModel.GetComponent<Renderer>().material = newMaterial;
        if (sphereModel) sphereModel.GetComponent<Renderer>().material = newMaterial;
    }

    // Обновленный метод для камеры (учитывает множитель формы)
    public float GetNormalizedSpeed()
    {
        float shapeMultiplier = (currentShape == PlayerShape.Sphere) ? 1.1f : 0.85f;
        float realSpeed = currentForwardSpeed * shapeMultiplier;

        // Ограничиваем значение от 0 до 1, чтобы FOV не "ломался"
        float minPossibleSpeed = startSpeed * 0.85f;
        float maxPossibleSpeed = maxSpeed * 1.1f;

        return Mathf.Clamp01((realSpeed - minPossibleSpeed) / (maxPossibleSpeed - minPossibleSpeed));
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null) scoreText.text = coinsCollected.ToString();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Coin"))
        {
            // Передаем команду менеджеру, чтобы он прибавил монету в общий счет забега
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.AddCoin();
                AudioManager.Instance.PlaySFX(AudioManager.Instance.coinSound);
            }

           
            Destroy(other.gameObject);

        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        string otherTag = collision.gameObject.tag;

        // 1. СЛАБАЯ СТЕНА (Кирпичи)
        if (otherTag == "WeakObstacle")
        {
            if (currentShape == PlayerShape.Cube)
            {
                BreakableWall breakable = collision.transform.GetComponentInParent<BreakableWall>();
                if (breakable != null)
                {
                    breakable.Shatter(collision.contacts[0].point);
                    return; // Проходим сквозь
                    
                }
            }
           
        }

        // 2. ОБЫЧНАЯ ПРЕГРАДА / СТЕНА С ДЫРКОЙ
        if (otherTag == "Obstacle")
        {
            ShapeWall wall = collision.gameObject.GetComponent<ShapeWall>();

            // Если это стена с формой и форма совпадает — не умираем (физика пропустит через дырку)
            if (wall != null && wall.requiredShape == currentShape)
            {
               
                return;
            }

            LevelManager.Instance.GameOver();
        }
    }



    
}