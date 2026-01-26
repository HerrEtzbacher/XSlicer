using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class SongBehaviour : MonoBehaviour
{
    [SerializeField] private string songName;
    [SerializeField] private float bpm;
    [SerializeField] private List<GameObject> speedyThings;
    [SerializeField] private List<Transform> positions;
    [SerializeField] private GameObject cube;
    [SerializeField] private string url;
    [SerializeField] private TextMeshPro scoreText;
    [SerializeField] private string backendUrl;

    private System.Random random;
    private float timeCount;
    private float videoLength;
    private bool isProcessing = true;
    private AudioClip clip;
    private AudioSource source;
    private int waveCount = 0;
    private int score = 0;

    void Awake()
    {
        url = DataCarrier.Instance.url;
    }

    void Start()
    {
        StartCoroutine(Loading());
        random = new System.Random();

        FastAPIClient.Instance.ProcessSong(url, (songData) =>
        {
            if (songData == null) return;

            bpm = songData.rhythm_analysis.tempo_bpm;
            videoLength = songData.duration;

            FastAPIClient.Instance.DownloadSongFile(songData.id, (filePath) =>
            {
                isProcessing = false;
                source = gameObject.AddComponent<AudioSource>();
                StartCoroutine(GetAudioClip(filePath));
                StartCoroutine(InstantiateWaves());
            });
        });
    }
    
    

    IEnumerator InstantiateWaves()
    {
        while (timeCount < videoLength)
        {
            waveCount++;
            Debug.Log($"Starting wave {waveCount}");

            for (int i = 0; i < positions.Count; i++)
            {
                Instantiate(speedyThings[random.Next(speedyThings.Count)], positions[i].position, Quaternion.identity);
            }

            yield return new WaitForSeconds(60 / bpm * positions.Count);
            yield return StartCoroutine(SendScoreToBackend());

            timeCount += 60 / bpm * positions.Count;
        }
    }

    IEnumerator SendScoreToBackend()
    {
        GameStatData data = new GameStatData();
        data.player_id = "PlayerOne"; 
        data.score = score;
        data.level = waveCount; 
        data.time_played = Time.timeSinceLevelLoad; 

        string jsonPayload = JsonUtility.ToJson(data);
    
        using (UnityWebRequest www = new UnityWebRequest(backendUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
        
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error sending score: {www.error}");
                Debug.LogError($"Response: {www.downloadHandler.text}");
            }
            else
            {
                Debug.Log("Score sent successfully!");
                Debug.Log($"Response: {www.downloadHandler.text}");
            }
        }
    }

    IEnumerator GetAudioClip(string filePath)
    {
        string uri = "file://" + filePath;
    
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(uri, AudioType.MPEG))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Audio Load Error: {www.error}");
            }
            else
            {
                clip = DownloadHandlerAudioClip.GetContent(www);
                source.clip = clip;
                source.Play();
                Debug.Log("Audio playing!");
            }
        }
    }

    IEnumerator Loading()
    {
        while (isProcessing)
        {
            scoreText.text = "Processing song.";
            yield return new WaitForSeconds(0.5f);
            scoreText.text = "Processing song..";
            yield return new WaitForSeconds(0.5f);
            scoreText.text = "Processing song...";
            yield return new WaitForSeconds(0.5f);
        }
    }

    void Update()
    {
        if (!isProcessing && timeCount < videoLength)
        {
            timeCount += Time.deltaTime / 100;
            scoreText.text = $"Time: {timeCount:F2} / {videoLength:F2}";
        }
        else if (!isProcessing)
        {
            scoreText.text = $"LEVEL COMPLETE! ({videoLength:F2}s)";
        }
    }
}

[System.Serializable]
public class GameStatData
{
    public string player_id;
    public int score;
    public int level;
    public float time_played;
}