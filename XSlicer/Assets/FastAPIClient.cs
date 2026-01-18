using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

public class FastAPIClient : MonoBehaviour
{
    public static FastAPIClient Instance { get; private set; }

    [SerializeField] private string baseUrl = "http://127.0.0.1:8000";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ProcessSong(string youtubeLink, System.Action<SongData> onComplete)
    {
        StartCoroutine(ProcessSongCoroutine(youtubeLink, onComplete));
    }

    private IEnumerator ProcessSongCoroutine(string link, System.Action<SongData> onComplete)
    {
        string url = $"{baseUrl}/process_link?link={UnityWebRequest.EscapeURL(link)}";
        UnityWebRequest request = UnityWebRequest.PostWwwForm(url, "");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError ||
            request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError($"[FastAPIClient] ProcessSong error: {request.error}");
            onComplete?.Invoke(null);
        }
        else
        {
            string json = request.downloadHandler.text;
            ProcessSongResponse response = JsonUtility.FromJson<ProcessSongResponse>(json);

            Debug.Log($"[FastAPIClient] {response.message}");
            Debug.Log($"[FastAPIClient] New song ID: {response.metadata.id}");

            onComplete?.Invoke(response.metadata);
        }
    }

    public void GetSongMetadataWithoutDownload(string link, System.Action<SongData> onComplete)
    {
        StartCoroutine(GetSongMetadataWithoutDownloadCoroutine(link, onComplete));
    }
    private IEnumerator GetSongMetadataWithoutDownloadCoroutine(string link, System.Action<SongData> onComplete)
    {
        string url = $"{baseUrl}/get_metadata?link={UnityWebRequest.EscapeURL(link)}";
        UnityWebRequest request = UnityWebRequest.Get(url);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError ||
            request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError($"[FastAPIClient] GetSongMetadata error: {request.error}");
        }
        else
        {
            string json = request.downloadHandler.text;
            ProcessSongResponse response = JsonUtility.FromJson<ProcessSongResponse>(json);
            Debug.Log($"[FastAPIClient] Metadata received for {link}");
            onComplete?.Invoke(response.metadata);
        }
    }

    public void GetSongMetadata(string songId, System.Action<string> onComplete = null)
    {
        StartCoroutine(GetSongMetadataCoroutine(songId, onComplete));
    }

    private IEnumerator GetSongMetadataCoroutine(string songId, System.Action<string> onComplete)
    {
        string url = $"{baseUrl}/songs/{songId}";
        UnityWebRequest request = UnityWebRequest.Get(url);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError ||
            request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError($"[FastAPIClient] GetSongMetadata error: {request.error}");
        }
        else
        {
            string json = request.downloadHandler.text;
            Debug.Log($"[FastAPIClient] Metadata received for {songId}");
            onComplete?.Invoke(json);
        }
    }

    public void DownloadSongFile(string songId, System.Action<string> onComplete = null)
    {
        StartCoroutine(DownloadSongFileCoroutine(songId, onComplete));
    }

    private IEnumerator DownloadSongFileCoroutine(string songId, System.Action<string> onComplete)
    {
        string url = $"{baseUrl}/songs/{songId}/file";
        UnityWebRequest request = UnityWebRequest.Get(url);

        string songDir = Path.Combine(Application.persistentDataPath, "Songs");
        Directory.CreateDirectory(songDir);

        string filePath = Path.Combine(songDir, $"{songId}.mp3");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError ||
            request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError($"[FastAPIClient] DownloadSongFile error: {request.error}");
        }
        else
        {
            File.WriteAllBytes(filePath, request.downloadHandler.data);
            Debug.Log($"[FastAPIClient] Downloaded song to {filePath}");
            onComplete?.Invoke(filePath);
        }
    }

    public void GetAllSongs(System.Action<string> onComplete = null)
    {
        StartCoroutine(GetAllSongsCoroutine(onComplete));
    }

    private IEnumerator GetAllSongsCoroutine(System.Action<string> onComplete)
    {
        string url = $"{baseUrl}/songs";
        UnityWebRequest request = UnityWebRequest.Get(url);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError ||
            request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError($"[FastAPIClient] GetAllSongs error: {request.error}");
        }
        else
        {
            string json = request.downloadHandler.text;
            Debug.Log("[FastAPIClient] Song list received");
            onComplete?.Invoke(json);
        }
    }
    public void GetProxyThumbnail(string originalThumbnailUrl, System.Action<Texture2D> onComplete)
    {
        StartCoroutine(GetProxyThumbnailCoroutine(originalThumbnailUrl, onComplete));
    }

    private IEnumerator GetProxyThumbnailCoroutine(string originalUrl, System.Action<Texture2D> onComplete)
    {
        string fallbackUrl = originalUrl.Replace("/vi_webp/", "/vi/").Replace(".webp", ".jpg");
        
        string url = $"{baseUrl}/proxy_image?url={UnityWebRequest.EscapeURL(fallbackUrl)}";

        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            request.certificateHandler = new BypassCertificate(); 

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[FastAPIClient] Proxy Image Error: {request.error} | URL: {url}");
                onComplete?.Invoke(null);
            }
            else
            {
                Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
                onComplete?.Invoke(texture);
            }
        }
    }

}

[System.Serializable]
public class ProcessSongResponse
{
    public string message;
    public string metadata_path;
    public SongData metadata;
}
