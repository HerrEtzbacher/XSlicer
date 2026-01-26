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

public class Song
{
    private string name;
    private string artist;
    private float length;
    private string thumbnail_url;
    private string source_url;

    // For Caching
    public Texture2D CachedTexture;

    public Song(string name, string artist, string thumbnail_url, float length, string source_url)
    {
        this.name = name;
        this.artist = artist;
        this.thumbnail_url = thumbnail_url;
        this.length = length;
        this.source_url = source_url;
    }

    public string GetStamina()
    {
        if (length < 120) return "Short";
        else if (length >= 120 && length < 240) return "Medium length";
        else return "Long";
    }

    public string GetName() => name;
    public string GetArtist() => artist;
    public string GetThumbnailURl() => thumbnail_url;
    public string GetUrl() => source_url; // Getter for the URL
}

public class LevelManagerBehaviour : MonoBehaviour
{
    public static LevelManagerBehaviour Instance { get; private set; }
    private InputSystem_Actions inputActions;

    [SerializeField]
    private List<string> urls = new List<string>();

    private List<Song> songs = new List<Song>();
    private List<GameObject> themes = new List<GameObject>();
    private List<GameObject> images = new List<GameObject>();
    private List<Song> shownSongs = new List<Song>();

    [SerializeField]
    private List<Transform> positions = new List<Transform>();

    [SerializeField]
    private TextMeshPro theme;
    [SerializeField]
    private MeshRenderer imagePrefab;

    public LevelManagerBehaviour()
    {
        Instance = this;
    }

    public void GetNext()
    {
        if (shownSongs.Count < 3) return;

        shownSongs[0] = shownSongs[1];
        shownSongs[1] = shownSongs[2];
        int index = songs.IndexOf(shownSongs[2]);
        if (songs.Count - 1 == index)
        {
            shownSongs[2] = songs[0];
        }
        else
        {
            shownSongs[2] = songs[index + 1];
        }
    }

    public void GetPrevious()
    {
        if (shownSongs.Count < 3) return;

        shownSongs[2] = shownSongs[1];
        shownSongs[1] = shownSongs[0];
        int index = songs.IndexOf(shownSongs[0]);
        if (index == 0)
        {
            shownSongs[0] = songs[songs.Count - 1];
        }
        else
        {
            shownSongs[0] = songs[index - 1];
        }
    }

    async Task Show()
    {
        foreach (var obj in themes) if (obj != null) Destroy(obj);
        themes.Clear();
        foreach (var obj in images) if (obj != null) Destroy(obj);
        images.Clear();

        for (int i = 0; i < shownSongs.Count; i++)
        {
            // Start loading thumbnail and passing the specific song object
            StartCoroutine(LoadThumbnail(shownSongs[i], i));

            // Create Text Info
            var sName = Instantiate(theme, positions[i].position, Quaternion.Euler(0, 270, 0));
            sName.text = "Name: " + shownSongs[i].GetName();
            themes.Add(sName.gameObject);

            var sArtist = Instantiate(theme, positions[i].position - new Vector3(0, 0.2f, 0), Quaternion.Euler(0, 270, 0));
            sArtist.text = "Artist: " + shownSongs[i].GetArtist();
            themes.Add(sArtist.gameObject);

            var sStamina = Instantiate(theme, positions[i].position - new Vector3(0, 0.4f, 0), Quaternion.Euler(0, 270, 0));
            sStamina.text = "Dauer: " + shownSongs[i].GetStamina();
            themes.Add(sStamina.gameObject);
        }
    }

    public IEnumerator LoadThumbnail(Song song, int i)
    {
        if (song.CachedTexture != null)
        {
            CreateImageObject(song, i);
            yield break;
        }

        string urlToProxy = song.GetThumbnailURl();
        if (string.IsNullOrEmpty(urlToProxy)) yield break;

        yield return new WaitForSeconds(i * 0.1f);

        FastAPIClient.Instance.GetProxyThumbnail(urlToProxy, (texture) =>
        {
            if (texture != null)
            {
                song.CachedTexture = texture;
                CreateImageObject(song, i);
            }
        });
    }

    private void CreateImageObject(Song song, int i)
    {
        MeshRenderer p = Instantiate(imagePrefab, positions[i].position + new Vector3(0, 1, 0), Quaternion.Euler(0, 270, 0));
        p.material.mainTexture = song.CachedTexture;

        if (p.TryGetComponent(out SceneLoaderBehaviour meinSkript))
        {
            meinSkript.url = song.GetUrl();
        }
        if (p.TryGetComponent(out LoadSongSceneEffect loadSongSceneEffect))
        {
            loadSongSceneEffect.url = song.GetUrl();
        }
        images.Add(p.gameObject);
    }

    async void Awake()
    {
        inputActions = new InputSystem_Actions();

        inputActions.Player.Next.performed += async ctx =>
        {
            GetNext();
            await Show();
        };
        inputActions.Player.Previous.performed += async ctx =>
        {
            GetPrevious();
            await Show();
        };

        await LoadSongs();

        // Initialize starting songs
        if (songs.Count >= 1) shownSongs.Add(songs[0]);
        if (songs.Count >= 2) shownSongs.Add(songs[1]);
        if (songs.Count >= 3) shownSongs.Add(songs[2]);

        await Show();
    }

    private async Task LoadSongs()
    {
        foreach (var url in urls)
        {
            var song = await LoadSongAsync(url);
            if (song != null) songs.Add(song);
        }
    }

    private Task<Song> LoadSongAsync(string url)
    {
        var tcs = new TaskCompletionSource<Song>();

        FastAPIClient.Instance.GetSongMetadataWithoutDownload(url, songData =>
        {
            if (songData == null)
            {
                Debug.LogError("Failed to process song at: " + url);
                tcs.SetResult(null); 
                return;
            }

            var song = new Song(
                songData.title,
                songData.artist,
                songData.thumbnail_url,
                (float)songData.duration,
                url 
            );

            tcs.SetResult(song);
        });

        return tcs.Task;
    }

    private void OnEnable() => inputActions.Player.Enable();
    private void OnDisable() => inputActions.Player.Disable();
}   