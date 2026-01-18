using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.UIElements;
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

    //Fürs Caching
    public Texture2D CachedTexture;


    public Song(string name, string artist, string thumbnail_url, float length)
    {
        this.name = name;
        this.artist = artist;
        this.thumbnail_url = thumbnail_url;
        this.length = length;
    }

    //Without song download = efficient BUT no BPM available...
    //With song download = inefficient BUT BPM available
    /*
    public string GetDifficultiy()
    {
        
        if(bpm < 80)
        {
            return "Easy";
        }
        else if(bpm >= 80 && bpm < 140)
        {
            return "Medium";
        }
        else
        {
            return "Hard";
        }
        
        
    }
    */

    public string GetStamina()
    {
        if(length < 120)
        {
            return "Short";
        }
        else if(length >= 120 && length < 240)
        {
            return "Medium length";
        }
        else
        {
            return "Long";
        }
    }

    public string GetName()
    {
        return name;
    }
    public string GetArtist()
    {
        return artist;
    }
    public string GetThumbnailURl()
    {
        return thumbnail_url;
    }
}

public class LevelManagerBehaviour : MonoBehaviour
{
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
    void GetNext()
    {
        shownSongs[0] = shownSongs[1];
        shownSongs[1] = shownSongs[2];
        int index = songs.IndexOf(shownSongs[2]);
        if(songs.Count - 1 == index)
        {
            shownSongs[2] = songs[0];
        }
        else
        {
            shownSongs[2] = songs[index + 1];    
        }
    }
    void GetPrevious()
    {
        shownSongs[2] = shownSongs[1];
        shownSongs[1] = shownSongs[0];
        int index = songs.IndexOf(shownSongs[0]);
        if(index == 0)
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
        foreach (var obj in themes) Destroy(obj);
            themes.Clear();
        foreach (var obj in images) Destroy(obj);
            images.Clear();
        for(int i = 0; i < positions.Count; i++)
        {
            
            StartCoroutine(LoadThumbnail(shownSongs[i], i));

            theme.text = "Name: " + shownSongs[i].GetName();
            var s = Instantiate(theme, positions[i].position, Quaternion.Euler(0,270,0));
            s.text = "Name: " + shownSongs[i].GetName();
            themes.Add(s.gameObject);

            theme.text = "Artist: " + shownSongs[i].GetArtist();
            s = Instantiate(theme, positions[i].position - new Vector3(0,0.2f,0), Quaternion.Euler(0,270,0));
            s.text = "Artist: " + shownSongs[i].GetArtist();
            themes.Add(s.gameObject);
            
            theme.text = "Dauer: " + shownSongs[i].GetStamina();
            s = Instantiate(theme, positions[i].position - new Vector3(0,0.4f,0), Quaternion.Euler(0,270,0));
            s.text = "Dauer: " + shownSongs[i].GetStamina();
            themes.Add(s.gameObject);

            
        }
    }

    public IEnumerator LoadThumbnail(Song song, int i)
    {
        if (song.CachedTexture != null)
        {
            CreateImageObject(song.CachedTexture, i);
            yield break;
        }

        string urlToProxy = song.GetThumbnailURl();
        if (string.IsNullOrEmpty(urlToProxy)) yield break;

        yield return new WaitForSeconds(i * 0.1f);

        FastAPIClient.Instance.GetProxyThumbnail(urlToProxy, (texture) => {
            if (texture != null)
            {
                song.CachedTexture = texture;
                CreateImageObject(texture, i);
            }
        });
    }

    private void CreateImageObject(Texture2D tex, int i)
    {
        
        MeshRenderer p = Instantiate(imagePrefab, positions[i].position + new Vector3(0, 1, 0), Quaternion.Euler(0, 270, 0));
        p.material.mainTexture = tex;
        if (p.TryGetComponent(out SceneLoaderBehaviour meinSkript)) 
        {
            meinSkript.url = urls[i]; 
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

        if(songs.Count >= 1)
        {
            shownSongs.Add(songs[0]);   
        }
        if(songs.Count >= 2)
        {
            shownSongs.Add(songs[1]);   
        }
        if(songs.Count >= 3)
        {
            shownSongs.Add(songs[2]);
        }

        Debug.Log("Shown songs " + shownSongs.Count);
        Debug.Log("Songs " + songs.Count);
        Debug.Log("Positions " + positions.Count);

        await Show();
       
        Debug.Log("Songs loaded: " + songs.Count);
        
    }

    private async Task LoadSongs()
{
    foreach (var url in urls)
    {
        var song = await LoadSongAsync(url);
        songs.Add(song);
    }
}

    private Task<Song> LoadSongAsync(string url)
{
    var tcs = new TaskCompletionSource<Song>();
 
    FastAPIClient.Instance.GetSongMetadataWithoutDownload(url, songData =>
    {
        if (songData == null)
        {
            tcs.SetException(new Exception("Failed to process song"));
            return;
        }

        
 
        var song = new Song(
            songData.title,
            songData.artist,
            songData.thumbnail_url,
            (float)songData.duration
        );
        Debug.Log("Song processed, check out the metadata: ");
        Debug.Log(song.GetName());
        Debug.Log(song.GetArtist());
        Debug.Log(song.GetStamina());

 
        tcs.SetResult(song);
    });
 
    return tcs.Task;
}
    private void OnEnable()
    {
        inputActions.Player.Enable();
    }
    private void OnDisable()
    {
        inputActions.Player.Disable();
    }
    void Start()
    {
        
        
    }

    void Update()
    {
        
    }
}
