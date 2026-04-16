using UnityEngine;
using TMPro;

public class CollectibleGoalManager : MonoBehaviour
{
    [Header("Goal")]
    [SerializeField] private int collectiblesToWin = 10;
    [SerializeField] private int collected = 0;

    [Header("UI")]
    [SerializeField] private TMP_Text objectiveText;

    [Header("Systems")]
    [SerializeField] private ScoreManager scoreManager;

    private void Start() => UpdateObjectiveUI();

    public void RegisterCollectible(int value)
    {
        if (GameFlowManager.Instance != null && !GameFlowManager.Instance.IsGameplay)
            return;

        collected++;

        if (scoreManager != null)
            scoreManager.AddScore(value);

        UpdateObjectiveUI();

        if (collected >= collectiblesToWin)
        {
            if (GameFlowManager.Instance != null)
                GameFlowManager.Instance.RequestVictory();
        }
    }

    private void UpdateObjectiveUI()
    {
        if (objectiveText != null)
            objectiveText.text = $": {collected}/{collectiblesToWin}";
    }
}
