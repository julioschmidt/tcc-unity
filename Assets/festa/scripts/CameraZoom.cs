using UnityEngine;
using System.Collections;

public class CameraZoom : MonoBehaviour
{
    public static CameraZoom Instance { get; private set; }

    [Header("Referências")]
    public Transform playerTransform;   // o objeto Player que é pai da câmera
    public Camera cam;                  // normalmente MainCamera

    [Header("Zoom Settings")]
    public float zoomDistance = 1f;     // distância que o Player se aproxima do alvo
    public float zoomDuration = 1f;     // tempo para ir/voltar
    public float holdTime = 2f;         // tempo segurando o zoom

    private Vector3 originalPlayerPosition;
    private Quaternion originalPlayerRotation;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (cam == null) cam = Camera.main;
        if (playerTransform == null && cam != null)
            playerTransform = cam.transform.parent;

        originalPlayerPosition = playerTransform.position;
        originalPlayerRotation = playerTransform.rotation;
    }

    /// <summary>
    /// Faz o Player se aproximar de uma posição (zoom físico) e depois volta.
    /// </summary>
    public void ZoomTo(Vector3 targetPosition)
    {
        StopAllCoroutines();
        StartCoroutine(DoZoom(targetPosition));
    }

    private IEnumerator DoZoom(Vector3 targetPosition)
    {
        originalPlayerPosition = playerTransform.position;
        originalPlayerRotation = playerTransform.rotation;

        // Calcula direção para o alvo
        Vector3 direction = (targetPosition - originalPlayerPosition).normalized;
        Vector3 zoomPosition = originalPlayerPosition + direction * zoomDistance;

        // Calcula rotação para olhar para o alvo
        Quaternion targetRotation = Quaternion.LookRotation(targetPosition - zoomPosition);

        // ---- Ir para zoom ----
        float t = 0f;
        while (t < zoomDuration)
        {
            t += Time.deltaTime;
            float lerp = t / zoomDuration;

            playerTransform.position = Vector3.Lerp(originalPlayerPosition, zoomPosition, lerp);
            playerTransform.rotation = Quaternion.Slerp(originalPlayerRotation, targetRotation, lerp);

            yield return null;
        }

        // Segura o zoom
        yield return new WaitForSeconds(holdTime);

        // ---- Voltar ----
        t = 0f;
        while (t < zoomDuration)
        {
            t += Time.deltaTime;
            float lerp = t / zoomDuration;

            playerTransform.position = Vector3.Lerp(zoomPosition, originalPlayerPosition, lerp);
            playerTransform.rotation = Quaternion.Slerp(targetRotation, originalPlayerRotation, lerp);

            yield return null;
        }

        // Garantir valores finais
        playerTransform.position = originalPlayerPosition;
        playerTransform.rotation = originalPlayerRotation;
    }
}
