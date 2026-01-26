using Unity.Android.Gradle.Manifest;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems; 

public class SceneLoaderBehaviour : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private string sceneToLoad;
    [SerializeField] public string url;


    public void OnPointerClick(PointerEventData eventData)
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.Log("Clicked");
            DataCarrier.Instance.url = url;
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
