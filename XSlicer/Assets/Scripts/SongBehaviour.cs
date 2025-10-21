using System;
using System.Collections;
using UnityEngine;

public class SongBehaviour : MonoBehaviour
{

    [SerializeField]
    private string songName;

    [SerializeField]
    private float bpm;

    [SerializeField]
    private GameObject cube;
    private float timeCount;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
   
    IEnumerator InstantiateCube()
    {
        while (true)
        {
            yield return new WaitForSeconds(60/bpm);
            Instantiate(cube, transform.position, Quaternion.identity);
        }
        
    }
    void Start()
    {
        Debug.Log("Test");
        /*
        FastAPIClient.Instance.ProcessSong("https://www.youtube.com/watch?v=Zv8czIoAw5w", (songData) =>
        {
            if (songData == null)
            {
                Debug.LogError("Failed to process song.");
                return;
            }

            Debug.Log(songData.rhythm_analysis.tempo_bpm);

            Debug.Log("Processed");
            bpm = (float)songData.rhythm_analysis.tempo_bpm;
            Debug.Log("Beats per minute" + bpm);

            StartCoroutine(InstantiateCube());
        }); 
        */
    }
    

    // Update is called once per frame
    void Update()
    {    
        
    }
}
