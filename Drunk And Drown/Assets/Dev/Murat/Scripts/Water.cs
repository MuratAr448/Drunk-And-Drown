using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Water : MonoBehaviour
{
    private float duration = 3f;
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            StartCoroutine(Drowned());
        }
    }
    private IEnumerator Drowned()
    {
        float elapsed = 0;
        MainPlayer.Instance.GetComponent<Movement>().canMove = false;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            //get dark
        }
        yield return new WaitForEndOfFrame();
        RestartScene();
    }
    private void RestartScene()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
