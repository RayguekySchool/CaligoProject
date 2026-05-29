using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu_Manager : MonoBehaviour
{
    public string nameOfTheLevel1;
    public GameObject MainMenuPanel;
    public GameObject MenuPanelOptions;

    public void SelectLevel1()
    {
        SceneManager.LoadScene(nameOfTheLevel1);
    }

    public void OpenOptions()
    {
        MainMenuPanel.SetActive(false);
        MenuPanelOptions.SetActive(true);
    }

    public void CloseOptions()
    {
        MainMenuPanel.SetActive(true);
        MenuPanelOptions.SetActive(false);
    }

    public void Quit()
    {
        Debug.Log("Quited");
        Application.Quit();
    }
}