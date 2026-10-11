
using UnityEngine;
using System.Collections.Generic;
using System;

public class EnemyHealth : MonoBehaviour
{
    [System.Serializable]
    public class BulletResistance
    {
        public string bulletType;
        public int bulletsToKill = 5;
        public int kills;
    }

    public ParticleSystem deathParticles;
    public List<BulletResistance> resistances = new List<BulletResistance>();

    private Dictionary<string, int> bulletHits = new Dictionary<string, int>();

    public static int kills1 = 0;
    public static int kills2 = 0;

    [SerializeField] private HealthBarCanva healthBar;
    [SerializeField] private float maxHealth = 100f;

    private float health;
    private bool isDead = false;

    public event Action OnDeath;

    [SerializeField] private int killCounterIndex = 1;

    private void Awake()
    {
        healthBar = GetComponentInChildren<HealthBarCanva>();
        health = maxHealth;

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
        if (healthBar != null)
        {
            healthBar.UpdateHealthBar(health, maxHealth);
        }
        else
        {
            Debug.LogWarning(
                $"[{name}] HealthBarCanva não foi encontrado nos filhos.",
                this
            );
        }
    }

    public void TakeBullet(string bulletType)
    {
        if (isDead || string.IsNullOrEmpty(bulletType))
            return;

        if (!bulletHits.ContainsKey(bulletType))
            return;

        BulletResistance resistance =
            resistances.Find(x => x != null && x.bulletType == bulletType);

        if (resistance == null || resistance.bulletsToKill <= 0)
            return;

        bulletHits[bulletType]++;

        float damage = maxHealth / resistance.bulletsToKill;
        health = Mathf.Clamp(health - damage, 0f, maxHealth);

        if (healthBar != null)
        {
            healthBar.UpdateHealthBar(health, maxHealth);
        }

        if (bulletHits[bulletType] >= resistance.bulletsToKill ||
            health <= 0f)
        {
            Die();
        }
    }

    public void Die()
    {
        // Impede que a morte e o contador sejam executados duas vezes.
        if (isDead)
            return;

        isDead = true;

        // Atualiza o contador de eliminações.
        if (killCounterIndex == 1)
        {
            kills1++;

            if (UImanager.instance != null)
                UImanager.instance.UpdateKillCounter1(kills1);
        }
        else if (killCounterIndex == 2)
        {
            kills2++;

            if (UImanager.instance != null)
                UImanager.instance.UpdateKillCounter2(kills2);
        }

        // Avisa o ZombieAI antes de destruir o objeto.
        OnDeath?.Invoke();

        // Cria as partículas de morte.
        if (deathParticles != null)
        {
            Instantiate(
                deathParticles,
                transform.position,
                Quaternion.identity
            );
        }

        Destroy(gameObject);
    }
}
