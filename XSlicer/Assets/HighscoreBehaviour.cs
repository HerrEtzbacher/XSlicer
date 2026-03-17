using System.Text;
using TMPro;
using UnityEngine;

public class HighscoreBehaviour : MonoBehaviour
{
    [SerializeField] private TMP_Text listLabel;

    private void Start()
    {
        FastAPIClient.Instance.GetHighscores((scores) =>
        {
            if (listLabel == null) return;
            if (scores == null || scores.Length == 0)
            {
                listLabel.text = "No scores yet.";
                return;
            }
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < scores.Length; i++)
                sb.AppendLine($"{i + 1}. {scores[i].player_id} — {scores[i].total_score}");
            listLabel.text = sb.ToString();
        });
    }
}
