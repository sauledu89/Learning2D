using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; // Necesario para detectar la tecla Esc/P

public class GameFlowManager : MonoBehaviour
{
    // Añadimos el estado Paused
    public enum GameState { Gameplay, GameOver, Victory, Paused, Options }

    public static GameFlowManager Instance { get; private set; }

    [Header("State (read-only)")]
    [SerializeField] private GameState currentState = GameState.Gameplay;
    public GameState CurrentState => currentState;
    public bool IsGameplay => currentState == GameState.Gameplay;

    [Header("Controllers to Disable")]
    [SerializeField] private MonoBehaviour playerController;
    [SerializeField] private List<MonoBehaviour> enemyControllers = new List<MonoBehaviour>();
    [SerializeField] private CameraFollow cameraFollow;

    [Header("UI Panels")]
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject pausePanel; // Panel de Pausa
    [SerializeField] private GameObject optionsPanel; // Panel de Opciones
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject victoryPanel;

    [Header("Delays (seconds)")]
    [SerializeField] private float gameOverDelay = 1.0f;
    [SerializeField] private float victoryDelay = 0.5f;

    private Coroutine endRoutine;

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
        ResumeGame(); // Asegura que el juego inicie normal
    }

    private void Update()
    {
        // Detectar tecla de Pausa (Esc o P) usando el nuevo Input System
        if (Keyboard.current != null && (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.pKey.wasPressedThisFrame))
        {
            if (currentState == GameState.Gameplay)
                PauseGame();
            else if (currentState == GameState.Paused || currentState == GameState.Options)
                ResumeGame();
        }
    }

    // --- LÓGICA DE PAUSA ---

    public void PauseGame()
    {
        if (!IsGameplay) return; // No pausar si ya morimos o ganamos

        SetState(GameState.Paused);
        Time.timeScale = 0f; // Congela el motor de física y tiempo
        ShowPanels(pause: true);
    }

    public void ResumeGame()
    {
        // Solo podemos reanudar si estamos en Pausa u Opciones
        if (currentState != GameState.Paused && currentState != GameState.Options && currentState != GameState.Gameplay) return;

        SetState(GameState.Gameplay);
        Time.timeScale = 1f; // Devuelve el tiempo a la normalidad
        ShowPanels(gameplay: true);
    }

    public void OpenOptions()
    {
        SetState(GameState.Options);
        ShowPanels(options: true);
    }

    // --- LÓGICA DE FIN DE PARTIDA ---

    public void RequestGameOver()
    {
        if (!IsGameplay) return;
        if (endRoutine != null) StopCoroutine(endRoutine);
        endRoutine = StartCoroutine(EndRoutine(GameState.GameOver, gameOverDelay));
    }

    public void RequestVictory()
    {
        if (!IsGameplay) return;
        if (endRoutine != null) StopCoroutine(endRoutine);
        endRoutine = StartCoroutine(EndRoutine(GameState.Victory, victoryDelay));
    }

    private IEnumerator EndRoutine(GameState endState, float delay)
    {
        DisableGameplayControllers();
        yield return new WaitForSecondsRealtime(Mathf.Max(0f, delay));

        SetState(endState);
        if (cameraFollow != null) cameraFollow.SetFrozen(true);

        if (endState == GameState.GameOver) ShowPanels(gameOver: true);
        else ShowPanels(victory: true);
    }

    private void DisableGameplayControllers()
    {
        if (playerController != null) playerController.enabled = false;
        foreach (var enemy in enemyControllers)
        {
            if (enemy != null) enemy.enabled = false;
        }
    }

    private void SetState(GameState newState) => currentState = newState;

    // Sistema de paneles centralizado
    private void ShowPanels(bool gameplay = false, bool pause = false, bool options = false, bool gameOver = false, bool victory = false)
    {
        if (hudPanel != null) hudPanel.SetActive(gameplay);
        if (pausePanel != null) pausePanel.SetActive(pause);
        if (optionsPanel != null) optionsPanel.SetActive(options);
        if (gameOverPanel != null) gameOverPanel.SetActive(gameOver);
        if (victoryPanel != null) victoryPanel.SetActive(victory);
    }

    // --- BOTONES DE UI ---

    public void RestartScene() // Quick Restart
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0); // Carga la escena con índice 0 (Menú)
    }

    public void QuitToDesktop()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }
}