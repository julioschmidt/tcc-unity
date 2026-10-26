using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeController : MonoBehaviour
{
    public Image fadeImage;
    public float fadeDuration = 3f;

    void Start()
    {
        // Garante que começa transparente
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
        }
    }

    public IEnumerator FadeOut()
    {
        yield return Fade(0f, 1f); // transparente  preto
    }

    public IEnumerator FadeIn()
    {
        // Garante que começa preto antes de clarear
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 1f;
            fadeImage.color = c;
        }

        yield return Fade(1f, 0f); // preto  transparente
    }

    private IEnumerator Fade(float start, float end)
    {
        float elapsed = 0f;
        Color c = fadeImage.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(start, end, elapsed / fadeDuration);
            c.a = alpha;
            fadeImage.color = c;
            yield return null;
        }

        c.a = end;
        fadeImage.color = c;
    }
}