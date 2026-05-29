using UnityEngine;
using UnityEngine.SceneManagement;

public class Utielety : MonoBehaviour
{
    [SerializeField] private string sceneName;
    public void ToScene()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(sceneName);
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
