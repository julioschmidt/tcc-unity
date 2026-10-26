using UnityEngine;
using System.Collections;

// Script para anexar ao NPC que vai mostrar o modal quando o player se aproximar
public class NPCModalTrigger : MonoBehaviour
{
    [Header("Configurações de Proximidade")]
    public float triggerDistance = 3f; // Distância para ativar o modal
    public float exitDistance = 5f; // Distância para fechar o modal (maior que triggerDistance)

    [Header("Configurações do Modal")]
    public CustomVRModal vrModal; // Referência ao modal
    public Transform modalSpawnPoint; // Ponto onde o modal vai aparecer (opcional)
    public string npcModalText;

    [Header("Posicionamento do Modal")]
    public float modalDistanceFromPlayer = 2f; // Distância do modal em relação ao player
    public float modalHeight = 1.5f; // Altura do modal em relação ao chão (não ao player)
    public bool lookAtPlayer = true; // Se o modal deve sempre olhar para o player
    public bool followPlayer = false; // Se o modal deve seguir o player (não recomendado)

    [Header("Player Detection")]
    public Transform playerTransform; // Referência ao player (será encontrada automaticamente se não definida)
    public string playerTag = "Player"; // Tag do player para busca automática
    public MotionObjectController motionController; // Referência direta ao controlador de movimento (GameObject separado)

    [Header("Feedback Visual")]
    public GameObject interactionIndicator; // Indicador visual quando o player está próximo
    public bool useDistanceBasedIndicator = true;

    [Header("Movimentação Automática do Player")]
    public Transform playerTargetPosition; // Ponto para onde o player deve ir (em frente ao NPC)
    public float moveSpeed = 3f; // Velocidade de movimentação automática
    public float arrivalThreshold = 0.5f; // Distância mínima para considerar que chegou ao destino
    public float autoMoveDistance = 2f; // Distância padrão do NPC se não houver targetPosition definido
    public float maxMoveTime = 5f;

    [Header("Narração")]
    public AudioClip npcDialogueClip; // Áudio que o NPC vai falar
    public Transform npcNarrationSource; // Source do áudio (opcional, usa o próprio NPC se não definido)

    [Header("NPCs que Olharão para o Player")]
    public Transform[] npcsToLookAtPlayer; // Lista de NPCs que devem se virar para o player
    public float npcRotationSpeed = 2f; // Velocidade de rotação dos NPCs
    public bool rotateNPCsSimultaneously = true; // Se os NPCs devem rotacionar ao mesmo tempo ou em sequência
    public float npcRotationDelay = 0.2f; // Delay entre as rotações (se não for simultâneo)

    [Header("Narração Resposta do Modal")]
    public AudioClip confirmDialogueClip;
    public AudioClip cancelDialogueClip;

    [Header("Debug")]
    public bool showDebugGizmos = true;

    private bool playerInRange = false;
    private bool modalIsShowing = false;
    private bool isMovingPlayer = false; // Flag para controlar se o player está sendo movimentado automaticamente
    private bool hasTriggeredSequence = false; // Evita múltiplas execuções da sequência

    private Camera playerCamera;
    private Vector3 fixedModalPosition; // Posição fixa do modal quando ativado
    private MotionObjectController playerController; // Referência ao controlador do player
    private Vector3 originalPlayerPosition; // Posição original do player antes da movimentação automática

    void Start()
    {
        // Encontrar o player automaticamente se não foi definido
        if (playerTransform == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);
            if (playerObject != null)
            {
                playerTransform = playerObject.transform;
            }
            else
            {
                // Tentar encontrar pela câmera principal
                playerCamera = Camera.main;
                if (playerCamera != null)
                {
                    playerTransform = playerCamera.transform;
                }
            }
        }

        // Encontrar o controlador do player
        if (motionController == null)
        {
            // Tentar encontrar o MotionObjectController na cena
            motionController = FindFirstObjectByType<MotionObjectController>();

            // Se ainda não encontrou, tentar buscar pelo player
            if (motionController == null && playerTransform != null)
            {
                motionController = playerTransform.GetComponent<MotionObjectController>();
                if (motionController == null)
                {
                    motionController = playerTransform.GetComponentInParent<MotionObjectController>();
                }
                if (motionController == null)
                {
                    motionController = playerTransform.GetComponentInChildren<MotionObjectController>();
                }
            }
        }

