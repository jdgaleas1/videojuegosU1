using UnityEngine;
using System.Collections;

public class HeroKnight : MonoBehaviour
{
    [SerializeField] int m_attackDamage = 20; // Daño del ataque
    [SerializeField] LayerMask m_enemyLayer; // Capa de los enemigos
    [SerializeField] private float m_speed = 4.0f;
    [SerializeField] private float m_jumpForce = 7.5f;
    [SerializeField] private float m_rollForce = 6.0f;
    [SerializeField] private bool m_noBlood = false;
    [SerializeField] private GameObject m_slideDust;
    [SerializeField] public int m_health = 100; // Vida inicial del caballero
    [SerializeField] private AudioClip m_jumpSound;  // Sonido del salto
    [SerializeField] private AudioClip m_attackSound;  // Sonido del ataque
    [SerializeField] private AudioClip m_deathSound;  // Sonido de la muerte
    [SerializeField] private float fallDamageThreshold = 10.0f; // Velocidad mínima para recibir daño
    [SerializeField] private int maxFallDamage = 50; // Daño máximo que puede causar una caída

    private AudioSource m_audioSource;  // Componente de AudioSource

    private Animator m_animator;
    private Rigidbody2D m_body2d;
    private Sensor_HeroKnight m_groundSensor;
    private Sensor_HeroKnight m_wallSensorR1;
    private Sensor_HeroKnight m_wallSensorR2;
    private Sensor_HeroKnight m_wallSensorL1;
    private Sensor_HeroKnight m_wallSensorL2;
    private bool m_isWallSliding = false;
    private bool m_grounded = false;
    private bool m_rolling = false;
    private int m_facingDirection = 1;
    private int m_currentAttack = 0;
    private float m_timeSinceAttack = 0.0f;
    private float m_delayToIdle = 0.0f;
    private float m_rollDuration = 8.0f / 14.0f;
    private float m_rollCurrentTime;
    private bool m_isAttacking = false; // Indica si el jugador está atacando
    private float m_lastAttackTime = 0f;
    private bool m_isBlocking = false; // Indica si el jugador está bloqueando
        // Asegúrate de que tu collider para la caída esté aquí
    private BoxCollider2D m_fallCollider;
    private float maxFallSpeed = 0f; // Velocidad vertical máxima durante la caída

    [SerializeField] private float attackRange = 1.0f;
[SerializeField] private LayerMask enemyLayer;

    // Inicialización
    void Start()
    {
        m_animator = GetComponent<Animator>();
        m_body2d = GetComponent<Rigidbody2D>();
                m_audioSource = GetComponent<AudioSource>();  // Obtén el AudioSource
        m_groundSensor = transform.Find("GroundSensor").GetComponent<Sensor_HeroKnight>();
        m_wallSensorR1 = transform.Find("WallSensor_R1").GetComponent<Sensor_HeroKnight>();
        m_wallSensorR2 = transform.Find("WallSensor_R2").GetComponent<Sensor_HeroKnight>();
        m_wallSensorL1 = transform.Find("WallSensor_L1").GetComponent<Sensor_HeroKnight>();
        m_wallSensorL2 = transform.Find("WallSensor_L2").GetComponent<Sensor_HeroKnight>();

                // Obtener el BoxCollider2D de la parte inferior (fallCollider)
        m_fallCollider = GetComponent<BoxCollider2D>(); 
        if (m_fallCollider == null)
        {
            Debug.LogError("Falta el BoxCollider2D para la detección de caída.");
        }
    }

