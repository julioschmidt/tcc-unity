using UnityEngine;

public class AnimationYOffsetSmooth : MonoBehaviour
{
    public Animator animator;
    public string problematicState = "IdleLow"; // nome da animação que precisa do ajuste
    public float yOffset = -0.2f; // quanto abaixar
    public float smoothSpeed = 2f; // velocidade da interpolação

    private float originalY;
    private float targetY;

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        originalY = transform.position.y;
        targetY = originalY;
    }

    void Update()
    {
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);

        // define o targetY dependendo da animação
        if (state.IsName(problematicState))
        {
            targetY = originalY + yOffset;
        }
        else
        {
            targetY = originalY;
        }

        // suaviza a posição Y
        Vector3 pos = transform.position;
        pos.y = Mathf.Lerp(pos.y, targetY, Time.deltaTime * smoothSpeed);
        transform.position = pos;
    }
}