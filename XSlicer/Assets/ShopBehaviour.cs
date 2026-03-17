using TMPro;
using UnityEngine;

public class ShopBehaviour : MonoBehaviour
{
    public static ShopBehaviour Instance { get; private set; }

    [SerializeField] private Transform[] swordSpawnPoints;
    [SerializeField] private GameObject swordItemPrefab;
    [SerializeField] private TMP_Text statusLabel;
    [SerializeField] private CurrencyDisplayBehaviour currencyDisplay;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        FastAPIClient.Instance.GetShopSwords((swords) =>
        {
            if (swords == null) return;
            for (int i = 0; i < swords.Length && i < swordSpawnPoints.Length; i++)
            {
                GameObject item = Instantiate(swordItemPrefab, swordSpawnPoints[i].position, swordSpawnPoints[i].rotation);
                item.GetComponent<SwordItemBehaviour>().Initialize(swords[i]);
            }
        });
    }

    public void OnPurchaseComplete()
    {
        FastAPIClient.Instance.GetUser(UserIDCarrier.player_id, (user) =>
        {
            if (user == null) return;
            UserIDCarrier.credits = user.credit;
            if (currencyDisplay != null) currencyDisplay.ForceRefresh();
            if (statusLabel != null) statusLabel.text = "Purchase successful!";
        });
    }
}
