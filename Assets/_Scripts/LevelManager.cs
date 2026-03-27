using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using TMPro;
public enum GamePhase { Tutorial, Standard, ShapeChallenge, Rest }

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("Level Configuration")]
    public LevelData[] levels;
    private LevelData currentData;

    [Header("Live Progress")]
    public int currentLevel = 1;
    public float levelDistance; // Теперь это значение будет перезаписано
    public float distanceTravelled;
    public bool isGameStarted = false;
    public GamePhase currentPhase = GamePhase.Standard;

    [Header("UI Panels")]
    public GameObject winUI;
    public GameObject loseUI;
    public GameObject mainMenuPanel;
    public GameObject gameHUDPanel;
    public GameObject levelSelectPanel;

    [Header("UI Text")]
    public TextMeshProUGUI menuCoinText;
    public TextMeshProUGUI gameCoinText;
    public TextMeshProUGUI LevelDisplayText;

    [Header("References")]
    public Transform playerTransform;

    private int coinsCollectedInRun = 0;
    private static bool shouldAutoStart = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        // ТЕСТОВЫЙ СБРОС: Раскомментируй строку ниже ОДИН РАЗ, запусти игру, и закомментируй обратно.
        // PlayerPrefs.DeleteAll(); 

        currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);

        // ПРОВЕРКА НА ОВЕРФЛОУ: Если уровни кончились, играем последний доступный
        if (currentLevel > levels.Length)
        {
            Debug.Log("Все уровни пройдены! Зацикливаем на последнем.");
            currentLevel = levels.Length;
        }

        LoadLevelSettings();
    }

    void LoadLevelSettings()
    {
        if (levels == null || levels.Length == 0)
        {
            Debug.LogError("Список уровней пуст в LevelManager!");
            return;
        }

        // Берем данные файла (уровень 1 — это индекс 0)
        int index = Mathf.Clamp(currentLevel - 1, 0, levels.Length - 1);
        currentData = levels[index];

        // ГЛАВНОЕ: перезаписываем дистанцию из ScriptableObject
        levelDistance = currentData.distanceToFinish;

        Debug.Log($"Загружен уровень {currentLevel}. Дистанция: {levelDistance}м.");
    }

    void Start()
    {
        // Всегда сбрасываем время при старте сцены
        Time.timeScale = 1f;

        UpdateMenuUI();

        UpdateMenuCoinDisplay();
        if (LevelDisplayText) LevelDisplayText.text = "Level " + currentLevel;
        gameCoinText.text = "0";

        if (shouldAutoStart)
        {
            shouldAutoStart = false;
            StartGame();
        }
        else
        {
            mainMenuPanel.SetActive(true);
            gameHUDPanel.SetActive(false);
        }
    }

    void Update()
    {
        if (playerTransform == null || !isGameStarted) return;

        distanceTravelled = playerTransform.position.z;
        UpdatePhases();

        if (playerTransform.position.y < -5f) ShowGameOverUI();
    }

    public void UpdateMenuUI()
    {
        // Показываем текущий уровень в главном меню
        if (LevelDisplayText != null)
        {
            LevelDisplayText.text = "LEVEL " + currentLevel;
        }

        // Показываем уровень в игровом HUD (если есть)
        if (LevelDisplayText != null)
        {
            LevelDisplayText.text = "Level " + currentLevel;
        }

        UpdateMenuCoinDisplay();
    }

    // --- ЛОГИКА КНОПОК ---

    public void StartGame()
    {
        isGameStarted = true;
        mainMenuPanel.SetActive(false);
        gameHUDPanel.SetActive(true);
        Time.timeScale = 1f;
    }

    public void OpenLevelSelect()
    {
        if (levelSelectPanel != null)
        {
            levelSelectPanel.SetActive(true);
            mainMenuPanel.SetActive(false); // Прячем главное меню, чтобы не мешало
        }
    }

    // Метод для кнопки "Back" или "Close" внутри панели уровней
    public void CloseLevelSelect()
    {
        if (levelSelectPanel != null)
        {
            levelSelectPanel.SetActive(false);
            mainMenuPanel.SetActive(true); // Возвращаем главное меню
        }
    }

    public void SelectLevel(int levelIndex)
    {
        currentLevel = levelIndex;
        PlayerPrefs.SetInt("CurrentLevel", currentLevel);
        PlayerPrefs.Save();

        Time.timeScale = 1f;

        // ГЛАВНОЕ ИЗМЕНЕНИЕ: Ставим false, чтобы игра просто загрузилась в меню
        shouldAutoStart = false;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void RestartLevel()
    {
        shouldAutoStart = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void NextLevel()
    {
        // Мы уже прибавили уровень в WinLevel(), просто перезагружаем
        shouldAutoStart = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMenu()
    {
        shouldAutoStart = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // --- СОСТОЯНИЯ ИГРЫ ---

    public void WinLevel()
    {
        if (!isGameStarted) return;
        isGameStarted = false;

        SaveCoinsToMemory();

        // Повышаем уровень и сохраняем
        currentLevel++;
        PlayerPrefs.SetInt("CurrentLevel", currentLevel);
        PlayerPrefs.Save();

        Time.timeScale = 0.5f; // Замедление для эффекта
        if (winUI) winUI.SetActive(true);
    }

    public void HandlePlayerDeath()
    {
        if (!isGameStarted) return;
        isGameStarted = false;
        
        Time.timeScale = 0.7f;
        SaveCoinsToMemory();
    }

    // 2. Этот метод вызовет КАМЕРА, когда долетит обратно
    public void ShowGameOverUI()
    {
        // Возвращаем нормальное время для работы UI анимаций
        Time.timeScale = 1f;
        if (loseUI) loseUI.SetActive(true);
    }

    // --- ВСПОМОГАТЕЛЬНОЕ ---

    public EnvironmentType GetCurrentLocation()
    {
        return currentData != null ? currentData.locationType : EnvironmentType.Forest;
    }

    private void UpdatePhases()
    {
        float progress = distanceTravelled / levelDistance;
        if (progress < 0.1f) currentPhase = GamePhase.Tutorial;
        else if (progress > 0.9f) currentPhase = GamePhase.Rest;
        else
        {
            currentPhase = ((int)(distanceTravelled / 100) % 2 == 0) ?
                GamePhase.Standard : GamePhase.ShapeChallenge;
        }
    }

    public void AddCoin()
    {
        coinsCollectedInRun++;
        gameCoinText.text = coinsCollectedInRun.ToString();
    }

    public void SaveCoinsToMemory()
    {
        int savedTotal = PlayerPrefs.GetInt("TotalCoins", 0);
        PlayerPrefs.SetInt("TotalCoins", savedTotal + coinsCollectedInRun);
        PlayerPrefs.Save();
        coinsCollectedInRun = 0;
    }

    public void UpdateMenuCoinDisplay()
    {
        int total = PlayerPrefs.GetInt("TotalCoins", 0);
        if (menuCoinText != null) menuCoinText.text = total.ToString();
    }
}