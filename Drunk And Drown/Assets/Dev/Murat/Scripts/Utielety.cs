using UnityEngine;
using UnityEngine.SceneManagement;

public class Utielety : MonoBehaviour
{
    public void ToGame()
    {
        SceneManager.LoadScene("Movement Scene");
    }
    public void ToStartScreen()
    {
        SceneManager.LoadScene("StartScreen");
    }
    public void UnPause()
    {
        FindObjectOfType<Player>().Pause();
    }
}
