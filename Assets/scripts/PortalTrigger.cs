using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalTrigger : MonoBehaviour
{
    public string cenaDestino;   // nome da cena, se quiser trocar de cena
    public Transform destino;    // posição, se quiser só teleportar
    public FadeController fade;  // script do fade (vai ver abaixo)

    private bool ativado = false;

    private void OnTriggerEnter(Collider other)
    {
        if (ativado) return;

        if (other.CompareTag("Player"))
        {
            ativado = true;
            StartCoroutine(Transicao());
        }
    }

    private System.Collections.IEnumerator Transicao()
    {
        // fade para preto
        yield return fade.FadeOut();

        // Se quiser trocar de cena
        if (!string.IsNullOrEmpty(cenaDestino))
            SceneManager.LoadScene(cenaDestino);
        else if (destino != null)
        {
            // teleportar player dentro da mesma cena
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            player.transform.position = destino.position;
            player.transform.rotation = destino.rotation;
        }

        // fade volta
        yield return fade.FadeIn();
    }
}
