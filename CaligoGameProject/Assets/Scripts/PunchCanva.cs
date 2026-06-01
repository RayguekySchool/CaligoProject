using UnityEngine;
using TMPro;

public class PunchCanva : MonoBehaviour
{
    public Animator animator;
    public float range = 2f; 
    public Camera fpsCam;
    public string bulletType = "Punch";
    public float punchRate = 0.8f;
    public int punchDamage = 10;

    public TextMeshProUGUI attackText;

    private float nextTimeToPunch = 0f;

    void Start()
    {
        if (attackText != null)
            attackText.gameObject.SetActive(false); 
    }

    void Update()
    {
        if (Time.time >= nextTimeToPunch && Input.GetButtonDown("Fire1"))
        {
            nextTimeToPunch = Time.time + punchRate;
            Punch();
        }
        else
        {
            animator.SetBool("Punch", false); 
        }
    }

    void Punch()
    {
        animator.SetTrigger("Punch");

        if (attackText != null && !attackText.gameObject.activeSelf)
            attackText.gameObject.SetActive(true);

        if (attackText != null)
            attackText.text = "Soco!";

        RaycastHit hit;
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range))
        {
            EnemyHealth enemy = hit.transform.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeBullet(bulletType); 
            }
        }
    }
}
