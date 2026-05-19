using UnityEngine;
using UnityEngine.SceneManagement;

public class UIButtonUtils : MonoBehaviour
{
    public void Navigate(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
