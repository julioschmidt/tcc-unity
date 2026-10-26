using UnityEngine;
using UnityEngine.InputSystem; // necessário para o New Input System

public class MotionObjectController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f;
    public Transform cameraTransform;
    public Transform playerTransform;
    public float startDelay = 30f; // tempo em segundos para liberar o movimento

    private Vector3 moveDirection;
    private Vector3 velocity;
    private bool isGrounded;
    private bool canMove = false; // bloqueado no início

    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (controller == null)
        {
            Debug.LogError("CharacterController component is missing!");
        }

        // inicia a contagem para liberar movimento
        StartCoroutine(UnlockMovementAfterDelay(startDelay));
    }

    void Update()
    {
        // se ainda não pode mover, sai do Update
        if (!canMove) return;

        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // Rotaciona junto com a câmera
        float cameraYRotation = cameraTransform.eulerAngles.y;
        Quaternion targetRotation = Quaternion.Euler(0, cameraYRotation, 0);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, 0.1f);

        // --- New Input System ---
        Vector2 input = Vector2.zero;

        if (Gamepad.current != null)
        {
            input = Gamepad.current.leftStick.ReadValue();
        }
        else if (Keyboard.current != null) // fallback no teclado
        {
            input = new Vector2(
                (Keyboard.current.dKey.isPressed ? 1 : 0) - (Keyboard.current.aKey.isPressed ? 1 : 0),
                (Keyboard.current.wKey.isPressed ? 1 : 0) - (Keyboard.current.sKey.isPressed ? 1 : 0)
            );
        }

        float x = input.x;
        float z = input.y;

        moveDirection = transform.right * x + transform.forward * z;
        controller.Move(moveDirection * moveSpeed * Time.deltaTime);

        // Pulo
        if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        else if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Gravidade
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // Atualiza player
        if (playerTransform != null)
        {
            playerTransform.position = transform.position;
        }
    }

    private System.Collections.IEnumerator UnlockMovementAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        canMove = true;
    }
}
