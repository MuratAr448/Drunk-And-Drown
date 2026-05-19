using UnityEngine;
using UnityEngine.SceneManagement;

public class UIButtonUtils : MonoBehaviour
{
    public void Navigate(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("You succefully quitted the game");
    }

    public void ToggleUI(GameObject UI)
    {
        UI.SetActive(!UI.activeSelf);
    }
}
