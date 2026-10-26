using UnityEngine;
using System.Collections;

public class TriggerOnStart : MonoBehaviour
{
    [Header("Configuração")]
    public AudioClip narrationClip;
    public float delayBeforePlay = 2f; // segundos de espera antes de tocar

    void Start()
    {
        StartCoroutine(PlayWithDelay());
    }

    private IEnumerator PlayWithDelay()
    {
        if (delayBeforePlay > 0)
            yield return new WaitForSeconds(delayBeforePlay);

        if (narrationClip != null && NarratorManager.Instance != null)
        {
            NarratorManager.Instance.PlayNarration(narrationClip);
        }
    }
}