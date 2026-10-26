using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

// Script para o Modal VR - anexar ao GameObject do modal
public class CustomVRModal : MonoBehaviour
{
    [Header("Botões do Modal")]
    public GazeButton confirmButton;
    public GazeButton cancelButton;

    [Header("Integração com CSV")]
    public CSVHandler csvHandler;

    [Header("Configurações")]
    public GameObject modalPanel;
    public CanvasGroup canvasGroup; // Para animações de fade
    public TextMeshProUGUI modalText;

    [Header("Animações")]
    public bool useAnimations = true;
    public float animationDuration = 0.3f;

    [Header("Controle de Movimento")]
    public bool freezePlayerOnShow = true; // Se deve travar o player quando o modal aparecer
    public Transform playerTransform; // Referência manual (opcional)
    public string playerTag = "Player"; // Tag para encontrar o player automaticamente
    public MotionObjectController playerController;

    [Header("Debug")]
    public bool showDebugLogs = true;

    private bool isVisible = false;

    // Componentes do player que podem controlar movimento
    private CharacterController playerCharacterController;
    private Rigidbody playerRigidbody;
    private MonoBehaviour[] playerMovementScripts;
    private MonoBehaviour currentNpc;

    // Estados salvos para restaurar depois
    private bool wasCharacterControllerEnabled;
    private bool wasRigidbodyKinematic;
    private RigidbodyConstraints originalRigidbodyConstraints;
    private bool[] movementScriptsWereEnabled;

    void Start()
    {
        // Configurar eventos dos botões
        if (confirmButton != null)
        {
            confirmButton.OnGazeClick.AddListener(OnConfirm);
        }
        if (cancelButton != null)
        {
            cancelButton.OnGazeClick.AddListener(OnCancel);
        }

        // Configurar canvas group se não foi definido
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        // Encontrar o player automaticamente se não foi definido
        FindPlayerComponents();

        // Inicializar como oculto
        HideModal();
    }

    private void FindPlayerComponents()
    {
        // Tentar encontrar o player se não foi definido manualmente
        if (playerTransform == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);
            if (playerObject != null)
            {
                playerTransform = playerObject.transform;
            }
            else
            {
                // Tentar pela câmera principal como fallback
                Camera mainCamera = Camera.main;
                if (mainCamera != null)
                {
                    playerTransform = mainCamera.transform;
                }
            }
        }

        if (playerTransform != null)
        {
            // Encontrar componentes de movimento no player
            playerCharacterController = playerTransform.GetComponent<CharacterController>();
            playerRigidbody = playerTransform.GetComponent<Rigidbody>();

            // Encontrar scripts de movimento comuns (adicione mais conforme necessário)
            playerMovementScripts = playerTransform.GetComponents<MonoBehaviour>();

            if (showDebugLogs)
            {
                Debug.Log($"[CustomVRModal] Player encontrado: {playerTransform.name}");
                Debug.Log($"[CustomVRModal] CharacterController: {(playerCharacterController != null ? "Sim" : "Não")}");
                Debug.Log($"[CustomVRModal] Rigidbody: {(playerRigidbody != null ? "Sim" : "Não")}");
                Debug.Log($"[CustomVRModal] Scripts encontrados: {playerMovementScripts.Length}");
            }
        }
        else if (showDebugLogs)
        {
            Debug.LogWarning($"[CustomVRModal] Player não encontrado! Use a tag '{playerTag}' ou defina playerTransform manualmente.");
        }
    }

    public void ShowModal(string text, MonoBehaviour npc)
    {
        // pode ser NPCTrigger ou NPCModalTrigger
        currentNpc = npc;

        if (isVisible) return;
        isVisible = true;

        if (modalPanel != null)
            modalPanel.SetActive(true);

        if (modalText != null)
            modalText.text = text;

        if (useAnimations && canvasGroup != null)
        {
            StartCoroutine(FadeIn());
        }
        else if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        if (showDebugLogs)
            Debug.Log("[CustomVRModal] Modal VR mostrado - Player travado. Texto: " + text);
    }

    public void HideModal()
    {
        if (!isVisible) return;

        isVisible = false;

        if (useAnimations && canvasGroup != null)
        {
            StartCoroutine(FadeOut());
        }
        else
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
            if (modalPanel != null)
            {
                modalPanel.SetActive(false);
            }
        }
        currentNpc = null;

        if (showDebugLogs)
        {
            Debug.Log("[CustomVRModal] Modal VR ocultado - Player destravado");
        }
    }

    //private void UnfreezePlayer()
    //{
    //    if (playerController != null)
    //    {
    //        playerController.enabled = true;
    //        Debug.Log("Controle do player reabilitado");
    //    }
    //}
    private IEnumerator FadeIn()
    {
        float elapsed = 0f;
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = true;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / animationDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
    }

    private IEnumerator FadeOut()
    {
        float elapsed = 0f;
        canvasGroup.interactable = false;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / animationDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;

        if (modalPanel != null)
        {
            modalPanel.SetActive(false);
        }
    }

    private void OnConfirm()
    {
        if (showDebugLogs)
            Debug.Log("[CustomVRModal] Botão Confirmar ativado por gaze!");

        if (csvHandler != null)
            csvHandler.SaveChoiceToCSV("Aceitou");

        if (currentNpc != null)
        {
            if (currentNpc is NPCTrigger npcTrigger)
            {
                // Iniciar coroutine para esperar o áudio
                StartCoroutine(npcTrigger.OnModalChoice(true));
            }
            else if (currentNpc is NPCModalTrigger npcModalTrigger)
            {
                npcModalTrigger.OnModalChoice(true);
            }
        }

        HideModal();
    }

    private void OnCancel()
    {
        if (showDebugLogs)
            Debug.Log("[CustomVRModal] Botão Cancelar ativado por gaze!");

        if (csvHandler != null)
            csvHandler.SaveChoiceToCSV("Negou");

        if (currentNpc != null)
        {
            if (currentNpc is NPCTrigger npcTrigger)
            {
                StartCoroutine(npcTrigger.OnModalChoice(false));
            }
            else if (currentNpc is NPCModalTrigger npcModalTrigger)
            {
                npcModalTrigger.OnModalChoice(false);
            }
        }

        HideModal();
    }
    // Método para forçar desbloqueio em caso de emergência
    //public void ForceUnfreezePlayer()
    //{
    //    if (freezePlayerOnShow)
    //    {
    //        UnfreezePlayer();
    //    }
    //}

    // Garantir que o player seja destravado se o script for destruído
    //void OnDestroy()
    //{
    //    if (isVisible && freezePlayerOnShow)
    //    {
    //        UnfreezePlayer();
    //    }
    //}
}