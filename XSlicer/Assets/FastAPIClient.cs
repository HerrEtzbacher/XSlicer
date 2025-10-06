using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class FastAPIClient : MonoBehaviour
{
    private string baseUrl = "http://127.0.0.1:8000";

    public void ProcessSong(string youtubeLink)
    {
        StartCoroutine(PostLinkCoroutine(youtubeLink));
    }

    private IEnumerator PostLinkCoroutine(string link)
    {
        string url = $"{baseUrl}/process_link?link={UnityWebRequest.EscapeURL(link)}";

        UnityWebRequest request = UnityWebRequest.PostWwwForm(url, "");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || 
            request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error: " + request.error);
        }
        else
        {
            Debug.Log("Response: " + request.downloadHandler.text);
        }
    }
}