    // Update es llamado una vez por frame
    void Update()
    {
        // Control de tiempo para combos y animaciones
        m_timeSinceAttack += Time.deltaTime;

            if (!m_grounded && m_body2d.linearVelocity.y < maxFallSpeed)
    {
        maxFallSpeed = m_body2d.linearVelocity.y;
    }if (!m_grounded && m_groundSensor.State())
{
    m_grounded = true;
    m_animator.SetBool("Grounded", m_grounded);

    // Revisa si se debe aplicar daño por caída
    CheckFallDamage();
}


        if (m_rolling)
            m_rollCurrentTime += Time.deltaTime;

        if (m_rollCurrentTime > m_rollDuration)
            m_rolling = false;

        // Control del estado en tierra
        if (!m_grounded && m_groundSensor.State())
        {
            m_grounded = true;
            m_animator.SetBool("Grounded", m_grounded);
        }

        if (m_grounded && !m_groundSensor.State())
        {
            m_grounded = false;
            m_animator.SetBool("Grounded", m_grounded);
        }

        // Movimiento horizontal y animaciones
        float inputX = Input.GetAxis("Horizontal");

        if (!m_isBlocking) // Solo permitir movimiento si no está bloqueando
        {
            if (inputX > 0)
            {
                GetComponent<SpriteRenderer>().flipX = false;
                m_facingDirection = 1;
            }
            else if (inputX < 0)
            {
                GetComponent<SpriteRenderer>().flipX = true;
                m_facingDirection = -1;
            }

            if (!m_rolling)
                m_body2d.linearVelocity = new Vector2(inputX * m_speed, m_body2d.linearVelocity.y);

            m_animator.SetFloat("AirSpeedY", m_body2d.linearVelocity.y);
        }
        else
        {
            // Si está bloqueando, mantener la velocidad en 0
            m_body2d.linearVelocity = new Vector2(0, m_body2d.linearVelocity.y);
        }

        // Control de animaciones
        m_isWallSliding = (m_wallSensorR1.State() && m_wallSensorR2.State()) || (m_wallSensorL1.State() && m_wallSensorL2.State());
        m_animator.SetBool("WallSlide", m_isWallSliding);

        // Bloqueo (escudo)
        if (Input.GetMouseButtonDown(1) && !m_rolling)
        {
            m_isBlocking = true;
            m_animator.SetTrigger("Block");
            m_animator.SetBool("IdleBlock", true);
        }
        else if (Input.GetMouseButtonUp(1))
        {
            m_isBlocking = false;
            m_animator.SetBool("IdleBlock", false);
        }

        // Otros controles
        if (Input.GetKeyDown("e") && !m_rolling)
        {
            m_animator.SetBool("noBlood", m_noBlood);
            m_animator.SetTrigger("Death");
        }
        else if (Input.GetKeyDown("q") && !m_rolling)
        {
            m_animator.SetTrigger("Hurt");
        }
        else if (Input.GetMouseButtonDown(0) && m_timeSinceAttack > 0.25f && !m_rolling)
        {
            m_currentAttack++;
            if (m_currentAttack > 3)
                m_currentAttack = 1;
            if (m_timeSinceAttack > 1.0f)
                m_currentAttack = 1;

            m_animator.SetTrigger("Attack" + m_currentAttack);
            m_timeSinceAttack = 0.0f;
                        // Reproducir sonido de ataque
            if (m_attackSound != null && m_audioSource != null)
            {
                m_audioSource.PlayOneShot(m_attackSound);  // Reproduce el sonido de ataque
            }
               // Ejecutar el ataque al enemigo
    PerformAttack(); // Llamada al método para infligir daño a enemigos en el rango
        }
        else if (Input.GetKeyDown("left shift") && !m_rolling && !m_isWallSliding)
        {
            m_rolling = true;
            m_animator.SetTrigger("Roll");
            m_body2d.linearVelocity = new Vector2(m_facingDirection * m_rollForce, m_body2d.linearVelocity.y);
        }
        else if (Input.GetKeyDown("space") && m_grounded && !m_rolling)
        {
            m_animator.SetTrigger("Jump");
            m_grounded = false;
            m_animator.SetBool("Grounded", m_grounded);
            m_body2d.linearVelocity = new Vector2(m_body2d.linearVelocity.x, m_jumpForce);
            m_groundSensor.Disable(0.2f);
            // Reproducir sonido del salto
            if (m_jumpSound != null && m_audioSource != null)
            {
                m_audioSource.PlayOneShot(m_jumpSound);
            }

        }
        else if (Mathf.Abs(inputX) > Mathf.Epsilon && !m_isBlocking)
        {
            m_delayToIdle = 0.05f;
            m_animator.SetInteger("AnimState", 1);
        }
        else
        {
            m_delayToIdle -= Time.deltaTime;
            if (m_delayToIdle < 0)
                m_animator.SetInteger("AnimState", 0);
        }
    }

