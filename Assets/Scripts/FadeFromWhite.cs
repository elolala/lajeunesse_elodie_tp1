using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeFromWhite : MonoBehaviour
{
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1.5f;

    void Start()
    {
        if (fadeImage == null) { Debug.LogError("Fade: pas d'Image"); return; }
        fadeImage.gameObject.SetActive(true);
        SetAlpha(1f);
        StartCoroutine(FadeRoutine());
    }

    private IEnumerator FadeRoutine()
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            SetAlpha(Mathf.Lerp(1f, 0f, t / fadeDuration));
            yield return null;
        }
        SetAlpha(0f);
        fadeImage.gameObject.SetActive(false);
    }

    private void SetAlpha(float a)
    {
        Color c = fadeImage.color;
        c.a = a;
        fadeImage.color = c;
    }
}
