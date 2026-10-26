using UnityEngine;

public class NPCInteraction : MonoBehaviour
{

    public SceneFlowController sceneFlowController;
    public void StartInteraction()
    {
        Debug.Log("Interação iniciada com " + gameObject.name);

        // aqui pode abrir modal, tocar fala, etc.
        // quando terminar, avança para o próximo ponto
        OnInteractionComplete();
    }

    public void OnInteractionComplete()
    {
        sceneFlowController.IrParaProximo();
    }
}