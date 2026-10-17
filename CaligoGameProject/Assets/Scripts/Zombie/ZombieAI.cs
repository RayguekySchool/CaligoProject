using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using System;

public class ZombieAI : MonoBehaviour
{
    public enum ZombieState { Idle, Walk, Attack, Bite, Dead }

    [System.Serializable]
    public class BulletResistance
    {
        public string bulletType = "pistol";
        public int bulletsToKill = 5;
    }

    [Header("Current State")]
    public ZombieState currentState = ZombieState.Idle;

    [Header("Ranges")]
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float attackRange = 2.5f;
    [SerializeField] private float biteRange = 1.8f;

    [Header("Speed")]
    [SerializeField] private float walkSpeed = 1.2f;

    [Header("Combat Parameters")]
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float biteDamage = 25f;
    [SerializeField] private float attackRate = 1.5f;
    [SerializeField] private float biteRate = 2.5f;

    [Header("Health & Damage Config")]
    [SerializeField] private float maxHealth = 100f;
    public ParticleSystem deathParticles;

    public List<BulletResistance> resistances =
        new List<BulletResistance>();

    private float currentHealth;

    private Dictionary<string, int> bulletHits =
        new Dictionary<string, int>();

    private Transform playerTransform;
    private NavMeshAgent agent;
    private Animator animator;
    private Collider zombieCollider;

    private float nextTimeToAttack = 0f;
    private bool isDead = false;

    private static readonly int StateHash =
        Animator.StringToHash("state");

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        zombieCollider = GetComponent<Collider>();

        currentHealth = maxHealth;

