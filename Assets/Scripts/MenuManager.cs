using TMPro;
using UnityEngine;
using UnityEngine.XR;
using static UIStateManager;

public class MenuManager : MonoBehaviour
{
    public enum MenuState
    {
        MainMenu,
        Options,
        Credits,
        ExitConfirmation
    }

    public MenuState currentState;

    [Header("UI Panels")]

    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject optionsPanel; 
    [SerializeField] private GameObject creditsPanel;

    [Header("Info")]

    [SerializeField] private TextMeshProUGUI txtStateDebug;

    private void Start()
    {
        changeState(MenuState.MainMenu);

    }

    void Update()
    {
        
    }

    public void changeState(MenuState newState)
    {
        currentState = newState;

        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);  
        if (creditsPanel != null) creditsPanel.SetActive(false);

        switch (currentState)
        {
            case MenuState.MainMenu:
                if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
                break;
                case MenuState.Options:
                if (optionsPanel != null) optionsPanel.SetActive(true);
                break;
                case MenuState.Credits:
                if (creditsPanel != null) creditsPanel.SetActive(true);
                break;
        }

        if (txtStateDebug != null)
        {
            txtStateDebug.text = $"State:  {(currentState)}";
        }
    }

    public void OnClickStart()
    {
        // cargar la escena del juego
            UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
    }
    public void OnClickOptions()
    {
        changeState(MenuState.Options);
    }
    public void OnClickBack()
    {
        changeState(MenuState.MainMenu);
    }

    public void OnClickExit()
           {

           if (Application.isEditor)
              {
            UnityEditor.EditorApplication.isPlaying = false;
              }
           else
              {
            Application.Quit();
              }

        /*
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit(); 
        #endif
       */
           }
    }
