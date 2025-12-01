using UnityEngine;

/// <summary>
/// Holds the point value for a cube and awards it exactly once when sliced.
/// </summary>
public class CubeScore : MonoBehaviour
{
    [SerializeField] private int pointValue = 100;
    private bool _hasAwarded;

    public int PointValue => Mathf.Max(0, pointValue);

    /// <summary>
    /// Called by the slicer once the cube has been successfully sliced.
    /// </summary>
    public void AwardPoints()
    {
        if (_hasAwarded || PointValue <= 0) return;
        _hasAwarded = true;
        ScoreManager.Instance?.AddPoints(PointValue);
    }
}

