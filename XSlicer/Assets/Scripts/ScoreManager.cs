using System;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [SerializeField] private TMP_Text scoreLabel;

    public int Score { get; private set; }
    public event Action<int> ScoreChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        UpdateLabel();
    }

    public void AddPoints(int amount)
    {
        if (amount <= 0) return;
        Score += amount;
        ScoreChanged?.Invoke(Score);
        UpdateLabel();
    }

    public void ResetScore()
    {
        Score = 0;
        ScoreChanged?.Invoke(Score);
        UpdateLabel();
    }

    public void SetScoreLabel(TMP_Text label)
    {
        scoreLabel = label;
        UpdateLabel();
    }

    private void UpdateLabel()
    {
        if (scoreLabel != null)
        {
            scoreLabel.text = $"Score: {Score}";
        }
    }
}

