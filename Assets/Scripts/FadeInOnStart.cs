using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeInOnStart : MonoBehaviour
{
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1.5f;

    void Start()
    {
        if (fadeImage == null) return;
        StartCoroutine(Fade());
    }

    private IEnumerator Fade()
    {
        fadeImage.gameObject.SetActive(true);
        SetAlpha(1f);

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
