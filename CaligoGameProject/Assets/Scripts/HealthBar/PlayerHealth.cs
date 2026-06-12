using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("UI Settings")]
    public Slider healthSlider;

    // Flag estática para controlar a exibição da mensagem
    private static bool hasDied = false;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
        hasDied = false; // Reset flag ao iniciar a cena
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.value = (float)currentHealth / maxHealth;
        }
    }

    private void DisableMouseLookAndUnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        MouseLook mouseLook = FindObjectOfType<MouseLook>();
        if (mouseLook != null)
        {
            mouseLook.enabled = false;
        }
    }

    private void Die()
    {
        if (!hasDied)
        {
            Debug.Log("You die!");
            hasDied = true;
        }
        DeleteSliderFillArea();

        LoseScreen loseScreen = FindObjectOfType<LoseScreen>();
        if (loseScreen != null)
        {
            loseScreen.ShowLoseScreen();
        }

        DisableMouseLookAndUnlockCursor();
    }

    private void DeleteSliderFillArea()
    {
        if (healthSlider != null)
        {
            Transform fillArea = healthSlider.transform.Find("Fill Area");
            if (fillArea != null)
            {
                Destroy(fillArea.gameObject);
            }
        }
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthUI();
    }
}