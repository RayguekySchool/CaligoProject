using UnityEngine;
using System.Collections.Generic;
using System;

public class EnemyHealth : MonoBehaviour
{
    [System.Serializable]
    public class BulletResistance
    {
        public string bulletType = "Pistol";
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
    [SerializeField] private float destroyDelay = 1.5f;

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
    }

    public void TakeBullet(string bulletType)
    {
        if (isDead || string.IsNullOrEmpty(bulletType))
            return;

        BulletResistance resistance = resistances.Find(x => x != null && x.bulletType.Equals(bulletType, StringComparison.OrdinalIgnoreCase));

        if (resistance == null || resistance.bulletsToKill <= 0)
        {
            health = Mathf.Clamp(health - (maxHealth / 5f), 0f, maxHealth);
        }
        else
        {
            if (!bulletHits.ContainsKey(resistance.bulletType))
                bulletHits[resistance.bulletType] = 0;

            bulletHits[resistance.bulletType]++;
            float damage = maxHealth / resistance.bulletsToKill;
            health = Mathf.Clamp(health - damage, 0f, maxHealth);
        }

        if (healthBar != null)
        {
            healthBar.UpdateHealthBar(health, maxHealth);
        }

        if (health <= 0f)
        {
            Die();
        }
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

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

        OnDeath?.Invoke();

        if (deathParticles != null)
        {
            ParticleSystem ps = Instantiate(deathParticles, transform.position, Quaternion.identity);
            Destroy(ps.gameObject, ps.main.duration);
        }

        if (healthBar != null)
        {
            healthBar.gameObject.SetActive(false);
        }

        Destroy(gameObject, destroyDelay);
    }
}