using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(NavMeshAgent))]
public class ZombieEnemy : MonoBehaviour
{
    public enum ZombieState { Idle, Walk, Attack, Bite, Dead }

    [System.Serializable]
    public class BulletResistance
    {
        public string bulletType = "Pistol";
        public int bulletsToKill = 5;
        public int kills;
    }

    [Header("Current State")]
    public ZombieState currentState = ZombieState.Idle;

    [Header("AI Ranges")]
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float attackRange = 2.5f;
    [SerializeField] private float biteRange = 1.8f;

    [Header("AI Speeds")]
    [SerializeField] private float walkSpeed = 1.2f;

    [Header("AI Combat Parameters")]
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float biteDamage = 25f;
    [SerializeField] private float attackRate = 1.5f;
    [SerializeField] private float biteRate = 2.5f;

    [Header("Health & Resistance")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float destroyDelay = 1.5f;
    [SerializeField] private int killCounterIndex = 1; // 1 ou 2
    public ParticleSystem deathParticles;
    public List<BulletResistance> resistances = new List<BulletResistance>();

    [Header("UI References")]
    [SerializeField] private HealthBarCanva healthBar;

    // Statics para contagem global
    public static int kills1 = 0;
    public static int kills2 = 0;

    // Privados
    private float currentHealth;
    private bool isDead = false;
    private float nextTimeToAttack = 0f;
    private Dictionary<string, int> bulletHits = new Dictionary<string, int>();

    private Transform playerTransform;
    private NavMeshAgent agent;
    private Animator animator;
    private Collider zombieCollider;

    private static readonly int StateHash = Animator.StringToHash("state");

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        zombieCollider = GetComponent<Collider>();
        healthBar = GetComponentInChildren<HealthBarCanva>();

        currentHealth = maxHealth;

        // Inicializa o dicionário de tiros
        foreach (var res in resistances)
        {
            if (res != null && !string.IsNullOrEmpty(res.bulletType))
            {
                bulletHits[res.bulletType] = 0;
            }
        }
    }

    private void Start()
    {
        // Encontra o Player
        if (FirstPersonController.instance != null)
        {
            playerTransform = FirstPersonController.instance.transform;
        }
        else
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) playerTransform = playerObj.transform;
        }

        // Atualiza UI de Vida
        if (healthBar != null)
        {
            healthBar.UpdateHealthBar(currentHealth, maxHealth);
        }
    }

    private void Update()
    {
        if (isDead || playerTransform == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        DetermineState(distanceToPlayer);
        ExecuteStateBehavior();
        UpdateAnimator();
    }

    #region AI Logic

    private void DetermineState(float distance)
    {
        if (isDead) return;

        float buffer = 0.3f;

        if (currentState == ZombieState.Bite)
        {
            if (distance > biteRange + buffer) currentState = ZombieState.Attack;
        }
        else if (currentState == ZombieState.Attack)
        {
            if (distance <= biteRange) currentState = ZombieState.Bite;
            else if (distance > attackRange + buffer) currentState = ZombieState.Walk;
        }
        else if (currentState == ZombieState.Walk)
        {
            if (distance <= attackRange) currentState = ZombieState.Attack;
            else if (distance > detectionRange + buffer) currentState = ZombieState.Idle;
        }
        else // Idle
        {
            if (distance <= biteRange) currentState = ZombieState.Bite;
            else if (distance <= attackRange) currentState = ZombieState.Attack;
            else if (distance <= detectionRange) currentState = ZombieState.Walk;
        }
    }

    private void ExecuteStateBehavior()
    {
        if (isDead) return;

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
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
    }

    private void UpdateAnimator()
    {
        if (animator != null && !isDead)
        {
            animator.SetInteger(StateHash, (int)currentState);
        }
    }

    #endregion

    #region Health & Damage Logic

    public void TakeBullet(string bulletType)
    {
        if (isDead || string.IsNullOrEmpty(bulletType)) return;

        BulletResistance resistance = resistances.Find(x => x != null && x.bulletType.Equals(bulletType, StringComparison.OrdinalIgnoreCase));

        if (resistance == null || resistance.bulletsToKill <= 0)
        {
            currentHealth = Mathf.Clamp(currentHealth - (maxHealth / 5f), 0f, maxHealth);
        }
        else
        {
            if (!bulletHits.ContainsKey(resistance.bulletType))
                bulletHits[resistance.bulletType] = 0;

            bulletHits[resistance.bulletType]++;
            float damage = maxHealth / resistance.bulletsToKill;
            currentHealth = Mathf.Clamp(currentHealth - damage, 0f, maxHealth);
        }

        if (healthBar != null)
        {
            healthBar.UpdateHealthBar(currentHealth, maxHealth);
        }

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        currentState = ZombieState.Dead;

        // Atualiza o Animator diretamente
        if (animator != null)
        {
            animator.SetInteger(StateHash, (int)ZombieState.Dead);
        }

        // Desativa colisão e navegação
        if (zombieCollider != null) zombieCollider.enabled = false;

        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.velocity = Vector3.zero;
            agent.enabled = false;
        }

        // Atualiza UI de Kills
        if (killCounterIndex == 1)
        {
            kills1++;
            if (UImanager.instance != null) UImanager.instance.UpdateKillCounter1(kills1);
        }
        else if (killCounterIndex == 2)
        {
            kills2++;
            if (UImanager.instance != null) UImanager.instance.UpdateKillCounter2(kills2);
        }

        // Instancia partículas de morte
        if (deathParticles != null)
        {
            ParticleSystem ps = Instantiate(deathParticles, transform.position, Quaternion.identity);
            Destroy(ps.gameObject, ps.main.duration);
        }

        // Esconde barra de vida
        if (healthBar != null)
        {
            healthBar.gameObject.SetActive(false);
        }

        Destroy(gameObject, destroyDelay);
    }

    #endregion

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