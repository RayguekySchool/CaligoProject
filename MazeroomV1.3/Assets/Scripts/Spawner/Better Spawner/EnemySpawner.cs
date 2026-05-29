using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EnemySpawner : MonoBehaviour
{
    [Header("Pontos de Spawn")]
    public List<Transform> spawnPoints = new List<Transform>();

    // Retorna até 'count' spawnpoints mais próximos (se tiver menos, retorna o que houver)
    public List<Transform> GetClosestSpawnPoints(Vector3 playerPos, int count)
    {
        if (spawnPoints == null || spawnPoints.Count == 0)
            return new List<Transform>();

        int take = Mathf.Clamp(count, 1, spawnPoints.Count);
        return spawnPoints
            .OrderBy(sp => Vector3.Distance(playerPos, sp.position))
            .Take(take)
            .ToList();
    }

    // Opção utilitária: pega um spawn point aleatório entre os 'count' mais próximos (retorna null se não houver)
    public Transform GetRandomClosestSpawnPoint(Vector3 playerPos, int count)
    {
        var list = GetClosestSpawnPoints(playerPos, count);
        if (list == null || list.Count == 0) return null;
        return list[Random.Range(0, list.Count)];
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        foreach (var sp in spawnPoints)
        {
            if (sp != null)
                Gizmos.DrawSphere(sp.position, 0.5f);
        }
    }
}
