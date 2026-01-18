using UnityEngine;

public class ScoreSliceEffect : MonoBehaviour, ISliceEffect
{
    [SerializeField] private int pointValue = 100;
    private bool _hasAwarded;
    
    public int PointValue => Mathf.Max(0, pointValue);

    public void OnSliced()
    {
        if (_hasAwarded || PointValue <= 0) return;
        _hasAwarded = true;
        ScoreManager.Instance?.AddPoints(PointValue);
    }
}