using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class BypassCertificate : CertificateHandler
{
    protected override bool ValidateCertificate(byte[] certificateData) => true;
}

[System.Serializable]
public class SongDTO
{
    public int id;
    public string url;
}

// 2. Logik-Klasse für das Spiel
[System.Serializable]
public class Song
{
    public string name;
    public string artist;
    public string url;
    public string thumbnail_url;
    public float length;
    public Texture2D CachedTexture;

    public string GetStamina()
    {
        if (length < 120) return "Short";
        if (length >= 120 && length < 240) return "Medium";
        return "Long";
    }
}

public class LevelManagerBehaviour : MonoBehaviour
{
    public static LevelManagerBehaviour Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private string apiBaseUrl = "http://127.0.0.1:8000";
    [SerializeField] private List<Transform> positions = new List<Transform>();
    [SerializeField] private TextMeshPro themePrefab;
    [SerializeField] private MeshRenderer imagePrefab;

    private List<string> songUrlsFromDB = new List<string>();
    private List<Song> loadedSongs = new List<Song>();
    private List<Song> shownSongs = new List<Song>();
    
    private List<GameObject> spawnedUI = new List<GameObject>();
    private InputSystem_Actions inputActions;

    private void Awake()
    {
        Debug.Log("[LevelManager] Awake gestartet"); // MUSS erscheinen
        Instance = this;
        inputActions = new InputSystem_Actions();
        
        // Wir nutzen eine Coroutine als Starter, um asynchrone Hänger in Awake zu vermeiden
        StartCoroutine(MainFlowRoutine());
    }
    private IEnumerator MainFlowRoutine()
{
    Debug.Log("[LevelManager] Flow gestartet");
    
    // 1. Datenbank abfragen (Wir wandeln Task in Coroutine um)
    Task dbTask = FetchSongsFromDatabase();
    yield return new WaitUntil(() => dbTask.IsCompleted);

    if (songUrlsFromDB.Count > 0)
    {
        Debug.Log($"[LevelManager] {songUrlsFromDB.Count} URLs gefunden. Lade Metadaten...");
        
        // 2. Metadaten laden
        Task metaTask = LoadAllSongMetadata();
        yield return new WaitUntil(() => metaTask.IsCompleted);
        
        // 3. Anzeige vorbereiten
        shownSongs.Clear();
        for (int i = 0; i < Mathf.Min(3, loadedSongs.Count); i++)
        {
            shownSongs.Add(loadedSongs[i]);
        }
        
        Debug.Log($"[LevelManager] Erstelle Anzeigen für {shownSongs.Count} Songs...");
        Show();
        
        Debug.Log("[LevelManager] Setup abgeschlossen.");
    }
    else
    {
        Debug.LogWarning("[LevelManager] Keine Songs zum Laden gefunden.");
    }
}


    private async void RunInitialization()
{
    try 
    {
        Debug.Log("[LevelManager] Starte DB-Abfrage...");
        await FetchSongsFromDatabase();
        
        if (songUrlsFromDB == null || songUrlsFromDB.Count == 0)
        {
            Debug.LogWarning("[LevelManager] Keine URLs gefunden. Tabellen-Name in DB prüfen?");
            return;
        }

        Debug.Log($"[LevelManager] {songUrlsFromDB.Count} URLs geladen. Lade Metadaten...");
        await LoadAllSongMetadata();
        
        // Anzeige vorbereiten
        shownSongs.Clear();
        for (int i = 0; i < Mathf.Min(3, loadedSongs.Count); i++)
        {
            shownSongs.Add(loadedSongs[i]);
        }
        
        Show();
        Debug.Log("[LevelManager] Setup erfolgreich beendet.");
    }
    catch (Exception e)
    {
        Debug.LogError($"[LevelManager] Kritischer Fehler im Flow: {e.Message}\n{e.StackTrace}");
    }
}


