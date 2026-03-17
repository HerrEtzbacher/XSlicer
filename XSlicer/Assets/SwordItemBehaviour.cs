using TMPro;
using UnityEngine;

public class SwordItemBehaviour : MonoBehaviour
{
    [SerializeField] private TMP_Text nameLabel;
    [SerializeField] private TMP_Text priceLabel;
    [SerializeField] private MeshRenderer swordMesh;

    private SwordData _swordData;
    private bool _purchaseAttempted;
    private float _enterTime;

    public void Initialize(SwordData data)
    {
        _swordData = data;
        if (nameLabel != null) nameLabel.text = data.name;
        if (priceLabel != null) priceLabel.text = $"{data.price} Credits";
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Sword")) return;
        _enterTime = Time.time;
        if (swordMesh != null)
        {
            swordMesh.material.color = Color.yellow;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Sword")) return;
        if (swordMesh != null)
        {
            swordMesh.material.color = Color.white;
        }

        if (_purchaseAttempted) return;
        if (Time.time - _enterTime < 0.5f) return;

        _purchaseAttempted = true;

        int userId;
        if (!int.TryParse(UserIDCarrier.player_id, out userId)) return;

        FastAPIClient.Instance.BuySword(userId, _swordData.id, (success) =>
        {
            if (success)
            {
                if (priceLabel != null) priceLabel.text = "OWNED";
                if (ShopBehaviour.Instance != null) ShopBehaviour.Instance.OnPurchaseComplete();
            }
            else
            {
                _purchaseAttempted = false;
            }
        });
    }
}