        // Usar a referência do motionController que foi configurada
        playerController = motionController;

        // Configurar referência da câmera
        if (playerCamera == null && playerTransform != null)
        {
            playerCamera = playerTransform.GetComponent<Camera>();
            if (playerCamera == null)
            {
                playerCamera = playerTransform.GetComponentInChildren<Camera>();
            }
        }

        // Encontrar o modal automaticamente se não foi definido
        if (vrModal == null)
        {
            vrModal = FindFirstObjectByType<CustomVRModal>();
        }

        // Configurar indicador inicial
        if (interactionIndicator != null)
        {
            interactionIndicator.SetActive(false);
        }

        // Configurar posição alvo padrão se não foi definida
        if (playerTargetPosition == null)
        {
            // Criar um ponto em frente ao NPC
            GameObject targetGO = new GameObject($"{gameObject.name}_PlayerTarget");
            targetGO.transform.SetParent(transform);
            targetGO.transform.position = transform.position + transform.forward * autoMoveDistance;
            playerTargetPosition = targetGO.transform;
        }

        // Configurar source de narração
        if (npcNarrationSource == null)
        {
            npcNarrationSource = transform; // Usa o próprio NPC como source
        }

        // Validações
        if (playerTransform == null)
        {
            Debug.LogWarning($"NPCModalTrigger em {gameObject.name}: Player não encontrado! Defina o playerTransform ou use a tag '{playerTag}'");
        }
        if (vrModal == null)
        {
            Debug.LogWarning($"NPCModalTrigger em {gameObject.name}: CustomVRModal não encontrado!");
        }
        if (playerController == null)
        {
            Debug.LogWarning($"NPCModalTrigger em {gameObject.name}: MotionObjectController não encontrado! Por favor, arraste o GameObject com o MotionObjectController para o campo 'Motion Controller'.");
        }
    }

    void Update()
    {
        if (playerTransform == null || vrModal == null) return;

        CheckPlayerProximity();
        UpdateInteractionIndicator();

        // Atualizar rotação do modal se estiver ativo e configurado para olhar para o player
        if (modalIsShowing && lookAtPlayer)
        {
            UpdateModalRotation();
        }
    }

    private void CheckPlayerProximity()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // Player entrou na área de trigger
        if (!playerInRange && distanceToPlayer <= triggerDistance && !hasTriggeredSequence)
        {
            OnPlayerEnterRange();
        }
        // Player saiu da área de trigger
        else if (playerInRange && distanceToPlayer > exitDistance && !isMovingPlayer)
        {
            OnPlayerExitRange();
        }
    }

    private void OnPlayerEnterRange()
    {
        playerInRange = true;
        hasTriggeredSequence = true; // Evita múltiplas execuções

        // Iniciar a sequência completa: mover player → narração → modal
        StartCoroutine(ExecuteFullSequence());

        Debug.Log($"Player entrou na área do NPC: {gameObject.name}");
    }

    private void OnPlayerExitRange()
    {
        playerInRange = false;
        HideModal();
        ResetSequence();
        Debug.Log($"Player saiu da área do NPC: {gameObject.name}");
    }

    private IEnumerator ExecuteFullSequence()
    {
        // 1. Desabilitar controle do player
        DisablePlayerControl();

        // 2. Mover player até o ponto alvo
        yield return StartCoroutine(MovePlayerToTarget());

        Debug.Log($"Player chegou no local pré-determinado da área do NPC");

        // 3. Fazer player olhar para o NPC
        //yield return StartCoroutine(RotatePlayerToLookAtNPC());

        // 4. Fazer NPCs olharem para o player
        yield return StartCoroutine(RotateNPCsToLookAtPlayer());

        // 5. Tocar narração do NPC
        if (npcDialogueClip != null)
        {
            PlayNPCNarration();

            // Aguardar a narração terminar
            yield return new WaitForSeconds(npcDialogueClip.length);

            // Pequena pausa adicional
            yield return new WaitForSeconds(0.5f);
        }

        // 6. Mostrar o modal
        ShowModal();

        // 7. Reabilitar controle do player
        // EnablePlayerControl();
    }

    private IEnumerator MovePlayerToTarget()
    {
        if (playerTargetPosition == null) yield break;

        isMovingPlayer = true;

        Transform moveTarget = (playerController != null) ? playerController.transform : playerTransform;
        originalPlayerPosition = moveTarget.position;
        Vector3 targetPos = playerTargetPosition.position;

        Debug.Log($"Movendo player para posição: {targetPos}");

        float elapsedTime = 0f;

        while (Vector3.Distance(moveTarget.position, targetPos) > arrivalThreshold && elapsedTime < maxMoveTime)
        {
            Vector3 direction = (targetPos - moveTarget.position).normalized;
            Vector3 newPosition = moveTarget.position + direction * moveSpeed * Time.deltaTime;

            moveTarget.position = newPosition;

            if (moveTarget != playerTransform && playerTransform != null)
            {
                playerTransform.position = newPosition;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Se estourou o tempo, forçar posição final
        if (elapsedTime >= maxMoveTime)
        {
            Debug.LogWarning("Timeout atingido em MovePlayerToTarget — forçando chegada!");
            moveTarget.position = targetPos;
            if (moveTarget != playerTransform && playerTransform != null)
            {
                playerTransform.position = targetPos;
            }
        }

        isMovingPlayer = false;
        Debug.Log("Player chegou (ou timeout forçado)!");
    }

    private IEnumerator RotateNPCsToLookAtPlayer()
    {
        if (npcsToLookAtPlayer == null || npcsToLookAtPlayer.Length == 0)
        {
            yield break;
        }

        // Usar a posição do MotionObjectController se disponível, senão usar playerTransform
        Transform playerTarget = (playerController != null) ? playerController.transform : playerTransform;

        if (playerTarget == null)
        {
            Debug.LogWarning("Não foi possível encontrar a posição do player para rotacionar os NPCs");
            yield break;
        }

        Vector3 playerPosition = playerTarget.position;
        Debug.Log($"Fazendo {npcsToLookAtPlayer.Length} NPCs olharem para o player");

        if (rotateNPCsSimultaneously)
        {
            // Rotacionar todos os NPCs ao mesmo tempo
            yield return StartCoroutine(RotateMultipleNPCsSimultaneously(playerPosition));
        }
        else
        {
            // Rotacionar NPCs em sequência
            for (int i = 0; i < npcsToLookAtPlayer.Length; i++)
            {
                if (npcsToLookAtPlayer[i] != null)
                {
                    yield return StartCoroutine(RotateSingleNPC(npcsToLookAtPlayer[i], playerPosition));

                    // Delay entre as rotações (exceto no último)
                    if (i < npcsToLookAtPlayer.Length - 1)
                    {
                        yield return new WaitForSeconds(npcRotationDelay);
                    }
                }
            }
        }

        Debug.Log("Todos os NPCs rotacionaram para olhar o player!");
    }

    private IEnumerator RotateMultipleNPCsSimultaneously(Vector3 playerPosition)
    {
        // Calcular rotações alvo para todos os NPCs
        Quaternion[] targetRotations = new Quaternion[npcsToLookAtPlayer.Length];
        bool[] shouldRotate = new bool[npcsToLookAtPlayer.Length];

        for (int i = 0; i < npcsToLookAtPlayer.Length; i++)
        {
            if (npcsToLookAtPlayer[i] != null)
            {
                Vector3 directionToPlayer = (playerPosition - npcsToLookAtPlayer[i].position).normalized;
                directionToPlayer.y = 0; // Manter apenas rotação horizontal

                if (directionToPlayer != Vector3.zero)
                {
                    targetRotations[i] = Quaternion.LookRotation(directionToPlayer);
                    shouldRotate[i] = true;
                }
            }
        }

        // Rotacionar todos simultaneamente
        bool anyStillRotating = true;
        while (anyStillRotating)
        {
            anyStillRotating = false;

            for (int i = 0; i < npcsToLookAtPlayer.Length; i++)
            {
                if (shouldRotate[i] && npcsToLookAtPlayer[i] != null)
                {
                    float angle = Quaternion.Angle(npcsToLookAtPlayer[i].rotation, targetRotations[i]);

                    if (angle > 5f)
                    {
                        npcsToLookAtPlayer[i].rotation = Quaternion.Lerp(
                            npcsToLookAtPlayer[i].rotation,
                            targetRotations[i],
                            npcRotationSpeed * Time.deltaTime
                        );
                        anyStillRotating = true;
                    }
                    else
                    {
                        // Finalizar rotação
                        npcsToLookAtPlayer[i].rotation = targetRotations[i];
                        shouldRotate[i] = false;
                    }
                }
            }

            yield return null;
        }
    }

    private IEnumerator RotateSingleNPC(Transform npc, Vector3 playerPosition)
    {
        if (npc == null) yield break;

        Vector3 directionToPlayer = (playerPosition - npc.position).normalized;
        directionToPlayer.y = 0; // Manter apenas rotação horizontal

        if (directionToPlayer != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);

            while (Quaternion.Angle(npc.rotation, targetRotation) > 5f)
            {
                npc.rotation = Quaternion.Lerp(npc.rotation, targetRotation, npcRotationSpeed * Time.deltaTime);
                yield return null;
            }

            npc.rotation = targetRotation;
            Debug.Log($"NPC {npc.name} rotacionado para olhar o player");
        }
    }

    private void PlayNPCNarration()
    {
        if (npcDialogueClip != null && NarratorManager.Instance != null)
        {
            NarratorManager.Instance.PlayNarration(npcDialogueClip, npcNarrationSource);
            Debug.Log("NPC falando...");
        }
    }

    private void DisablePlayerControl()
    {
        if (playerController != null)
        {
            playerController.enabled = false;
            Debug.Log("Controle do player desabilitado");
        }
    }

    private void ShowModal()
    {
        if (vrModal != null && !modalIsShowing)
        {
            modalIsShowing = true;
            PositionModal();
            vrModal.ShowModal(npcModalText, this);
        }
    }

    private void HideModal()
    {
        if (vrModal != null && modalIsShowing)
        {
            modalIsShowing = false;
            vrModal.HideModal();
        }
    }

    private void ResetSequence()
    {
        hasTriggeredSequence = false;
        isMovingPlayer = false;
        //EnablePlayerControl(); // Garantir que o controle esteja habilitado
    }

    private void PositionModal()
    {
        if (vrModal == null || playerTransform == null) return;

        Vector3 playerPos = playerTransform.position;
        Debug.Log("=== MODAL POSITIONING DEBUG ===");
        Debug.Log($"Player Position: {playerPos}");
        Debug.Log($"NPC Position: {transform.position}");

        // Usar ponto de spawn específico se definido
        if (modalSpawnPoint != null)
        {
            fixedModalPosition = modalSpawnPoint.position;
            Debug.Log($"Using spawn point: {fixedModalPosition}");
        }
        else
        {
            // LÓGICA INTELIGENTE: Escolher a direção baseada na posição relativa player-NPC
            Vector3 npcPos = transform.position;
            Vector3 playerToNpc = (npcPos - playerPos).normalized;

            // Calcular as 4 direções com acréscimo
            Vector3 extraDistance = Vector3.one * 200f; // Acréscimo extra
            Vector3 leftPosition = playerPos + (Vector3.left * modalDistanceFromPlayer) + (Vector3.left * extraDistance.x);
            Vector3 rightPosition = playerPos + (Vector3.right * modalDistanceFromPlayer) + (Vector3.right * extraDistance.x);
            Vector3 frontPosition = playerPos + (Vector3.forward * modalDistanceFromPlayer) + (Vector3.forward * extraDistance.z);
            Vector3 backPosition = playerPos + (Vector3.back * modalDistanceFromPlayer) + (Vector3.back * extraDistance.z);

            // ESCOLHA COM ROTAÇÃO HORÁRIA: Sul→Oeste, Oeste→Norte, Norte→Leste, Leste→Sul
            Vector3 chosenPosition;
            string chosenDirection;

            // Se o NPC está mais ao SUL (atrás/baixo), modal vai para OESTE (esquerda)
            if (playerToNpc.z < -0.5f)
            {
                chosenPosition = backPosition;
                chosenDirection = "OESTE (NPC ao sul)";
            }
            // Se o NPC está mais ao OESTE (esquerda), modal vai para NORTE (frente)
            else if (playerToNpc.x < -0.5f)
            {
                chosenPosition = leftPosition;
                chosenDirection = "NORTE (NPC ao oeste)";
            }
            // Se o NPC está mais ao NORTE (frente/cima), modal vai para LESTE (direita)
            else if (playerToNpc.z > 0.5f)
            {
                chosenPosition = frontPosition;
                chosenDirection = "LESTE (NPC ao norte)";
            }
            // Se o NPC está mais ao LESTE (direita), modal vai para SUL (atrás)
            else
            {
                chosenPosition = rightPosition;
                chosenDirection = "SUL (NPC ao leste)";
            }

            fixedModalPosition = chosenPosition;
            fixedModalPosition.y = playerPos.y + modalHeight;

            Debug.Log($"=== POSICIONAMENTO INTELIGENTE ===");
            Debug.Log($"Player: {playerPos}, NPC: {npcPos}");
            Debug.Log($"Direção Player->NPC: {playerToNpc}");
            Debug.Log($"Posição escolhida: {chosenDirection}");
            Debug.Log($"Modal em: {fixedModalPosition}");
        }

        // FORÇAR a posição do modal
        vrModal.transform.position = fixedModalPosition;
        Debug.Log($"Modal positioned at: {vrModal.transform.position}");
        Debug.Log($"Distance from player: {Vector3.Distance(playerPos, vrModal.transform.position)}");

        // ROTAÇÃO SIMPLES - fazer o modal sempre olhar para o player
        Vector3 lookDirection = (playerPos - vrModal.transform.position).normalized;
        if (lookDirection != Vector3.zero)
        {
            vrModal.transform.LookAt(playerPos);
            Debug.Log($"Modal rotation: {vrModal.transform.rotation.eulerAngles}");
        }
    }

    private void UpdateModalRotation()
    {
        if (vrModal == null || playerCamera == null) return;

        // Fazer o modal sempre olhar para o player
        Vector3 directionToPlayer = (playerCamera.transform.position - vrModal.transform.position).normalized;

        // Manter apenas rotação no eixo Y (horizontal)
        directionToPlayer.y = 0f;
        directionToPlayer = directionToPlayer.normalized;

        if (directionToPlayer != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
            // Adicionar 180 graus no eixo Y para corrigir a orientação
            vrModal.transform.rotation = lookRotation * Quaternion.Euler(0, 180, 0);
        }
    }

    private void UpdateInteractionIndicator()
    {
        if (interactionIndicator == null) return;

        if (useDistanceBasedIndicator && playerTransform != null)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            bool shouldShow = distance <= triggerDistance * 1.2f && !hasTriggeredSequence; // Mostrar um pouco antes do trigger
            interactionIndicator.SetActive(shouldShow);
        }
        else
        {
            interactionIndicator.SetActive(playerInRange && !hasTriggeredSequence);
        }
    }

    // Métodos públicos para controle externo
    public void ForceShowModal()
    {
        ShowModal();
    }

    public void ForceHideModal()
    {
        HideModal();
    }

    public void SetTriggerDistance(float newDistance)
    {
        triggerDistance = newDistance;
        exitDistance = Mathf.Max(exitDistance, newDistance + 1f);
    }

    public void ResetTrigger()
    {
        ResetSequence();
        HideModal();
    }

    // Métodos públicos para controlar NPCs
    public void AddNPCToLookAtPlayer(Transform npc)
    {
        if (npc == null) return;

        // Verificar se já existe na lista
        for (int i = 0; i < npcsToLookAtPlayer.Length; i++)
        {
            if (npcsToLookAtPlayer[i] == npc) return;
        }

        // Adicionar à lista
        Transform[] newArray = new Transform[npcsToLookAtPlayer.Length + 1];
        for (int i = 0; i < npcsToLookAtPlayer.Length; i++)
        {
            newArray[i] = npcsToLookAtPlayer[i];
        }
        newArray[npcsToLookAtPlayer.Length] = npc;
        npcsToLookAtPlayer = newArray;
    }

    public void RemoveNPCFromLookAtPlayer(Transform npc)
    {
        if (npc == null || npcsToLookAtPlayer.Length == 0) return;

        // Encontrar e remover o NPC
        Transform[] newArray = new Transform[npcsToLookAtPlayer.Length - 1];
        int newIndex = 0;

        for (int i = 0; i < npcsToLookAtPlayer.Length; i++)
        {
            if (npcsToLookAtPlayer[i] != npc)
            {
                if (newIndex < newArray.Length)
                {
                    newArray[newIndex] = npcsToLookAtPlayer[i];
                    newIndex++;
                }
            }
        }

        npcsToLookAtPlayer = newArray;
    }

    public void ForceNPCsLookAtPlayer()
    {
        if (playerTransform != null || playerController != null)
        {
            StartCoroutine(RotateNPCsToLookAtPlayer());
        }
    }

    public void OnModalChoice(bool confirmed)
    {
        Debug.Log("CHEGOU NO ONMODALCHOICE");
        AudioClip clipToPlay = confirmed ? confirmDialogueClip : cancelDialogueClip;

        if (clipToPlay != null && NarratorManager.Instance != null)
        {
            NarratorManager.Instance.PlayNarration(clipToPlay, npcNarrationSource);
            Debug.Log($"NPC respondeu com áudio: {(confirmed ? "CONFIRM" : "CANCEL")}");
        }

        // Aqui tu libera o player normalmente
        if (playerController != null)
        {
            playerController.enabled = true;
            Debug.Log("Controle do player reativado após escolha do modal");
        }
    }

    // Gizmos para visualização no editor
    void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;

        // Área de trigger
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, triggerDistance);

        // Área de saída
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, exitDistance);

        // Ponto de spawn do modal
        if (modalSpawnPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(modalSpawnPoint.position, Vector3.one * 0.3f);
            Gizmos.DrawLine(transform.position, modalSpawnPoint.position);
        }

        // Posição alvo do player
        if (playerTargetPosition != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(playerTargetPosition.position, Vector3.one * 0.4f);
            Gizmos.DrawLine(transform.position, playerTargetPosition.position);

            // Seta indicando a direção que o player vai olhar
            Gizmos.color = Color.cyan;
            Vector3 lookDirection = (transform.position - playerTargetPosition.position).normalized;
            Gizmos.DrawRay(playerTargetPosition.position, lookDirection * 1f);
        }

        // NPCs que vão olhar para o player
        if (npcsToLookAtPlayer != null && npcsToLookAtPlayer.Length > 0)
        {
            Gizmos.color = Color.magenta;
            for (int i = 0; i < npcsToLookAtPlayer.Length; i++)
            {
                if (npcsToLookAtPlayer[i] != null)
                {
                    // Desenhar cubo no NPC
                    Gizmos.DrawWireCube(npcsToLookAtPlayer[i].position, Vector3.one * 0.3f);

                    // Linha conectando o trigger ao NPC
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawLine(transform.position, npcsToLookAtPlayer[i].position);

                    // Se o player estiver definido, mostrar linha do NPC para o player
                    if (playerTransform != null)
                    {
                        Gizmos.color = new Color(1f, 0f, 1f, 0.3f); // Magenta transparente
                        Gizmos.DrawLine(npcsToLookAtPlayer[i].position, playerTransform.position);
                        Gizmos.DrawLine(npcsToLookAtPlayer[i].position, playerTransform.position);

                        // Seta indicando direção que o NPC vai olhar
                        Vector3 npcLookDirection = (playerTransform.position - npcsToLookAtPlayer[i].position).normalized;
                        npcLookDirection.y = 0;
                        Gizmos.color = Color.magenta;
                        Gizmos.DrawRay(npcsToLookAtPlayer[i].position + Vector3.up * 0.1f, npcLookDirection * 1.5f);
                    }
                }
            }
        }

        // Linha para o player se estiver definido
        if (playerTransform != null)
        {
            Gizmos.color = playerInRange ? Color.yellow : Color.gray;
            Gizmos.DrawLine(transform.position, playerTransform.position);
        }

        // Mostrar posição fixa do modal quando ativo
        if (modalIsShowing)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(fixedModalPosition, Vector3.one * 0.5f);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos) return;

        // Mostrar preview da posição do modal
        if (playerTransform != null)
        {
            Vector3 previewPosition;
            if (modalSpawnPoint != null)
            {
                previewPosition = modalSpawnPoint.position;
            }
            else
            {
                Vector3 playerPosition = playerTransform.position;
                // Usar a direção da câmera para preview
                Vector3 cameraForward;
                if (playerCamera != null)
                {
                    cameraForward = playerCamera.transform.forward;
                }
                else
                {
                    cameraForward = playerTransform.forward;
                }

                Vector3 modalDirection = cameraForward;
                modalDirection.y = 0f;
                modalDirection = modalDirection.normalized;

                previewPosition = playerPosition + (modalDirection * modalDistanceFromPlayer);
                previewPosition.y = playerPosition.y + modalHeight;
            }

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(previewPosition, Vector3.one * 0.5f);
            Gizmos.DrawLine(playerTransform.position, previewPosition);
        }
    }
}