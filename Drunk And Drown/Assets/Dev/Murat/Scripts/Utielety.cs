using Unity.VectorGraphics;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Utielety : MonoBehaviour
{
    public SceneAsset scene;
    public void ToScene()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(scene.name);
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
