using UnityEngine;

public class RotateObjectX : MonoBehaviour
{
    // Geschwindigkeit der Drehung um die X-Achse (Grad pro Sekunde)
    public float rotationSpeed = 50f;

    void Update()
    {
        // Drehe das Objekt um die eigene X-Achse
        transform.Rotate(rotationSpeed * Time.deltaTime, 0f, 0f);
    }
}
