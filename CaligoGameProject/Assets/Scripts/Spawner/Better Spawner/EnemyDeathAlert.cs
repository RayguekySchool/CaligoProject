using UnityEngine;
using System;

public class EnemyDeathAlert : MonoBehaviour
{
    public static event Action onDeath; // Evento para avisar o WaveManager

    private EnemyHealth health;

    private void Awake()
    {
        health = GetComponent<EnemyHealth>();
        if (health != null)
        {
            // Inscreve no evento de morte do EnemyHealth
            health.OnDeath += HandleDeath;
        }
    }

    private void OnDestroy()
    {
        if (health != null)
        {
            // Remove inscrição para evitar erros
            health.OnDeath -= HandleDeath;
        }
    }

    private void HandleDeath()
    {
        onDeath?.Invoke(); // Avisa para o WaveManager que morreu
    }
}