    public void TakeDamage(int damage)
    {
        if (m_isBlocking)
        {
            Debug.Log("Daño bloqueado.");
            m_animator.SetTrigger("ShieldBlock"); // Activar animación de escudo bloqueando
            return;
        }

        m_health -= damage;
        m_health = Mathf.Clamp(m_health, 0, 100); // Asegúrate de que no pase de los límites
        m_animator.SetTrigger("Hurt");

        if (m_health <= 0)
        {
            Die();
        }
    }
    void OnGUI()
{
    // Configura el estilo del texto
    GUIStyle style = new GUIStyle();
    style.fontSize = 24; // Tamaño de fuente
    style.normal.textColor = Color.white; // Color del texto
    style.alignment = TextAnchor.MiddleLeft; // Alinear texto al lado izquierdo

    // Fondo detrás del texto
    Texture2D backgroundTexture = new Texture2D(1, 1);
    backgroundTexture.SetPixel(0, 0, new Color(0, 0, 0, 0.5f)); // Negro semitransparente
    backgroundTexture.Apply();
    style.normal.background = backgroundTexture;

    // Posición del texto en la esquina superior izquierda dentro del mapa
    Rect textRect = new Rect(50, 20, 200, 50);

    // Dibujar fondo
    GUI.Box(textRect, GUIContent.none, style);

    // Dibujar texto encima
    GUI.Label(textRect, "Vida: " + m_health, style);
}

    void Die()
    {
                // Reproducir sonido de muerte
        if (m_deathSound != null && m_audioSource != null)
        {
            m_audioSource.PlayOneShot(m_deathSound);  // Reproduce el sonido de muerte
        }

        m_animator.SetTrigger("Death");
        m_body2d.linearVelocity = Vector2.zero;
        this.enabled = false;
        Debug.Log("El caballero ha muerto");
    }

    void AE_SlideDust()
    {
        Vector3 spawnPosition = m_facingDirection == 1 ? m_wallSensorR2.transform.position : m_wallSensorL2.transform.position;

        if (m_slideDust != null)
        {
            GameObject dust = Instantiate(m_slideDust, spawnPosition, gameObject.transform.localRotation);
            dust.transform.localScale = new Vector3(m_facingDirection, 1, 1);
        }
    }

    void Attack()
    {
        // Inicia el ataque
        m_isAttacking = true;
        m_lastAttackTime = Time.time;

        // Reproducir animación de ataque
        m_animator.SetTrigger("Attack");

        // Configurar un temporizador para finalizar el ataque
        Invoke("EndAttack", 0.2f); // Ajusta el tiempo según la animación de ataque
    }

    void EndAttack()
    {
        m_isAttacking = false;
    }

    // Detectar colisiones con enemigos
    void OnTriggerEnter2D(Collider2D other)
    {
        // Verificar si el personaje ha tocado un área de caída
        if (other.CompareTag("FallArea"))
        {
            // Si el personaje entra en una zona de muerte, muere
            Die();
        }

        // También puedes detectar otras interacciones, como el ataque a enemigos
        if (m_isAttacking && ((1 << other.gameObject.layer) & m_enemyLayer) != 0)
        {
            // Infligir daño al enemigo
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(m_attackDamage);
            }
        }
        }
void OnCollisionEnter2D(Collision2D collision)
{
    if (collision.gameObject.CompareTag("Enemy") && m_isAttacking)
    {
        // Si estás atacando, aplica daño al enemigo
        Enemy enemy = collision.gameObject.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(m_attackDamage);
        }
    }
}
void PerformAttack()
{
    // Detecta todos los enemigos en el rango del ataque
    Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, attackRange, enemyLayer);

    // Itera sobre todos los enemigos golpeados
    foreach (Collider2D enemyCollider in hitEnemies)
    {
        Enemy enemy = enemyCollider.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(m_attackDamage); // Aplica el daño al enemigo
        }
    }
}
public int GetHealth()
{
    return m_health;
}

void CheckFallDamage()
{
    if (Mathf.Abs(maxFallSpeed) > fallDamageThreshold)
    {
        // Calcula el daño basado en la velocidad de caída
        int fallDamage = Mathf.Clamp((int)((Mathf.Abs(maxFallSpeed) - fallDamageThreshold) * 2), 0, maxFallDamage);

        // Aplica el daño al personaje
        TakeDamage(fallDamage);
        Debug.Log($"Daño por caída: {fallDamage}");
    }

    // Reinicia la velocidad máxima de caída
    maxFallSpeed = 0f;
}



}
