using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class SceneFlowController : MonoBehaviour
{
    [Header("Referências")]
    public MotionObjectController motionObject; // Player/motionObject
    public Transform[] pontos;                  // Pontos de teleporte
    public Transform[] lookAtTargets;          // Pontos para olhar após teleporte
    public float delayBeforeMovement = 1f;      // tempo antes de liberar movimento
    public FadeController fade;                 // referência do fade
    public AudioSource som;

    private int currentIndex = 0;
    public int CurrentIndex => currentIndex;

    //private void Start()
    //{
    //    IrParaProximo(1);
    //}

    public void IrParaProximo(float fadeTime = -1f)
    {
        // Se já chegou no último ponto  finaliza o app
        if (currentIndex >= pontos.Length - 1)
        {
            Debug.Log("[SceneFlowController] Todos os pontos concluídos. Exibindo tela preta final...");
            StartCoroutine(EncerrarFluxo());
            return;
        }

        currentIndex = currentIndex + 1;
        // Caso contrário continua o fluxo normal
        Transform lookTarget = (lookAtTargets != null && currentIndex < lookAtTargets.Length)
                                ? lookAtTargets[currentIndex]
                                : null;

        

        StartCoroutine(MoverComFade(pontos[currentIndex], fadeTime, lookTarget));
    }

    private IEnumerator MoverComFade(Transform destino, float fadeTime, Transform lookTarget)
    {
        if (motionObject != null) motionObject.enabled = false;

        // Guardar fade original
        float originalFade = fade != null ? fade.fadeDuration : 1f;

        // Ajustar fade se passado
        if (fade != null && fadeTime > 0f)
            fade.fadeDuration = fadeTime;

        // FadeOut
        if (fade != null) yield return StartCoroutine(fade.FadeOut());

        // Teleporte
        if (motionObject != null)
        {
            motionObject.transform.position = destino.position;

            // Rotação para lookTarget se fornecido
            if (lookTarget != null)
            {
                Vector3 dir = lookTarget.position - destino.position;
                dir.y = 0; // manter eixo Y estável
                if (dir.sqrMagnitude > 0.001f)
                    motionObject.transform.rotation = Quaternion.LookRotation(dir);
            }
            else
            {
                motionObject.transform.rotation = destino.rotation;
            }

            // Atualizar playerTransform (garantia extra)
            if (motionObject.playerTransform != null)
            {
                motionObject.playerTransform.position = motionObject.transform.position;
                motionObject.playerTransform.rotation = motionObject.transform.rotation;
            }
        }

        // FadeIn
        if (fade != null) yield return StartCoroutine(fade.FadeIn());

        // Restaurar fade original
        if (fade != null)
            fade.fadeDuration = originalFade;

        // Delay antes de liberar movimento
        yield return new WaitForSeconds(delayBeforeMovement);
        if (motionObject != null) motionObject.enabled = true;
    }

    private IEnumerator EncerrarFluxo()
    {
        // força um fade rápido para preto
        if (fade != null)
        {
            float originalFade = fade.fadeDuration;
            fade.fadeDuration = 0.5f; // tempo rápido de fade
            yield return StartCoroutine(fade.FadeOut());
            fade.fadeDuration = originalFade;
        }

        // desliga o som
        if (som != null)
            som.Stop();

        // garante que fique preto para sempre (sem FadeIn)
        Debug.Log("[SceneFlowController] Cena finalizada, tela preta e som desligado.");
    }
}
