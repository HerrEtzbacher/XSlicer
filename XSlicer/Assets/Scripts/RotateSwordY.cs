using UnityEngine;

public class RotateObjectY : MonoBehaviour
{
    // Geschwindigkeit der Drehung um die X-Achse (Grad pro Sekunde)
    public float rotationSpeed = 50f;

    void Update()
    {
        // Drehe das Objekt um die eigene X-Achse
        transform.Rotate(0f,0f, rotationSpeed *Time.deltaTime);
    }
}
