using UnityEngine;

public class LoseScreen : MonoBehaviour
{
    public GameObject loseCanvas;

    public void ShowLoseScreen() 
    {
        if (loseCanvas != null) 
        { 
            loseCanvas.SetActive(true); Time.timeScale = 1f; 
        }
    }
}
