using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class Water : MonoBehaviour
{
    private AudioSource audioSource;
    [SerializeField] private AudioEvent waterSounds;
    private float duration = 10f;
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            StartCoroutine(Drowned());
        }
    }
    private IEnumerator Drowned()
    {
        float elapsed = 0;
        MainPlayer.Instance.GetComponent<Movement>().canMove = false;
        waterSounds.Play(audioSource);
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
