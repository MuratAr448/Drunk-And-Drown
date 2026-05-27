using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DamageEffects : MonoBehaviour
{
    [Header("Volume Setup")]
    [SerializeField] private Volume globalVolume;

    [Header("Vignette Settings")]
    [SerializeField] private float intensityOnHit = 0.5f;
    [SerializeField] private float fadeDuration = 0.5f;

    private Vignette vignette;
    private Coroutine flashCoroutine;

    void Start()
    {
        if (globalVolume != null && globalVolume.profile.TryGet(out vignette))
        {
            vignette.intensity.value = 0f;
        }
        else
        {
            Debug.LogError("DamageEffects: Global Volume is missing, or Vignette override is missing from the Profile!", this);
        }
    }

    public void TakeDamageFlash(Color vignetteColor)
    {
        if (vignette == null) return;

        vignette.color.value = vignetteColor;

        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }

        flashCoroutine = StartCoroutine(FadeVignette());
    }


    private IEnumerator FadeVignette()
    {
        vignette.intensity.value = intensityOnHit;

        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            vignette.intensity.value = Mathf.Lerp(intensityOnHit, 0f, elapsedTime / fadeDuration);
            yield return null;
        }

        vignette.intensity.value = 0f;
    }
}