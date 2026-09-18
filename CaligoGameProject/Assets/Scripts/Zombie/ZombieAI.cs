using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class ZombieAI : MonoBehaviour
{
    public int health = 100;

    public Transform Player;
    public float detectionRange = 10f;
    public float attackDistance = 3f;
    public float attackInterval = 2f;

    NavMeshAgent Agent;
    Animator anim;
    bool isDead = false;
    bool isAttacking= false;

    private void Start()
    {
        Agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        if (Player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                Player = playerObj.transform;
        }
    }

    private void Update()
    {
        if (isDead) return;

        float Distance = Vector3.Distance(transform.position, Player.position);

        if (Distance <= detectionRange)
        {
            StartCoroutine(PlayerAttackAnimation());
        }
        else
        {
            Agent.ResetPath();
            anim.SetBool("isWaking", false);

        }
    }

    public void Hit(int Damage)
    {
        health -= Damage;
        if (health <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(PlayHitAnimation());
        }
    }

    IEnumerator PlayHitAnimation()
    {
        anim.SetTrigger("Hit");
        yield return null;
    }

    IEnumerator PlayerAttackAnimation()
    {
        isAttacking = true;
        Agent.isStopped = true;
        anim.SetTrigger("Attack");

        yield return new WaitForSeconds(attackInterval);

        Agent.isStopped = false;
        isAttacking = false;
    }

    void Die()
    {
        isDead = true;
        Agent.isStopped = true;
        anim.SetBool("isWalking", false);
        Destroy(gameObject, 3f);
    })
}
