using UnityEngine;
using System.Collections;
public class TriggerProximityEnhanced : MonoBehaviour
{
    [Header("Fluxo do jogo")]
    public int fluxoIndex = 0;
    public float fadeDuration = 1f;
    public float extraWaitTime = 1f;
    public SceneFlowController sceneFlowController;
    [Header("Narrativa")]
    public AudioClip narrationClip;
    public Transform narrationSource;
    public string playerTag = "Player";
    [Header("Zoom temporário (opcional)")]
    public Transform tempZoomPosition;
    public Transform tempLookTarget;
    private bool alreadyTriggered = false;
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("ENTROY NO TRIGGER AMIGO");
        if (alreadyTriggered) return;
        if (!other.CompareTag(playerTag)) return;
        if (!PodeAtivarTrigger()) return;
        alreadyTriggered = true;
        Debug.Log("CARALHO PORRA MERDA");
        StartCoroutine(TriggerSequence());
    }
    private bool PodeAtivarTrigger()
    {
        if (sceneFlowController == null) return true;
        return sceneFlowController.CurrentIndex == fluxoIndex;
    }
    private IEnumerator TriggerSequence()
    {
        Debug.Log("ENTROY BNA PORRA DO TRIGGER");
        var motion = sceneFlowController?.motionObject;
        if (motion == null)
        {
            Debug.LogWarning("[TriggerProximityEnhanced] MotionObject não encontrado.");
            yield break;
        }
        Vector3 originalPos = motion.transform.position;
        Quaternion originalRot = motion.transform.rotation;
        // Teleporte temporário se houver
        if (tempZoomPosition != null)
        {
            Vector3 lookDir = tempLookTarget != null
                ? tempLookTarget.position - tempZoomPosition.position
                : Vector3.zero;
            yield return StartCoroutine(MoverComFadeTemporario(motion, tempZoomPosition.position, lookDir));
        }
        // Narração
        float waitTime = 0f;
        if (narrationClip != null && NarratorManager.Instance != null)
        {
            if (narrationSource != null)
                NarratorManager.Instance.PlayNarration(narrationClip, narrationSource);
            else
                NarratorManager.Instance.PlayNarration(narrationClip);
            waitTime = narrationClip.length;
        }
        yield return new WaitForSeconds((waitTime + extraWaitTime) / 2);
        // Voltar para posição original
        if (tempZoomPosition != null)
        {
            yield return StartCoroutine(MoverComFadeTemporario(motion, originalPos, originalRot * Vector3.forward));
        }
        // Avança fluxo
        sceneFlowController?.IrParaProximo(fadeDuration);
    }
    private IEnumerator MoverComFadeTemporario(MotionObjectController motion, Vector3 position, Vector3 lookDir)
    {
        var fade = sceneFlowController.fade;
        float originalFade = fade != null ? fade.fadeDuration : 1f;
        if (fade != null) fade.fadeDuration = fadeDuration;
        if (motion != null) motion.enabled = false;

        motion.transform.position = position;
        if (lookDir.sqrMagnitude > 0.001f)
            motion.transform.rotation = Quaternion.LookRotation(lookDir);
        if (motion.playerTransform != null)
        {
            motion.playerTransform.position = motion.transform.position;
            motion.playerTransform.rotation = motion.transform.rotation;
        }
        if (fade != null) fade.fadeDuration = originalFade;
        yield return new WaitForSeconds(sceneFlowController.delayBeforeMovement);
        if (motion != null) motion.enabled = true;
    }
}