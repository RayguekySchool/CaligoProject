using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GenerateEnemies : MonoBehaviour
{
    public GameObject theEnemy;
    public int enemyCount;
    public int maxEnemies;
    public Vector2 spawnXRange;
    public Vector2 spawnZRange;
    public TextMeshProUGUI hordeText;

    private Transform spawnPoint;
    private int currentHorde = 1;
    private bool waitingForNextHorde = false;

    void Start()
    {
        spawnPoint = transform;
        ShowHordeText();
        StartCoroutine(EnemyDrop());
    }

    void Update()
    {
        if (!waitingForNextHorde && GameObject.FindGameObjectsWithTag("Enemy").Length == 0 && enemyCount == maxEnemies)
        {
            waitingForNextHorde = true;
            StartCoroutine(WaitAndStartNextHorde());
        }
    }

    IEnumerator EnemyDrop()
    {
        enemyCount = 0;
        while (enemyCount < maxEnemies)
        {
            float xPos = Random.Range(spawnXRange.x, spawnXRange.y);
            float zPos = Random.Range(spawnZRange.x, spawnZRange.y);

            // Y sempre 1
            Vector3 spawnPosition = new Vector3(spawnPoint.position.x + xPos, 1f, spawnPoint.position.z + zPos);

            Instantiate(theEnemy, spawnPosition, Quaternion.identity);
            yield return new WaitForSeconds(0.1f);
            enemyCount++;
        }
    }

    IEnumerator WaitAndStartNextHorde()
    {
        yield return new WaitForSeconds(5f);
        currentHorde++;
        ShowHordeText();
        waitingForNextHorde = false;
        StartCoroutine(EnemyDrop());
    }

    void ShowHordeText()
    {
        if (hordeText != null)
        {
            hordeText.text = $"Horde {currentHorde}";
            hordeText.gameObject.SetActive(true);
            StartCoroutine(HideHordeTextAfterSeconds(2f));
        }
    }

    IEnumerator HideHordeTextAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (hordeText != null)
        {
            hordeText.gameObject.SetActive(false);
        }
    }

    public void SetSpawnerPosition(Vector3 newPosition)
    {
        spawnPoint.position = newPosition;
    }
}
