using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using TMPro;
public enum GamePhase { Tutorial, Standard, ShapeChallenge, Rest }

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Status")]
    public bool isGameStarted = false;
    public float distanceTravelled;

    [Header("Phases")]
    public GamePhase currentPhase = GamePhase.Tutorial;

    [Header("UI")]
    public GameObject mainMenuPanel;
    public GameObject gameHUDPanel;
    public GameObject loseUI;

    [Header("UI Text")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI phaseText;

    public Transform playerTransform;

    private int sessionCoins = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        Time.timeScale = 1f;
        mainMenuPanel.SetActive(true);
        gameHUDPanel.SetActive(false);
        UpdateCoinDisplay(false);
    }

    private void Update()
    {
        if (!isGameStarted || playerTransform == null) return;

        distanceTravelled = playerTransform.position.z;
        scoreText.text = $"{Mathf.FloorToInt(distanceTravelled)}m";

        UpdatePhase();

        if (playerTransform.position.y < -5f)
            ShowGameOver();
    }

    // ── Phase System ─────────────────────────────────────────────────────────
    private void UpdatePhase()
    {
        GamePhase newPhase = CalculateCurrentPhase();

        if (newPhase != currentPhase)
            SetPhase(newPhase);
    }

    private GamePhase CalculateCurrentPhase()
    {
        if (distanceTravelled < 100f)
            return GamePhase.Tutorial;

        float cycle = (distanceTravelled - 100f) % 500f;

        if (cycle < 200f) return GamePhase.Standard;
        if (cycle < 400f) return GamePhase.ShapeChallenge;
        return GamePhase.Rest;
    }

    private void SetPhase(GamePhase newPhase)
    {
        currentPhase = newPhase;
        if (phaseText) phaseText.text = newPhase.ToString();
    }

    // ── Game Flow ────────────────────────────────────────────────────────────
    public void StartGame()
    {
        isGameStarted = true;
        mainMenuPanel.SetActive(false);
        gameHUDPanel.SetActive(true);
    }

    public void ShowGameOver()
    {
        isGameStarted = false;
        SaveProgress();
        if (loseUI) loseUI.SetActive(true);
    }

    public void GoToMenu()
    {
        if (isGameStarted) SaveProgress();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Restart() => SceneManager.LoadScene(0);

    public void AddCoin()
    {
        sessionCoins++;
        if (coinText) coinText.text = sessionCoins.ToString();
    }

    // ── Save & Display ───────────────────────────────────────────────────────
    private void SaveProgress()
    {
        // Total coins
        int total = PlayerPrefs.GetInt("TotalCoins", 0);
        PlayerPrefs.SetInt("TotalCoins", total + sessionCoins);

        // High score
        float highScore = PlayerPrefs.GetFloat("HighScore", 0f);
        if (distanceTravelled > highScore)
            PlayerPrefs.SetFloat("HighScore", distanceTravelled);

        PlayerPrefs.Save();
    }

    public void UpdateCoinDisplay(bool showTotal = false)
    {
        if (!coinText) return;

        if (showTotal)
            coinText.text = PlayerPrefs.GetInt("TotalCoins", 0).ToString();
        else
            coinText.text = sessionCoins.ToString();
    }
}