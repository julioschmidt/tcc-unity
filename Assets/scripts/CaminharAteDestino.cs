using UnityEngine;

public class CaminharAteDestino : MonoBehaviour
{
    public Transform destino;       // arraste o objeto "Destino" aqui no Inspector
    public Animator animator;       // arraste o Animator do personagem
    public float velocidade = 2f;   // velocidade de movimento
    public float distanciaParada = 0.2f; // distância mínima para considerar "chegou"

    private bool chegou = false;

    void Start()
    {
        // começa andando
        animator.SetBool("isWalking", true);
    }

    void Update()
    {
        if (chegou || destino == null) return;

        // calcula distância
        float distancia = Vector3.Distance(transform.position, destino.position);

        if (distancia > distanciaParada)
        {
            // move em direção ao destino
            Vector3 direcao = (destino.position - transform.position).normalized;
            transform.position += direcao * velocidade * Time.deltaTime;

            // gira para olhar para frente
            if (direcao != Vector3.zero)
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(direcao),
                    0.15f
                );
        }
        else
        {
            // chegou!
            chegou = true;
            animator.SetBool("isWalking", false); // troca para Idle

            // força um ajuste no Y (por exemplo, -0.1f mais baixo)
            Vector3 pos = transform.position;
            pos.y -= 15f; // ajuste fino até ele não ficar voando
            transform.position = pos;
        }
    }
}