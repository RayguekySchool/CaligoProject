using UnityEngine;
using UnityEngine.AI;

public class ZombieAI : MonoBehaviour
{
    public enum ZombieState { Idle, Walk, Attack, Bite, Dead }

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

    private Transform playerTransform;
    private NavMeshAgent agent;
    private Animator animator;
    private EnemyHealth enemyHealth;

    private float nextTimeToAttack = 0f;
    private bool isDead = false;

    private static readonly int StateHash = Animator.StringToHash("state");

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        enemyHealth = GetComponent<EnemyHealth>();

        if (agent == null)
            Debug.LogError($"[{name}] NavMeshAgent não encontrado!", this);

        if (animator == null)
            Debug.LogError($"[{name}] Animator não encontrado!", this);

        if (enemyHealth == null)
            Debug.LogError($"[{name}] EnemyHealth não encontrado!", this);
    }

    private void Start()
    {
        // Busca o Player através do Singleton ou por Tag
        if (FirstPersonController.instance != null)
        {
            playerTransform = FirstPersonController.instance.transform;
        }
        else
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) playerTransform = playerObj.transform;
        }

        // Inscreve no evento de morte do EnemyHealth
        if (enemyHealth != null)
        {
            enemyHealth.OnDeath += HandleDeath;
        }
    }

    private void OnDestroy()
    {
        // Cancela a inscrição para evitar memory leaks/null pointers
        if (enemyHealth != null)
        {
            enemyHealth.OnDeath -= HandleDeath;
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

    // Chamado automaticamente quando Die() roda no EnemyHealth

    private void HandleDeath()
    {
        if (isDead)
            return;

        isDead = true;
        currentState = ZombieState.Dead;

        // Para o agente e impede novos movimentos.
        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.velocity = Vector3.zero;
            agent.enabled = false;
        }

        // Atualiza o estado de morte no Animator imediatamente.
        if (animator != null)
        {
            animator.SetInteger(StateHash, (int)ZombieState.Dead);
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