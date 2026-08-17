using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    public void LoadScene(int i)
    {
        SceneController.LoadScene(i, 1, 2);
    }
}
