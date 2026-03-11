using UnityEngine;
using TMPro;
using UnityEngine.InputSystem; // Usando el nuevo sistema de Input para consistencia

public class UIStateManager : MonoBehaviour
{
    // [MODIFICADO] Eliminamos GameOver de aquí, ya que GameFlowManager lo gestionará
    public enum UIState
    {
        InGame,
        Paused,
        Options
    }

    [Header("UI Panels")]
    [SerializeField] private GameObject inGamePanel;
    [SerializeField] private GameObject pausedPanel;
    [SerializeField] private GameObject optionsPanel;
    // [BORRADO] El gameOverPanel ya no se asigna aquí, se asigna en el GameFlowManager

    [Header("Info")]
    [SerializeField] private TextMeshProUGUI txtStateDebug;

    private UIState currentState;

    private void Start()
    {
        // Iniciamos en InGame
        changeState(UIState.InGame);
    }

    private void Update()
    {
        // [OPTIMIZADO] Usamos el nuevo Input System como en tu PlayerStateManager para evitar mezclar sistemas
        if (Keyboard.current != null && (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.pKey.wasPressedThisFrame))
        {
            if (currentState == UIState.InGame)
            {
                OnClickPause();
            }
            else if (currentState == UIState.Paused)
            {
                OnClickResume();
            }
        }
    }

    public void changeState(UIState nextState)
    {
        currentState = nextState;

        // Desactivamos paneles antes de prender el correcto
        if (inGamePanel != null) inGamePanel.SetActive(false);
        if (pausedPanel != null) pausedPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);

        switch (currentState)
        {
            case UIState.InGame:
                if (inGamePanel != null) inGamePanel.SetActive(true);
                Time.timeScale = 1f; // Reanuda el juego    
                break;
            case UIState.Paused:
                if (pausedPanel != null) pausedPanel.SetActive(true);
                Time.timeScale = 0f; // Pausa el juego
                break;
            case UIState.Options:
                if (optionsPanel != null) optionsPanel.SetActive(true);
                break;
        }

        if (txtStateDebug != null)
        {
            txtStateDebug.text = $"UI State: {currentState}";
        }
    }

    // --- Métodos públicos para botones ---

    public void OnClickPause()
    {
        changeState(UIState.Paused);
    }

    public void OnClickResume()
    {
        changeState(UIState.InGame);
    }

    public void OnClickOptions()
    {
        changeState(UIState.Options);
    }

    public void OnBackToMenu()
    {
        // Asegúrate de que el tiempo regrese a 1 antes de cambiar de escena
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}