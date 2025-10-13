using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SongStartupManager : MonoBehaviour
{
    [SerializeField] private bool fetchFromAPIIfEmpty = true;

    private string songsDir;

    private void Start()
    {
        songsDir = Path.Combine(Application.persistentDataPath, "Songs");
        Directory.CreateDirectory(songsDir);
        StartCoroutine(InitializeSongs());
    }

    private IEnumerator InitializeSongs()
    {
        yield return new WaitUntil(() => FastAPIClient.Instance != null);

        var localSongs = LoadLocalSongs();
        if (localSongs.Count > 0)
        {
            Debug.Log($"Loaded {localSongs.Count} local songs from disk.");
            yield break;
        }

        if (fetchFromAPIIfEmpty)
        {
            Debug.Log("No local songs found, fetching from API...");
            FastAPIClient.Instance.GetAllSongs(OnSongsReceived);
        }
    }

    private List<SongData> LoadLocalSongs()
    {
        var songList = new List<SongData>();
        foreach (string dir in Directory.GetDirectories(songsDir))
        {
            string jsonPath = Path.Combine(dir, Path.GetFileName(dir) + ".json");
            string audioPath = Path.Combine(dir, Path.GetFileName(dir) + ".mp3");

            if (File.Exists(jsonPath) && File.Exists(audioPath))
            {
                string json = File.ReadAllText(jsonPath);
                SongData data = JsonUtility.FromJson<SongData>(json);
                data.localPath = audioPath;
                songList.Add(data);
            }
        }
        return songList;
    }

    private void OnSongsReceived(string json)
    {
        Debug.Log("Song list received: " + json);

        SongData[] songs = JsonHelper.FromJsonArray<SongData>(json);
        if (songs == null || songs.Length == 0)
        {
            Debug.LogWarning("No songs available from API.");
            return;
        }

        foreach (var song in songs)
        {
            StartCoroutine(DownloadAndCacheSong(song));
        }
    }

    private IEnumerator DownloadAndCacheSong(SongData song)
    {
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
}