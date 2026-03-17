using System.Text;
using TMPro;
using UnityEngine;

public class InventoryBehaviour : MonoBehaviour
{
    [SerializeField] private TMP_Text listLabel;

    private void Start()
    {
        if (string.IsNullOrEmpty(UserIDCarrier.player_id)) return;
        FastAPIClient.Instance.GetUserSwords(UserIDCarrier.player_id, (swords) =>
        {
            if (listLabel == null) return;
            if (swords == null || swords.Length == 0)
            {
                listLabel.text = "No swords owned.";
                return;
            }
            StringBuilder sb = new StringBuilder();
            foreach (var sword in swords)
                sb.AppendLine($"- {sword.name}");
            listLabel.text = sb.ToString();
        });
    }
}
