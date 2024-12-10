using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    public Transform player; // Referencia al caballero
    public float speed = 2.0f; // Velocidad de movimiento
    public float detectionRange = 5.0f; // Rango para detectar al caballero
    public float attackRange = 1.5f; // Rango para atacar
    public float attackCooldown = 1.0f; // Tiempo entre ataques
    public int damage = 10; // Daño que inflige al jugador
    public int health = 50; // Vida del enemigo

    private Animator animator;
    private float lastAttackTime = 0.0f;
    private bool isDead = false;

    void Start()
    {
        animator = GetComponent<Animator>();

        // Buscar automáticamente al caballero si no está asignado
        if (player == null)
        {
            GameObject knight = GameObject.FindWithTag("Player");
            if (knight != null)
                player = knight.transform;
        }
    }

    void Update()
    {
        if (isDead || player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Si el jugador está dentro del rango de detección
        if (distanceToPlayer <= detectionRange)
        {
            // Si está en el rango de ataque, atacar
            if (distanceToPlayer <= attackRange)
            {
                AttackPlayer();
            }
            else
            {
                MoveTowardsPlayer();
            }
        }
        else
        {
            // Si el caballero está fuera del rango de detección, detener animaciones
            animator.SetBool("camina", false);
        }
    }

    void MoveTowardsPlayer()
    {
        if (isDead) return;

        // Activar animación de caminar
        animator.SetBool("camina", true);

        // Girar hacia el jugador
        Vector3 direction = (player.position - transform.position).normalized;

        // Asegurarse de que no se mueva en el eje Y
        direction.y = 0;

        // Mover al enemigo hacia el jugador
        transform.position += direction * speed * Time.deltaTime;

        // Ajustar rotación para mirar hacia el jugador
        if (direction.x > 0)
            transform.localScale = new Vector3(2, 2, 1); // Mirar a la derecha
        else
            transform.localScale = new Vector3(-2, 2, 1); // Mirar a la izquierda
    }

    void AttackPlayer()
    {
        // Verifica si el enemigo ha hecho el último ataque hace suficiente tiempo
        if (Time.time - lastAttackTime < attackCooldown) return;

        // Activar animación de ataque
        animator.SetBool("camina", false);  // Detiene la animación de caminar
        animator.SetBool("ataque1", true);

        // Reducir la vida del caballero si el enemigo está lo suficientemente cerca
        HeroKnight hero = player.GetComponent<HeroKnight>();
        if (hero != null)
        {
            hero.TakeDamage(damage);
        }

        // Registrar el último ataque
        lastAttackTime = Time.time;

        // Desactivar "ataque1" después de un tiempo
        StartCoroutine(ResetAttackState());
    }

    private IEnumerator ResetAttackState()
    {
        yield return new WaitForSeconds(0.5f); // Ajusta según la duración del clip de ataque
        animator.SetBool("ataque1", false);
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        health -= damage;

        Debug.Log("Enemy takes damage: " + damage + ". Health: " + health);

        if (health > 0)
        {
            animator.SetBool("herido", true); // Activar la animación de herido
            animator.SetBool("camina", false); // Detener la animación de caminar

            StartCoroutine(ResetHurtState()); // Esperar antes de desactivar "herido"
        }

        if (health <= 0) Die();
    }

    private IEnumerator ResetHurtState()
    {
        // Esperar el tiempo que dura la animación de "herido"
        yield return new WaitForSeconds(0.5f); // Ajusta el tiempo según la duración real del clip

        if (!isDead) // Solo desactiva "herido" si el enemigo sigue vivo
        {
            animator.SetBool("herido", false);
        }
    }

    void Die()
    {
        // Activar animación de muerte
        animator.SetBool("muere", true);

        isDead = true;

        // Desactivar colisiones y movimiento
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;
    }
}