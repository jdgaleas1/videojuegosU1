using UnityEngine;
using TMPro; // Importa TextMeshPro

public class CameraFollow : MonoBehaviour
{
    public Transform player;   // Referencia al jugador
    public Vector3 offset;     // Desplazamiento de la cámara respecto al jugador
    public float smoothSpeed = 0.125f; // Velocidad de suavizado

    private float targetX = 157.27f; // Posición X que la cámara debe alcanzar
    private bool lockX = false; // Bandera para bloquear el seguimiento en X

    // Referencia a UI para mostrar la vida
    public TMP_Text vida;  // Texto para mostrar la vida
    private HeroKnight playerScript;  // Referencia al script del jugador (HeroKnight)

    void Start()
    {
        // Si no se ha asignado un jugador, intenta buscarlo en la escena
        if (player == null)
            player = GameObject.FindWithTag("Player").transform;

        // Establecer un desplazamiento inicial si no se ha definido
        if (offset == Vector3.zero)
            offset = transform.position - player.position;

        // Obtener el script del jugador
        playerScript = player.GetComponent<HeroKnight>();

        // Asegurarnos de que el texto de vida esté inicializado
        if (vida != null && playerScript != null)
        {
            vida.text = "Vida: " + playerScript.GetHealth();
        }
    }

    void FixedUpdate()
    {
        float newX = transform.position.x; // Inicializamos en la posición actual de la cámara
        float newY = transform.position.y;

        if (!lockX)
        {
            // Mientras la posición del jugador en X sea menor que targetX
            if (player.position.x < targetX)
            {
                newX = player.position.x + offset.x; // Sigue al jugador en X
            }
            else
            {
                // Fija la posición en X y bloquea el seguimiento en X
                newX = targetX;
                lockX = true; // Activamos el bloqueo del eje X
            }
        }
        else
        {
            // Si el seguimiento en X está bloqueado, asegura que la cámara esté centrada en targetX
            newX = Mathf.Lerp(transform.position.x, targetX, smoothSpeed);
        }

        // Sigue al jugador solo en Y
        newY = player.position.y + offset.y;

        // Calcula la nueva posición deseada con la posición de X y Y
        Vector3 desiredPosition = new Vector3(newX, newY, transform.position.z);

        // Suaviza la transición hacia la posición deseada
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // Asigna la nueva posición a la cámara
        transform.position = smoothedPosition;

        // Actualizar el texto de vida en la UI
        if (vida != null && playerScript != null)
        {
            vida.text = "Vida: " + playerScript.GetHealth();
        }
    }
}
