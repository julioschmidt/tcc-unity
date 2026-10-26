using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;

// Script para ser anexado aos botões que receberão interação por gaze
// Interface para objetos que podem receber gaze - DEVE estar no topo
public interface IGazeInputModule
{
    void OnGazeEnter();
    void OnGazeExit();
}
public class GazeButton : MonoBehaviour, IGazeInputModule
{
    [Header("Configurações")]
    public float gazeTime = 2f; // Tempo necessário para ativar o botão
    public Image fillImage; // Imagem que será preenchida durante o gaze

    [Header("Feedback Visual")]
    public Color normalColor = Color.white;
    public Color gazeColor = Color.yellow;
    public Color fillColor = Color.green;

    [Header("Eventos")]
    public UnityEngine.Events.UnityEvent OnGazeClick;
    public UnityEngine.Events.UnityEvent OnGazeEnterEvent;
    public UnityEngine.Events.UnityEvent OnGazeExitEvent;

    private bool isGazing = false;
    private float gazeTimer = 0f;
    private Coroutine gazeCoroutine;
    private Button button;
    private Image buttonImage;

    void Start()
    {
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();

        // Configurar a imagem de preenchimento se não foi definida
        if (fillImage == null)
        {
            // Procurar por uma imagem filha chamada "FillImage"
            Transform fillTransform = transform.Find("FillImage");
            if (fillTransform != null)
            {
                fillImage = fillTransform.GetComponent<Image>();
            }
        }

        // Inicializar o preenchimento como vazio
        if (fillImage != null)
        {
            fillImage.fillAmount = 0f;
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;

            // CORREÇÃO: Tornar invisível inicialmente
            Color invisibleFill = fillColor;
            invisibleFill.a = 0f;
            fillImage.color = invisibleFill;
        }

        // CORREÇÃO: Definir cor inicial do botão principal
        if (buttonImage != null)
        {
            buttonImage.color = normalColor;
        }
    }

    // Chamado quando o ReticlePointer entra no botão
    public void OnGazeEnter()
    {
        isGazing = true;
        gazeTimer = 0f;

        if (gazeCoroutine != null)
            StopCoroutine(gazeCoroutine);

        gazeCoroutine = StartCoroutine(GazeProcess());

        // Feedback visual
        if (buttonImage != null)
        {
            buttonImage.color = gazeColor;
        }

        // CORREÇÃO: Tornar o fillImage visível durante o gaze
        if (fillImage != null)
        {
            Color visibleFill = fillColor;
            visibleFill.a = 0.7f; // Semi-transparente
            fillImage.color = visibleFill;
        }

        // Evento personalizado
        OnGazeEnterEvent.Invoke();

        Debug.Log($"Gaze Enter: {gameObject.name}");
    }

    // Chamado quando o ReticlePointer sai do botão
    public void OnGazeExit()
    {
        isGazing = false;
        gazeTimer = 0f;

        if (gazeCoroutine != null)
        {
            StopCoroutine(gazeCoroutine);
            gazeCoroutine = null;
        }

        // Reset do preenchimento
        if (fillImage != null)
        {
            fillImage.fillAmount = 0f;
            // CORREÇÃO: Tornar invisível novamente
            Color invisibleFill = fillColor;
            invisibleFill.a = 0f;
            fillImage.color = invisibleFill;
        }

        // Reset da cor
        if (buttonImage != null)
        {
            buttonImage.color = normalColor;
        }

        // Evento personalizado
        OnGazeExitEvent.Invoke();

        Debug.Log($"Gaze Exit: {gameObject.name}");
    }

    // Processo de gaze com contagem regressiva
    private IEnumerator GazeProcess()
    {
        while (isGazing && gazeTimer < gazeTime)
        {
            gazeTimer += Time.deltaTime;

            // Atualizar preenchimento visual
            if (fillImage != null)
            {
                fillImage.fillAmount = gazeTimer / gazeTime;
            }

            yield return null;
        }

        // Se completou o tempo de gaze, executar o clique
        if (gazeTimer >= gazeTime && isGazing)
        {
            ExecuteGazeClick();
        }
    }

    private void ExecuteGazeClick()
    {
        Debug.Log($"Gaze Click: {gameObject.name}");

        // Executar o evento do botão
        if (button != null && button.interactable)
        {
            button.onClick.Invoke();
        }

        // Executar eventos personalizados
        OnGazeClick.Invoke();

        // Reset visual
        if (fillImage != null)
        {
            fillImage.fillAmount = 0f;
            // CORREÇÃO: Tornar invisível após o clique
            Color invisibleFill = fillColor;
            invisibleFill.a = 0f;
            fillImage.color = invisibleFill;
        }

        if (buttonImage != null)
        {
            buttonImage.color = normalColor;
        }

        // Parar o gaze após o clique
        isGazing = false;
    }

    // Método público para forçar um clique (útil para testes)
    public void ForceClick()
    {
        ExecuteGazeClick();
    }
}