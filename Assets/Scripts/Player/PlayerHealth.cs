using UnityEngine;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    [Header("HP")]
    [SerializeField] private int maxHP = 10;
    [SerializeField] private int currentHP;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI txtHP;

    [Header("Game Over")]
    [SerializeField] private GameObject panelGameOver;

    private void Start()
    {
        currentHP = maxHP;
        UpdateHPUI();

        if (panelGameOver != null)
            panelGameOver.SetActive(false);
    }

    public void TakeDamage(int amount)
    {
        currentHP -= amount;
        if (currentHP < 0)
            currentHP = 0;
        UpdateHPUI();
        if (currentHP == 0)
            GameOver();
    }

    private void UpdateHPUI()
    {
        if (txtHP != null)
            txtHP.text = $"HP: {currentHP}/{maxHP}";
    }

    private void GameOver()
    {
        Debug.Log("[PlayerHealth] Game Over");
        if (panelGameOver != null)
        {
            // Pausar el tiempo para que el juego se detenga
            Time.timeScale = 0f;
        }
        panelGameOver.SetActive(true);
        // Aquí podrías agregar lógica adicional, como detener el tiempo, mostrar un menú, etc.
    }
}
