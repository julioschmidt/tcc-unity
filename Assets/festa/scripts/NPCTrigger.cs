using UnityEngine;
using System.Collections;

public class NPCTrigger : MonoBehaviour
{
    [Header("Fluxo do NPC")]
    public int fluxoIndex = 0;
    public float fadeDuration = 2f;

    [Header("Referências")]
    public CustomVRModal vrModal;                 // Modal VR
    public SceneFlowController sceneFlowController; // Controlador do fluxo
    public string npcModalText;                   // Texto do modal
    public float modalDistanceFromPlayer = 2f;
    public float modalHeight = 0; // Altura do modal em relação ao chão (não ao player)
    private Vector3 fixedModalPosition; // Posição fixa do modal quando ativado

    [Header("Áudio do NPC")]
    public AudioClip npcDialogueClip;             // Fala inicial
    public AudioClip confirmDialogueClip;         // Fala se confirmar
    public AudioClip cancelDialogueClip;          // Fala se cancelar
    public Transform npcNarrationSource;          // Fonte do áudio (normalmente o NPC)

    [Header("Player Detection")]
    public Transform playerTransform;             // Player (auto detectado se vazio)
    public string playerTag = "Player";           // Tag do player
    public float triggerDistance = 3f;            // Distância para ativar
    public MotionObjectController motionController;

    [Header("Configuração de NPCs")]
    public Transform[] npcsToRotate;              // NPCs que devem virar para o player
    public float npcRotationSpeed = 2f;           // Velocidade da rotação

    private bool hasTriggered = false;
    private bool modalIsShowing = false;

    void Start()
    {
        // Player automático se não setado
        if (playerTransform == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);
            if (playerObject != null) playerTransform = playerObject.transform;
        }

        // Modal automático se não setado
        if (vrModal == null)
        {
            vrModal = FindFirstObjectByType<CustomVRModal>();
        }

        // Source de áudio padrão = este NPC
        if (npcNarrationSource == null) npcNarrationSource = transform;
    }

    void Update()
    {
        if (hasTriggered || playerTransform == null) return;

        float dist = Vector3.Distance(transform.position, playerTransform.position);
        if (dist <= triggerDistance && PodeAtivarTrigger())
        {
            hasTriggered = true;
            StartCoroutine(TriggerSequence());
        }
    }

    private bool PodeAtivarTrigger()
    {
        if (sceneFlowController == null) return true; // sem controller, sempre ativa

        return sceneFlowController.CurrentIndex == fluxoIndex;
    }

    private IEnumerator TriggerSequence()
    {
        if (motionController != null)
        {
            motionController.enabled = false;
            Debug.Log("Controle do player desabilitado");
        }

        // 1. Fala inicial
        if (npcDialogueClip != null && NarratorManager.Instance != null)
        {
            Debug.Log("NPC falando...");
            NarratorManager.Instance.PlayNarration(npcDialogueClip, npcNarrationSource);
            yield return new WaitForSeconds(npcDialogueClip.length + 0.3f);
        }

        // 2. Mostrar modal
        if (vrModal != null)
        {
            modalIsShowing = true;
            PositionModal();
            vrModal.ShowModal(npcModalText, this);
        }
    }

    public IEnumerator OnModalChoice(bool confirmed)
    {
        Debug.Log($"NPCTrigger: Escolha do modal = {(confirmed ? "CONFIRM" : "CANCEL")}");

        // 3. Resposta do NPC
        AudioClip clipToPlay = confirmed ? confirmDialogueClip : cancelDialogueClip;
        if (clipToPlay != null && NarratorManager.Instance != null)
        {
            NarratorManager.Instance.PlayNarration(clipToPlay, npcNarrationSource);
            yield return new WaitForSeconds(clipToPlay.length + 0.3f);
        }

        if (motionController != null)
        {
            motionController.enabled = true;
            Debug.Log("Controle do player reativado após escolha do modal");
        }

        // 4. Avançar no fluxo
        if (sceneFlowController != null)
        {
            sceneFlowController.IrParaProximo(fadeDuration);
        }

        modalIsShowing = false;
    }

    private void PositionModal()
    {
        if (vrModal == null || playerTransform == null) return;

        Vector3 playerPos = playerTransform.position;
        Vector3 npcPos = transform.position;

        // Lógica de posicionamento inteligente
        Vector3 playerToNpc = (npcPos - playerPos).normalized;

        Vector3 extraDistance = Vector3.one;
        Vector3 leftPosition = playerPos + Vector3.left * (modalDistanceFromPlayer);
        Vector3 rightPosition = playerPos + Vector3.right * (modalDistanceFromPlayer);
        Vector3 frontPosition = playerPos + Vector3.forward * (modalDistanceFromPlayer);
        Vector3 backPosition = playerPos + Vector3.back * (modalDistanceFromPlayer);

        Vector3 chosenPosition;

        if (playerToNpc.z < -0.5f) chosenPosition = backPosition;
        else if (playerToNpc.x < -0.5f) chosenPosition = leftPosition;
        else if (playerToNpc.z > 0.5f) chosenPosition = frontPosition;
        else chosenPosition = rightPosition;

        fixedModalPosition = chosenPosition;
        fixedModalPosition.y = playerPos.y;

        // Setar posição
        vrModal.transform.position = fixedModalPosition;

        // ROTAÇÃO CORRIGIDA: manter apenas eixo Y, frente real é -Z
        Vector3 lookDir = playerPos - vrModal.transform.position;
        lookDir.y = 0; // trava eixo Y

        if (lookDir != Vector3.zero)
        {
            vrModal.transform.rotation = Quaternion.LookRotation(-lookDir, Vector3.up);
        }

        Debug.Log($"Modal positioned at: {vrModal.transform.position}, looking at player");
    }

    void LateUpdate()
    {
        if (modalIsShowing && vrModal != null && playerTransform != null)
        {
            // Mantém o modal sempre voltado para o player
            Vector3 lookDir = playerTransform.position - vrModal.transform.position;
            lookDir.y = 0;
            if (lookDir != Vector3.zero)
                vrModal.transform.rotation = Quaternion.LookRotation(-lookDir, Vector3.up);
        }

        RotateNPCsToLookAtPlayer();
    }

    private void RotateNPCsToLookAtPlayer()
    {
        if (playerTransform == null || npcsToRotate == null) return;

        foreach (Transform npc in npcsToRotate)
        {
            Vector3 direction = (playerTransform.position - npc.position).normalized;
            direction.y = 0;

            if (direction.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                npc.rotation = Quaternion.Slerp(npc.rotation, targetRotation, Time.deltaTime * npcRotationSpeed);
            }
        }
    }
}