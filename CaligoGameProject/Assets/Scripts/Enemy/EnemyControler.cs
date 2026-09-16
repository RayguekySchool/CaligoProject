using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyControler : MonoBehaviour
{
    private PlayerMove player;
    private NavMeshAgent nav;
    public Animator animator;
    private Coroutine damageCoroutine;

    [System.Obsolete]
    void Start()
    {
        player = FindObjectOfType<PlayerMove>();
        nav = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (player != null)
        {
            nav.SetDestination(player.transform.position);

            bool isMoving = nav.velocity.magnitude > 0.1f;
            if (animator != null)
                animator.SetBool("isWalking", isMoving);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                if (damageCoroutine == null)
                    damageCoroutine = StartCoroutine(DealDamageOverTime(playerHealth));
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && damageCoroutine != null)
        {
            StopCoroutine(damageCoroutine);
            damageCoroutine = null;
        }
    }

    private IEnumerator DealDamageOverTime(PlayerHealth playerHealth)
    {
        while (true)
        {
            playerHealth.TakeDamage(10);
            yield return new WaitForSeconds(2f);
        }
    }
}
