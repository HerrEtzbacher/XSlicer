using System.Collections;
using System.IO;
using UnityEngine;

public class SongBehaviour : MonoBehaviour
{
    private FastAPIClient aPIClient;

    [SerializeField]
    private string songName;

    [SerializeField]
    private int bpm;
    [SerializeField]
    private GameObject cube;
    private float timeCount;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        FastAPIClient.Instance.ProcessSong("https://www.youtube.com/watch?v=14954yE101g&list=RDlPGipwoJiOM&index=4", (songData) =>
        {
            if (songData == null)
            {
                Debug.LogError("Failed to process song.");
                return;
            }
            
            StartCoroutine(DownloadAndCacheSong(songData));

            Debug.Log($"New song ready: {songData.title}, ID: {songData.id}");
            Debug.Log($"Cached at: {songData.localPath}");
        });
        timeCount = 0;
    }
    
    private IEnumerator DownloadAndCacheSong(SongData song)
    {
        string songsDir = Path.Combine(Application.persistentDataPath, "Songs");
        string songFolder = Path.Combine(songsDir, song.id);
        Directory.CreateDirectory(songFolder);

        bool metadataDone = false;
        bool fileDone = false;

        string metadataPath = Path.Combine(songFolder, $"{song.id}.json");
        File.WriteAllText(metadataPath, JsonUtility.ToJson(song, true));
        metadataDone = true;

        FastAPIClient.Instance.DownloadSongFile(song.id, (filePath) =>
        {
            string destPath = Path.Combine(songFolder, $"{song.id}.mp3");
            if (filePath != destPath)
                File.Copy(filePath, destPath, true);
            song.localPath = destPath;
            fileDone = true;
        });

        yield return new WaitUntil(() => metadataDone && fileDone);
        Debug.Log($"Song {song.id} cached locally.");
    }

    // Update is called once per frame
    void Update()
    {
        timeCount += Time.deltaTime;
        if((60/bpm) - timeCount <= 0.001f){
            timeCount = 0;
            Instantiate(cube, transform.position, Quaternion.identity);
        }
    }
}
