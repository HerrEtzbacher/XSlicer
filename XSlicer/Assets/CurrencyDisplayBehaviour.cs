using System.Collections;
using TMPro;
using UnityEngine;

public class CurrencyDisplayBehaviour : MonoBehaviour
{
    [SerializeField] private TMP_Text creditLabel;
    [SerializeField] private float pollInterval = 10f;

    private Coroutine _pollCoroutine;

    private void Start()
    {
        _pollCoroutine = StartCoroutine(PollCredits());
    }

    private void OnDisable()
    {
        if (_pollCoroutine != null) StopCoroutine(_pollCoroutine);
    }

    private IEnumerator PollCredits()
    {
        while (true)
        {
            RefreshFromServer();
            yield return new WaitForSeconds(pollInterval);
        }
    }

    public void ForceRefresh()
    {
        RefreshFromServer();
    }

    private void RefreshFromServer()
    {
        if (string.IsNullOrEmpty(UserIDCarrier.player_id)) return;
        FastAPIClient.Instance.GetUser(UserIDCarrier.player_id, (user) =>
        {
            if (user == null) return;
            UserIDCarrier.credits = user.credit;
            if (creditLabel != null)
                creditLabel.text = $"Credits: {user.credit}";
        });
    }
}
