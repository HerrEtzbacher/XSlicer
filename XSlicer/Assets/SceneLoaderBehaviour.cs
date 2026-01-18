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
            DataCarrier.url = url;
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}

public static class DataCarrier 
{
    public static string url;
}
