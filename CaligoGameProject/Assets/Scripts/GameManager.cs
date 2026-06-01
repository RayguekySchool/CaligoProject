using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public int CoinsOnTheLevel;
    public AudioSource coinSound;
    public string NameOfTheNextLevel;
    public TextMeshProUGUI coinCounterText;
    private int collectedCoins = 0;

    public GameObject pauseMenu;

    void Start()
    {
        UpdateCoinUI();
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(false);
        }
    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }
    }

    public void DiscountCoin()
    {
        collectedCoins += 1;
        CoinsOnTheLevel -= 1;
        coinSound.Play();
        UpdateCoinUI();

        if (CoinsOnTheLevel <= 0)
        {
            SceneManager.LoadScene(NameOfTheNextLevel);
        }
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void UpdateCoinUI()
    {
        if (coinCounterText != null)
        {
            coinCounterText.text = collectedCoins + "/" + (collectedCoins + CoinsOnTheLevel);
        }
    }


    private void TogglePauseMenu()
    {
        if (pauseMenu != null)
        {
            bool isActive = pauseMenu.activeSelf;
            pauseMenu.SetActive(!isActive);
            Time.timeScale = isActive ? 1 : 0;
        }
    }
}