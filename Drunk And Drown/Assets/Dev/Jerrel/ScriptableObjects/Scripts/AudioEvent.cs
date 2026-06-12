using UnityEngine;

[CreateAssetMenu(fileName = "NewAudioEvent", menuName = "Audio/Audio Event")]
public class AudioEvent : ScriptableObject
{
    [Header("Clips")]
    [SerializeField] private AudioClip[] clips;

    [Header("Volume Settings")]
    [Range(0f, 1f)] [SerializeField] private float volume = 1f;
    [Range(0f, 0.5f)] [SerializeField] private float volumeRandomRange = 0.05f;

    [Header("Pitch Settings")]
    [Range(0.1f, 3f)] [SerializeField] private float pitch = 1f;
    [Range(0f, 0.5f)] [SerializeField] private float pitchRandomRange = 0.1f;

    /// <summary>
    /// Play the event on a specified AudioSource
    /// </summary>
    public void Play(AudioSource source)
    {
        if (clips == null || clips.Length == 0) return;

        AudioClip clip = clips[Random.Range(0, clips.Length)];
        if (clip == null) return;

        source.clip = clip;
        source.volume = Mathf.Clamp(volume + Random.Range(-volumeRandomRange, volumeRandomRange), 0f, 1f);
        source.pitch = Mathf.Clamp(pitch + Random.Range(-pitchRandomRange, pitchRandomRange), 0.1f, 3f);
        source.Play();
    }

    /// <summary>
    /// Play the event as a one-shot on a specified AudioSource.
    /// Good for overlapping multiple sounds on a single source.
    /// Note: Changing pitch affects all currently playing sounds on the same AudioSource.
    /// </summary>
    public void PlayOneShot(AudioSource source)
    {
        if (clips == null || clips.Length == 0) return;

        AudioClip clip = clips[Random.Range(0, clips.Length)];
        if (clip == null) return;

        float calculatedVolume = Mathf.Clamp(volume + Random.Range(-volumeRandomRange, volumeRandomRange), 0f, 1f);
        float calculatedPitch = Mathf.Clamp(pitch + Random.Range(-pitchRandomRange, pitchRandomRange), 0.1f, 3f);
        
        source.pitch = calculatedPitch;
        source.PlayOneShot(clip, calculatedVolume);
    }
}
