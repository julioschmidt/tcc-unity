using UnityEngine;

public class TriggerOnButton : MonoBehaviour
{
    public AudioClip narrationClip;
    public Transform narrationSource; // opcional

    public void OnButtonClicked()
    {
        if (narrationSource != null)
            NarratorManager.Instance.PlayNarration(narrationClip, narrationSource);
        else
            NarratorManager.Instance.PlayNarration(narrationClip);
    }
}