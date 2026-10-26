using UnityEngine;

public class TriggerOnProximity : MonoBehaviour
{
    public AudioClip narrationClip;
    public Transform narrationSource; // opcional

    private bool alreadyTriggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (alreadyTriggered) return; // impede repetir

        if (other.CompareTag("Player"))
        {
            alreadyTriggered = true; // marca que já foi ativado

            if (narrationSource != null)
                NarratorManager.Instance.PlayNarration(narrationClip, narrationSource);
            else
                NarratorManager.Instance.PlayNarration(narrationClip);
        }
    }
}