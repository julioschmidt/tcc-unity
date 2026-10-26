using UnityEngine;

public class CustomGazeInputModule : MonoBehaviour
{
    [Header("Configurações")]
    public LayerMask gazeLayerMask = -1;
    public float maxGazeDistance = 100f;

    private Camera vrCamera;
    private GameObject currentGazeObject;
    private IGazeInputModule currentGazeComponent;

    void Start()
    {
        vrCamera = GetComponent<Camera>();
        if (vrCamera == null)
        {
            vrCamera = Camera.main;
        }
    }

    void Update()
    {
        ProcessGaze();
    }

    private void ProcessGaze()
    {
        RaycastHit hit;
        Ray gazeRay = new Ray(vrCamera.transform.position, vrCamera.transform.forward);

        if (Physics.Raycast(gazeRay, out hit, maxGazeDistance, gazeLayerMask))
        {
            GameObject hitObject = hit.collider.gameObject;
            IGazeInputModule gazeComponent = hitObject.GetComponent<IGazeInputModule>();

            // Se encontrou um novo objeto com gaze
            if (hitObject != currentGazeObject)
            {
                // Sair do objeto anterior
                if (currentGazeComponent != null)
                {
                    currentGazeComponent.OnGazeExit();
                }

                // Entrar no novo objeto
                currentGazeObject = hitObject;
                currentGazeComponent = gazeComponent;

                if (currentGazeComponent != null)
                {
                    currentGazeComponent.OnGazeEnter();
                }
            }
        }
        else
        {
            // Não está olhando para nenhum objeto válido
            if (currentGazeComponent != null)
            {
                currentGazeComponent.OnGazeExit();
                currentGazeObject = null;
                currentGazeComponent = null;
            }
        }
    }

    void OnDrawGizmos()
    {
        if (vrCamera != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(vrCamera.transform.position, vrCamera.transform.forward * maxGazeDistance);
        }
    }
}