using Unity.VisualScripting;
using UnityEngine;
using TMPro;

public class UIStateManager : MonoBehaviour
{
    public enum UIState
    {
        MainMenu,
        InGame,
        Paused,
        Options,
        GameOver
    }

    [Header("UI Panels")]

    [SerializeField] private GameObject inGamePanel;
    [SerializeField] private GameObject pausedPanel;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject gameOverPanel;

    [Header("Info")]

    [SerializeField] private TextMeshProUGUI txtStateDebug;

    private UIState currentState;

    private void Start()
    {
        changeState(UIState.InGame);
    }

    private void Update()
    {

        /*
           if(Keyboard.current!=null && Keyboard.current.escapeKey.wasPressedThisFrame)
           if (Keyboard.current.pKey.wasPressedThisFrame)
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
        */

        // For testing purposes: Press Escape to toggle pause
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
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

        if (inGamePanel != null) inGamePanel.SetActive(false);
        if (pausedPanel != null) pausedPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

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
            case UIState.GameOver:
                if (gameOverPanel != null) gameOverPanel.SetActive(true);
                break;
        }

        if (txtStateDebug != null)
        {
            txtStateDebug.text = $"State : {(currentState)}";
        }

    }

    public void OnClickPause()
    {
        changeState(UIState.Paused);
    }
    public void OnClickResume()
    {
        changeState(UIState.InGame);
    }
    public void OnBackToMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

}
