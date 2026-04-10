using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using TMPro;
public enum GamePhase { Tutorial, Standard, ShapeChallenge, Rest }

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("Phase Settings")]
    public GamePhase currentPhase = GamePhase.Tutorial;
    public float phaseDistance = 200f; // Длина каждой фазы в метрах

    [Header("Status")]
    public bool isGameStarted = false;
    public float distanceTravelled;

    [Header("Movement Settings")]
    public float baseSpeed = 10f;
    public float maxSpeed = 25f;
    public float speedMultiplier = 0.05f; // Насколько сильно растет скорость от дистанции
    public float currentSpeed;

    [Header("UI Panels")]
    public GameObject mainMenuPanel;
    public GameObject gameHUDPanel;
    public GameObject loseUI;

    [Header("UI Text")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI phaseText; // Чтобы игрок видел, что режим сменился

    public Transform playerTransform;
    private int sessionCoins = 0;

    void Awake() => Instance = this;

    void Start()
    {
        Time.timeScale = 1f;
        UpdateCoinDisplay();
        mainMenuPanel.SetActive(true);
        gameHUDPanel.SetActive(false);
    }

    void Update()
    {
        if (!isGameStarted || playerTransform == null) return;

        distanceTravelled = playerTransform.position.z;
        scoreText.text = ((int)distanceTravelled).ToString() + "m";

        UpdatePhaseLogic();

        currentSpeed = Mathf.MoveTowards(currentSpeed, maxSpeed, speedMultiplier * Time.deltaTime);

        if (playerTransform.position.y < -5f) ShowGameOver();
    }

    // ПРОДВИНУТАЯ ЛОГИКА ФАЗ
    private void UpdatePhaseLogic()
    {
        if (distanceTravelled < 100f)
        {
            currentPhase = GamePhase.Tutorial;
        }
        else
        {
            // Берем остаток от деления всей дистанции на общую длину цикла фаз
            // Например, цикл: Standard(200) + Shape(200) + Rest(100) = 500м.
            float cyclePos = (distanceTravelled - 100f) % 500f;

            if (cyclePos < 200f) SetPhase(GamePhase.Standard);
            else if (cyclePos < 400f) SetPhase(GamePhase.ShapeChallenge);
            else SetPhase(GamePhase.Rest);
        }
    }

    private void SetPhase(GamePhase newPhase)
    {
        if (currentPhase == newPhase) return;
        currentPhase = newPhase;

        // Визуальное оповещение о смене фазы (по желанию)
        if (phaseText) phaseText.text = currentPhase.ToString();
        Debug.Log("Смена фазы на: " + currentPhase);
    }

    public void StartGame()
    {
        isGameStarted = true;
        mainMenuPanel.SetActive(false);
        gameHUDPanel.SetActive(true);
    }

    public void GoToMenu()
    {
        // Если мы в проигрыше, сохраняем данные и идем в меню
        if (isGameStarted) SaveData();

        // Перезагрузка сцены вернет нас в Start(), где включится панель меню
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ShowGameOver()
    {
        isGameStarted = false;
        SaveData();
        if (loseUI) loseUI.SetActive(true);
    }

    private void SaveData()
    {
        int total = PlayerPrefs.GetInt("TotalCoins", 0);
        PlayerPrefs.SetInt("TotalCoins", total + sessionCoins);

        float high = PlayerPrefs.GetFloat("HighScore", 0);
        if (distanceTravelled > high) PlayerPrefs.SetFloat("HighScore", distanceTravelled);

        PlayerPrefs.Save();
    }

    public void AddCoin()
    {
        sessionCoins++;
        coinText.text = sessionCoins.ToString();
    }

    public void UpdateCoinDisplay()
    {
        // Общий баланс монет
    }

    public void Restart() => UnityEngine.SceneManagement.SceneManager.LoadScene(0);
}