using TMPro;
using UnityEngine;

public class ClockBehaviour : MonoBehaviour
{
    private float score = 0;
    public float videoLength = 0;

    [SerializeField] TextMeshPro scoreText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(score < videoLength)
        {
            score += Time.deltaTime;
            scoreText.text = "Zeit: " + score + " / " + videoLength;
        }
        else
        {
            scoreText.text = "Zeit: " + videoLength + " (Fertig!)";
        }
        
    }
}
