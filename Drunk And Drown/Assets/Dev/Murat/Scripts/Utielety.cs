using UnityEngine;
using UnityEngine.SceneManagement;

public class Utielety : MonoBehaviour
{
    public void ToGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("Murat test");
    }
    public void ToStartScreen()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("StartScreen");
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
