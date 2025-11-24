using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.LightTransport;



public class SongBehaviour : MonoBehaviour
{

    [SerializeField]
    private string songName;

    [SerializeField]
    private float bpm;

    [SerializeField]
    private List<GameObject> speedyThings;

    [SerializeField]
    private List<Transform> positions;

    private System.Random random;

    [SerializeField]
    private GameObject cube;
    private float timeCount;

    private float videoLength;   
    [SerializeField] TextMeshPro scoreText;

    private bool isProcessing = true;

    private AudioSource audioSource;
    IEnumerator InstantiateCube()
    {
        while (videoLength > float.Parse(string.Format("{0:F2}", timeCount), CultureInfo.InvariantCulture.NumberFormat))
        {
            
            yield return new WaitForSeconds(60 / bpm);
            int randy = random.Next(0, speedyThings.Count + 1);
            int randomRotation = random.Next(0, 360);
            if (randy == speedyThings.Count)
            {
        
                Instantiate(speedyThings[0], positions[0].position, Quaternion.identity);
                Instantiate(speedyThings[1], positions[1].position, Quaternion.identity);
                
            }
            else
            {
                Instantiate(speedyThings[randy], positions[randy].position, Quaternion.identity);
            }
        }
        
    }
    
    void Start()
    {
        StartCoroutine(Loading());
        random = new System.Random();
        Debug.Log("Test");
        
        FastAPIClient.Instance.ProcessSong("https://www.youtube.com/watch?v=Zv8czIoAw5w", (songData) =>
        {
            isProcessing = false;
            if (songData == null)
            {
                Debug.LogError("Failed to process song.");
                return;
            }

            Debug.Log(songData.rhythm_analysis.tempo_bpm);

            Debug.Log("Processed");

            bpm = (float)songData.rhythm_analysis.tempo_bpm;
            videoLength = (float)songData.duration;

            Debug.Log("Beats per minute" + bpm);

            StartCoroutine(InstantiateCube());
        }); 
    }
    

    IEnumerator Loading()
    {
        while (isProcessing)
        {
            scoreText.text = "Verarbeite Lied.";
            yield return new WaitForSeconds(0.5f);
            scoreText.text = "Verarbeite Lied..";
            yield return new WaitForSeconds(0.5f);
            scoreText.text = "Verarbeite Lied...";
            yield return new WaitForSeconds(0.5f);
        }
        
    }
    // Update is called once per frame
    void Update()
    {    
       if(!isProcessing && timeCount < videoLength)
       {
           timeCount += Time.deltaTime / 100;
            scoreText.text = "Zeit: " + float.Parse(string.Format("{0:F2}", timeCount), CultureInfo.InvariantCulture.NumberFormat) + " / " + float.Parse(string.Format("{0:F2}", videoLength), CultureInfo.InvariantCulture.NumberFormat)/100 ;
        }
        else if (!isProcessing)
        {
            scoreText.text = " LEVEL COMPLETE! (" + float.Parse(string.Format("{0:F2}", videoLength), CultureInfo.InvariantCulture.NumberFormat)/100 + "s) ";
        }
    }
}
