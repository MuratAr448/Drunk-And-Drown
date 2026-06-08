using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Utielety : MonoBehaviour
{
    private string sceneName;
    [SerializeField] private SceneAsset scene;
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
#if UNITY_EDITOR
    public void OnAfterDeserialize() => FillScene();
    public void OnBeforeSerialize() => FillScene();
    public void OnValidate() => FillScene();
    private void FillScene()
    {
        if (scene != null)
        {
            sceneName = scene.name;
        }
    }
#endif
    public void Quit()
    {
        Application.Quit();
    }
    public void UnPause()
    {
        FindFirstObjectByType<MainPlayer>().Pause();
    }
}
