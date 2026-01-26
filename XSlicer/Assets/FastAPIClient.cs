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
        using (UnityWebRequest request = UnityWebRequest.PostWwwForm(url, ""))
        {
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Process Error: {request.error}");
                onComplete?.Invoke(null);
            }
            else
            {
                ProcessSongResponse resp = JsonUtility.FromJson<ProcessSongResponse>(request.downloadHandler.text);
                onComplete?.Invoke(resp.metadata);
            }
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

    public void DownloadSongFile(string videoId, System.Action<string> onComplete)
    {
        // CLIENT-SIDE CACHE CHECK
        string songDir = Path.Combine(Application.persistentDataPath, "Songs");
        if (!Directory.Exists(songDir)) Directory.CreateDirectory(songDir);
        
        string localPath = Path.Combine(songDir, $"{videoId}.mp3");

        if (File.Exists(localPath))
        {
            Debug.Log("[Cache] File already exists locally.");
            onComplete?.Invoke(localPath);
            return;
        }

        StartCoroutine(DownloadCoroutine(videoId, localPath, onComplete));
    }

    private IEnumerator DownloadCoroutine(string videoId, string savePath, System.Action<string> onComplete)
    {
        string url = $"{baseUrl}/songs/{videoId}/file";
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                File.WriteAllBytes(savePath, request.downloadHandler.data);
                onComplete?.Invoke(savePath);
            }
            else { Debug.LogError("Download failed"); }
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
