using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyControler : MonoBehaviour
{
    private PlayerMove player;
    private NavMeshAgent nav;
    public Animator animator; // Animator reference
    private Coroutine damageCoroutine;

    [System.Obsolete]
    void Start()
    {
        player = FindObjectOfType<PlayerMove>();
        nav = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>(); // Get Animator component
    }

    void Update()
    {
        if (player != null)
        {
            nav.SetDestination(player.transform.position);

            // Check if enemy is moving
            bool isMoving = nav.velocity.magnitude > 0.1f;
            if (animator != null)
                animator.SetBool("isWalking", isMoving); // Trigger walking animation
        }
    }

    // These need to be class-level methods so Unity can call them
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
