using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("UI en partida")]
    [SerializeField] private TMP_Text scoreText;

    [Header("Puntuación base")]
    [SerializeField] private int survivalScorePerTick = 1;
    [SerializeField] private float survivalTickInterval = 1.0f;

    [Header("Kills")]
    [SerializeField] private int pointsPerKill = 50;

    // ── Totales ─────────────────────────────────────────────
    private int score = 0;

    // ── Desglose por categoría ───────────────────────────────
    private int survivalScore = 0;
    private int collectibleScore = 0;
    private int killScore = 0;
    private int victoryBonusScore = 0;
    private int healthBonusScore = 0;

    // ── Contadores extra ─────────────────────────────────────
    private int collectiblesCount = 0;
    private int enemiesKilled = 0;
    private float survivalTime = 0f;
    private float survivalTimer = 0f;

    // ── Propiedades públicas (para el panel de victoria) ─────
    public int Score => score;
    public int SurvivalScore => survivalScore;
    public int CollectibleScore => collectibleScore;
    public int KillScore => killScore;
    public int VictoryBonusScore => victoryBonusScore;
    public int HealthBonusScore => healthBonusScore;
    public int CollectiblesCount => collectiblesCount;
    public int EnemiesKilled => enemiesKilled;
    public float SurvivalTime => survivalTime;

    // ────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start() => UpdateUI();

    private void Update()
    {
        if (GameFlowManager.Instance == null || !GameFlowManager.Instance.IsGameplay) return;

        survivalTime += Time.deltaTime;
        survivalTimer += Time.deltaTime;

        if (survivalTimer >= survivalTickInterval)
        {
            survivalTimer -= survivalTickInterval;
            AddSurvivalScore(survivalScorePerTick);
        }
    }

    // ── Métodos por categoría ────────────────────────────────

    public void AddSurvivalScore(int amount)
    {
        if (amount <= 0) return;
        survivalScore += amount;
        AddScore(amount);
    }

    public void AddCollectibleScore(int amount)
    {
        if (amount <= 0) return;
        collectiblesCount++;
        collectibleScore += amount;
        AddScore(amount);
    }

    public void RegisterKill()
    {
        enemiesKilled++;
        killScore += pointsPerKill;
        AddScore(pointsPerKill);
    }

    public void AddVictoryBonus(int amount)
    {
        victoryBonusScore += amount;
        AddScore(amount);
    }

    public void AddHealthBonus(int currentHP, int pointsPerHP)
    {
        int bonus = currentHP * pointsPerHP;
        healthBonusScore += bonus;
        AddScore(bonus);
    }

    // ── Interno ──────────────────────────────────────────────

    private void AddScore(int amount)
    {
        if (amount <= 0) return;
        score += amount;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }
}