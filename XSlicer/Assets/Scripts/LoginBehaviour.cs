using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.SceneManagement; 
using TMPro;

[System.Serializable]
public class UserData
{
    public int id;
    public string username;
    public string password;
    public int credit;
}

[System.Serializable]
public class StatData
{
    public int id;
    public string player_id;
    public int score;
    public int level;
    public float time_played;
    public string created_at; 
}

public static class JsonParser
{
    public static T[] FromJson<T>(string json)
    {
        string newJson = "{ \"items\": " + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
        return wrapper.items;
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public T[] items;
    }
}

public class LoginBehaviour : MonoBehaviour, ISliceEffect
{
    private string baseUrl = "http://127.0.0.1:8000";

    [SerializeField] private string username;
    [SerializeField] private string password;
    [SerializeField] private TextMeshPro scoreText;
    [SerializeField] private string nextSceneName = "SongChoicesSinglePlayer"; 

    private bool isLoading;

    public void OnSliced()
    {
        isLoading = true;
        StartCoroutine(Loading());
        StartCoroutine(Login());
    }

     IEnumerator Loading()
    {
        while (isLoading)
        {
            scoreText.text = "Loading.";
            yield return new WaitForSeconds(0.5f);
            scoreText.text = "Loading..";
            yield return new WaitForSeconds(0.5f);
            scoreText.text = "Loading...";
            yield return new WaitForSeconds(0.5f);
        }
    }

    public IEnumerator Login()
    {
        string url = $"{baseUrl}/users?username={UnityWebRequest.EscapeURL(username)}&password={UnityWebRequest.EscapeURL(password)}";
        
        Debug.Log("Sende Anfrage an: " + url); // Damit du siehst, dass es startet

        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Netzwerkfehler: " + webRequest.error);
                isLoading = false;
            }
            else
            {
                string jsonResponse = webRequest.downloadHandler.text;
                Debug.Log("Roh-JSON vom Server: " + jsonResponse); // Das MUSS erscheinen

                if (string.IsNullOrEmpty(jsonResponse) || jsonResponse == "[]" || jsonResponse == "null")
                {
                    Debug.LogWarning("Login fehlgeschlagen: Keine Daten gefunden.");
                    isLoading = false;
                }
                else
                {
                    UserData[] users = JsonParser.FromJson<UserData>(jsonResponse);

                    if (users != null && users.Length > 0)
                    {
                        UserData loggedInUser = users[0];
                        Debug.Log($"ERFOLG! ID: {loggedInUser.id}, Name: {loggedInUser.username}");
                        UserIDCarrier.player_id = loggedInUser.id.ToString();
                        UserIDCarrier.username = loggedInUser.username;
                        UserIDCarrier.credits = loggedInUser.credit;
                        isLoading = false;
                        SceneManager.LoadScene("SongChoicesSinglePlayer");
                    }
                    else
                    {
                        Debug.LogError("Kein solcher User gefunden");
                        isLoading = false;
                    }
                }
            }
        }
    }

    
}
