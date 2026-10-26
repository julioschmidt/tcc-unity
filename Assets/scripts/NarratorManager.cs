using UnityEngine;

public class NarratorManager : MonoBehaviour
{
    public static NarratorManager Instance;
    [SerializeField] private AudioSource audioSource; // narrador fixo

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// Toca uma narração, vinda do narrador fixo.
    /// </summary>
    public void PlayNarration(AudioClip clip)
    {
        if (clip == null) return;

        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.spatialBlend = 0f; // 2D
        audioSource.transform.position = transform.position;
        audioSource.Play();
    }

    /// <summary>
    /// Toca uma narração vinda de um objeto no mundo.
    /// </summary>
    public void PlayNarration(AudioClip clip, Transform source)
    {
        if (clip == null || source == null) return;

        var go = new GameObject("One shot audio");
        var audio = go.AddComponent<AudioSource>();

        go.transform.position = source.position;

        audio.clip = clip;
        audio.spatialBlend = 1f;   // som 3D
        audio.minDistance = 200f;    // quando começa a atenuar
        audio.maxDistance = 400f;   // alcance máximo
        audio.dopplerLevel = 0f; // desativa efeito Doppler
        audio.Play();

        Destroy(go, clip.length);
    }

    public void StopNarration()
    {
        audioSource.Stop();
    }

    public bool IsPlaying => audioSource.isPlaying;
}
