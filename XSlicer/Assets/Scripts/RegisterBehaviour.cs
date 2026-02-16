using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Text; // WICHTIG für Encoding

public class RegisterBehaviour : MonoBehaviour, ISliceEffect
{
    private string baseUrl = "http://127.0.0.1:8000";

    [SerializeField] private string username;
    [SerializeField] private string password;

    public void OnSliced()
    {
        
    }
    void Start()
    {
        StartCoroutine(Register());
    }

    public IEnumerator Register()
    {
        string url = $"{baseUrl}/user"; 

        UserRegistrationData regData = new UserRegistrationData();
        regData.username = username;
        regData.password = password;
        string jsonBody = JsonUtility.ToJson(regData);

        using (UnityWebRequest webRequest = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            
            // WICHTIG: FastAPI braucht den Content-Type Header
            webRequest.SetRequestHeader("Content-Type", "application/json");

            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Registrierung fehlgeschlagen: {webRequest.error} | {webRequest.downloadHandler.text}");
            }
            else
            {
                string jsonResponse = webRequest.downloadHandler.text;
                Debug.Log("User erfolgreich erstellt: " + jsonResponse);

                // 3. Antwort parsen (Dein Endpoint gibt {"status": "success", "data": {...}} zurück)
                RegisterResponse response = JsonUtility.FromJson<RegisterResponse>(jsonResponse);

                if (response != null && response.status == "success")
                {
                    
                    UserIDCarrier.player_id = response.data.id.ToString();
                    UserIDCarrier.username = response.data.username;
                    
                    Debug.Log($"Registriert mit ID: {response.data.id}");
                }
            }
        }
    }
}

// Hilfsklassen für das JSON-Handling
[System.Serializable]
public class UserRegistrationData
{
    public string username;
    public string password;
}

[System.Serializable]
public class RegisterResponse
{
    public string status;
    public UserData data; 
}