        foreach (var res in resistances)
        {
            if (res != null &&
                !string.IsNullOrEmpty(res.bulletType))
            {
                bulletHits[res.bulletType] = 0;
            }
        }
    }

    private void Start()
    {
        if (FirstPersonController.instance != null)
        {
            playerTransform =
                FirstPersonController.instance.transform;
        }
        else
        {
            GameObject playerObj =
                GameObject.FindGameObjectWithTag("Player");

            if (playerObj != null)
                playerTransform = playerObj.transform;
        }
    }

    private void Update()
    {
        if (isDead || playerTransform == null)
            return;

        float distanceToPlayer = Vector3.Distance(
            transform.position,
            playerTransform.position
        );

        DetermineState(distanceToPlayer);
        ExecuteStateBehavior();
        UpdateAnimator();
    }

    private void DetermineState(float distance)
    {
        if (isDead)
            return;

        float buffer = 0.3f;

        if (currentState == ZombieState.Bite)
        {
            if (distance > biteRange + buffer)
                currentState = ZombieState.Attack;
        }
        else if (currentState == ZombieState.Attack)
        {
            if (distance <= biteRange)
                currentState = ZombieState.Bite;
            else if (distance > attackRange + buffer)
                currentState = ZombieState.Walk;
        }
        else if (currentState == ZombieState.Walk)
        {
            if (distance <= attackRange)
                currentState = ZombieState.Attack;
            else if (distance > detectionRange + buffer)
                currentState = ZombieState.Idle;
        }
        else
        {
            if (distance <= biteRange)
                currentState = ZombieState.Bite;
            else if (distance <= attackRange)
                currentState = ZombieState.Attack;
            else if (distance <= detectionRange)
                currentState = ZombieState.Walk;
        }
    }

    private void ExecuteStateBehavior()
    {
        if (isDead)
            return;

        switch (currentState)
        {
            case ZombieState.Idle:
                if (agent != null && agent.enabled)
                {
                    agent.isStopped = true;
                    agent.velocity = Vector3.zero;
                }
                break;

            case ZombieState.Walk:
                if (agent != null && agent.enabled)
                {
                    agent.isStopped = false;
                    agent.updateRotation = true;
                    agent.speed = walkSpeed;
                    agent.SetDestination(playerTransform.position);
                }
                break;

            case ZombieState.Attack:
                if (agent != null && agent.enabled)
                {
                    agent.isStopped = true;
                    agent.velocity = Vector3.zero;
                    agent.updateRotation = false;
                }

                LookAtPlayer();

                if (Time.time >= nextTimeToAttack)
                {
                    nextTimeToAttack = Time.time + attackRate;
                    DealDamage(attackDamage);
                }
                break;

            case ZombieState.Bite:
                if (agent != null && agent.enabled)
                {
                    agent.isStopped = true;
                    agent.velocity = Vector3.zero;
                    agent.updateRotation = false;
                }

                LookAtPlayer();

                if (Time.time >= nextTimeToAttack)
                {
                    nextTimeToAttack = Time.time + biteRate;
                    DealDamage(biteDamage);
                }
                break;
        }
    }

    private void DealDamage(float damage)
    {
        if (FirstPersonController.OnTakeDamage != null)
        {
            FirstPersonController.OnTakeDamage.Invoke(damage);
        }
    }

    private void LookAtPlayer()
    {
        Vector3 direction =
            (playerTransform.position - transform.position).normalized;

        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion lookRotation =
                Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                lookRotation,
                Time.deltaTime * 5f
            );
        }
    }

    private void UpdateAnimator()
    {
        if (animator != null && !isDead)
        {
            animator.SetInteger(
                StateHash,
                (int)currentState
            );
        }
    }



    public void TakeBullet(string bulletType)
    {
        if (isDead)
        {
            Debug.LogWarning(name + " já está morto.");
            return;
        }

        if (string.IsNullOrEmpty(bulletType))
        {
            Debug.LogError("O tipo de bala está vazio!");
            return;
        }

        BulletResistance resistance = resistances.Find(
            x => x != null &&
            x.bulletType.Equals(
                bulletType,
                StringComparison.OrdinalIgnoreCase
            )
        );

        if (resistance == null)
        {
            Debug.LogWarning(
                "Nenhuma resistência encontrada para: " + bulletType
            );

            currentHealth -= maxHealth / 5f;
        }
        else
        {
            if (resistance.bulletsToKill <= 0)
            {
                Debug.LogError("Bullets To Kill precisa ser maior que zero.");
                return;
            }

            if (!bulletHits.ContainsKey(resistance.bulletType))
                bulletHits[resistance.bulletType] = 0;

            bulletHits[resistance.bulletType]++;

            float damage = maxHealth / resistance.bulletsToKill;
            currentHealth -= damage;

            Debug.Log(
                name + " recebeu tiro " +
                bulletHits[resistance.bulletType] +
                "/" + resistance.bulletsToKill +
                " | Dano: " + damage +
                " | Vida restante: " + currentHealth
            );
        }

        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        if (currentHealth <= 0f)
        {
            Debug.Log(name + " morreu! Executando Die().");
            Die();
        }
    }


    public void Die()
    {
        if (isDead)
            return;

        // Impede que o zumbi morra mais de uma vez.
        isDead = true;
        currentState = ZombieState.Dead;

        // Guarda a posição antes de destruir o zumbi.
        Vector3 deathPosition = transform.position;
        Quaternion deathRotation = transform.rotation;

        // Desativa a colisão.
        if (zombieCollider != null)
            zombieCollider.enabled = false;

        // Para e desativa o NavMeshAgent.
        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.velocity = Vector3.zero;
            agent.enabled = false;
        }

        // Interrompe a animação e a lógica do zumbi.
        if (animator != null)
            animator.enabled = false;

        // Cria uma partícula independente do zumbi.
        if (deathParticles != null)
        {
            ParticleSystem particles = Instantiate(
                deathParticles,
                deathPosition,
                deathRotation
            );

            // Garante que o efeito comece a tocar.
            particles.Play(true);

            // Destrói o efeito somente quando terminar.
            StartCoroutine(
                DestroyParticlesWhenFinished(particles)
            );
        }

        // Destrói o GameObject do zumbi imediatamente.
        Destroy(gameObject);
    }

    private IEnumerator DestroyParticlesWhenFinished(
        ParticleSystem particles)
    {
        if (particles == null)
            yield break;

        // Aguarda todas as partículas dos sistemas filhos
        // também terminarem.
        while (particles != null && particles.IsAlive(true))
        {
            yield return null;
        }

        if (particles != null)
        {
            Destroy(particles.gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.orange;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, biteRange);
    }
}
