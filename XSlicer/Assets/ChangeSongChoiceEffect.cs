using UnityEngine;

enum SongOptions { Previous, Next}

public class ChangeSongChoiceEffect : MonoBehaviour, ISliceEffect
{
    [SerializeField] private SongOptions songChoice = SongOptions.Next;
    private bool _hasChanged;

    public void OnSliced()
    {
        if (_hasChanged) return;
        _hasChanged = true;
        if (songChoice == SongOptions.Next)
        {
            LevelManagerBehaviour.Instance?.GetNext();
        }
        else if (songChoice == SongOptions.Previous)
        {
            LevelManagerBehaviour.Instance?.GetPrevious();
        }
    }
}
