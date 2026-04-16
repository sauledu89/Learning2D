using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text scoreText;

    [Header("Current Score")]
    [SerializeField] private int score = 0;

    [Header("Survival Score")]
    [SerializeField] private int survivalScorePerTick = 1;
    [SerializeField] private float survivalTickInterval = 1.0f;

    private float survivalTimer = 0f;

    private void Start() => UpdateUI();

    private void Update()
    {
        if (GameFlowManager.Instance == null || !GameFlowManager.Instance.IsGameplay) return;

        survivalTimer += Time.deltaTime;
        if (survivalTimer >= survivalTickInterval)
        {
            survivalTimer -= survivalTickInterval;
            AddScore(survivalScorePerTick);
        }
    }

    public void AddScore(int amount)
    {
        if (amount <= 0) return;
        score += amount;
        UpdateUI();
    }

    // MÉTODOS DE BONUS CON IMPRESIÓN EN CONSOLA
    // Dentro de ScoreManager.cs

    public void AddVictoryBonus(int amount)
    {
        Debug.Log("<color=green>[SCORE] Bonus por Victoria: +" + amount + "</color>");
        AddScore(amount); // Suma el valor a la variable score
    }

    public void AddHealthBonus(int currentHP, int pointsPerHP)
    {
        int bonusValue = currentHP * pointsPerHP;
        Debug.Log("<color=cyan>[SCORE] Bonus por Vida: " + currentHP + " HP x " + pointsPerHP + " pts = +" + bonusValue + "</color>");
        AddScore(bonusValue);

        // Este Log confirmará que el score final subió
        Debug.Log("<color=yellow>[SCORE] TOTAL FINAL CALCULADO: " + score + "</color>");
    }

    private void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }
}