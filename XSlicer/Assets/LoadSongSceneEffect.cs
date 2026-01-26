using Unity.Android.Gradle.Manifest;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSongSceneEffect : MonoBehaviour, ISliceEffect
{
    [SerializeField] private string sceneName;
    private bool _hasLoaded;
    public string url;

    public void OnSliced()
    {
        Debug.Log("LoadSongSceneEffect: OnSliced called");
        if (_hasLoaded) return;
        _hasLoaded = true;
        DataCarrier.Instance.url = url;
        SceneManager.LoadScene(sceneName);
    }
}
