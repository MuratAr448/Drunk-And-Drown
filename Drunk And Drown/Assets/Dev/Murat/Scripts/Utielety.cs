using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Utielety : MonoBehaviour
{
    public void ToTitleScene()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("TitleScreen");
    }
    public void RestartScene()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void Quit()
    {
        Application.Quit();
    }
    public void UnPause()
    {
        FindFirstObjectByType<MainPlayer>().Pause();
    }
}