    private async Task FetchSongsFromDatabase()
{
    string fullUrl = $"{apiBaseUrl}/songs";
    songUrlsFromDB.Clear();

    Debug.Log($"[LevelManager] Starte Request an: {fullUrl}");

    using (UnityWebRequest www = UnityWebRequest.Get(fullUrl))
    {
        // Das hier ist der sicherste Weg, asynchron in Unity auf einen WebRequest zu warten
        var operation = www.SendWebRequest();

        while (!operation.isDone)
        {
            await Task.Delay(100); // Kurze Pause, um den Thread nicht zu blockieren
        }

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[LevelManager] DB-Fehler: {www.error}");
            return;
        }

        string responseText = www.downloadHandler.text;
        Debug.Log($"[LevelManager] Antwort erhalten: {responseText}");

        // WICHTIG: Prüfen ob JsonParser existiert
        SongDTO[] dtos = JsonParser.FromJson<SongDTO>(responseText);
        
        if (dtos != null && dtos.Length > 0)
        {
            foreach (var dto in dtos)
            {
                if (!string.IsNullOrEmpty(dto.url))
                {
                    songUrlsFromDB.Add(dto.url);
                    Debug.Log($"[LevelManager] URL geladen: {dto.url}");
                }
            }
        }
        else
        {
            Debug.LogWarning("[LevelManager] Keine Songs im JSON gefunden oder Parser fehlgeschlagen.");
        }
    }
}


    private async Task LoadAllSongMetadata()
    {
        loadedSongs.Clear();
        foreach (var url in songUrlsFromDB)
        {
            var song = await LoadSingleSongAsync(url);
            if (song != null) loadedSongs.Add(song);
        }
    }

    private Task<Song> LoadSingleSongAsync(string url)
    {
        var tcs = new TaskCompletionSource<Song>();

        // Ruft Metadaten vom FastAPIClient ab (z.B. via yt-dlp auf dem Server)
        FastAPIClient.Instance.GetSongMetadataWithoutDownload(url, songData =>
        {
            if (songData == null)
            {
                Debug.LogError($"Metadaten-Fehler für: {url}");
                tcs.SetResult(null);
                return;
            }

            Song newSong = new Song {
                name = songData.title,
                artist = songData.artist,
                thumbnail_url = songData.thumbnail_url,
                length = (float)songData.duration,
                url = url
            };

            tcs.SetResult(newSong);
        });

        return tcs.Task;
    }

    void Show() // Nicht mehr async Task
{
    // Altes UI aufräumen
    foreach (var obj in spawnedUI) if (obj != null) Destroy(obj);
    spawnedUI.Clear();

    if (shownSongs.Count == 0) {
        Debug.LogWarning("[LevelManager] shownSongs ist leer!");
        return;
    }

    for (int i = 0; i < shownSongs.Count; i++)
    {
        if (i >= positions.Count) break;

        Song song = shownSongs[i];
        Vector3 pos = positions[i].position;

        // Thumbnail laden
        StartCoroutine(LoadThumbnailRoutine(song, i));

        // Texte instanziieren
        CreateText(song.name, pos, 0f);
        CreateText(song.artist, pos, 0.2f);
        CreateText($"Dauer: {song.GetStamina()}", pos, 0.4f);
        
        Debug.Log($"[LevelManager] Anzeige für {song.name} an Position {i} erstellt.");
    }
}


    private void CreateText(string content, Vector3 basePos, float yOffset)
    {
        var t = Instantiate(themePrefab, basePos - new Vector3(0, yOffset, 0), Quaternion.Euler(0, 270, 0));
        t.text = content;
        spawnedUI.Add(t.gameObject);
    }

    private IEnumerator LoadThumbnailRoutine(Song song, int index)
    {
        if (song.CachedTexture != null)
        {
            CreateImageObject(song, index);
            yield break;
        }

        if (string.IsNullOrEmpty(song.thumbnail_url)) yield break;

        FastAPIClient.Instance.GetProxyThumbnail(song.thumbnail_url, (tex) =>
        {
            if (tex != null)
            {
                song.CachedTexture = tex;
                CreateImageObject(song, index);
            }
        });
    }

    private void CreateImageObject(Song song, int i)
    {
        MeshRenderer mr = Instantiate(imagePrefab, positions[i].position + Vector3.up, Quaternion.Euler(0, 270, 0));
        mr.material.mainTexture = song.CachedTexture;
        
        // URL an Scripte auf dem Prefab weitergeben
        if (mr.TryGetComponent(out SceneLoaderBehaviour slb)) slb.url = song.url;
        if (mr.TryGetComponent(out LoadSongSceneEffect lse)) lse.url = song.url;

        spawnedUI.Add(mr.gameObject);
    }

    // --- Navigation ---
    public void GetNext()
    {
        if (loadedSongs.Count < 2) return;
        // Einfache Rotation der Liste
        Song first = loadedSongs[0];
        loadedSongs.RemoveAt(0);
        loadedSongs.Add(first);
        UpdateShownSongs();
    }

    public void GetPrevious()
    {
        if (loadedSongs.Count < 2) return;
        Song last = loadedSongs[loadedSongs.Count - 1];
        loadedSongs.RemoveAt(loadedSongs.Count - 1);
        loadedSongs.Insert(0, last);
        UpdateShownSongs();
    }

    private void UpdateShownSongs()
    {
        shownSongs.Clear();
        for (int i = 0; i < Mathf.Min(3, loadedSongs.Count); i++)
            shownSongs.Add(loadedSongs[i]);
    }

    private void OnEnable() => inputActions?.Player.Enable();
    private void OnDisable() => inputActions?.Player.Disable();
}
