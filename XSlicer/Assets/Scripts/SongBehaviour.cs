using UnityEngine;

public class SongBehaviour : MonoBehaviour
{
    //private FastAPIClient aPIClient;

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
        //aPIClient.ProcessSong("https://www.youtube.com/watch?v=dQw4w9WgXcQ");
        timeCount = 0;
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
