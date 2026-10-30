using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;

public class FadeController : MonoBehaviour
{
    [SerializeField] private Image fadeImage;
    [SerializeField] private PlayableDirector director;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private bool fadeOutAtEnd = true;

    private bool started;

    void Awake()
    {
        if (fadeImage != null)
        {
            SetAlpha(0f);
            fadeImage.gameObject.SetActive(false);
        }
    }

    void Start()
    {
        if (director == null) return;

        // Se déclenche quand la cutscene se termine
        director.stopped += OnCutsceneStopped;

        // Fade-in au démarrage : on part du noir et on révèle
        StartCoroutine(FadeIn());
    }

    private void OnDestroy()
    {
        if (director != null)
            director.stopped -= OnCutsceneStopped;
    }

    private IEnumerator FadeIn()
    {
        if (fadeImage == null) yield break;

        fadeImage.gameObject.SetActive(true);
        SetAlpha(1f);

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            SetAlpha(1f - Mathf.Clamp01(t / fadeDuration));
            yield return null;
        }

        SetAlpha(0f);
        fadeImage.gameObject.SetActive(false);
    }

    private IEnumerator FadeOut()
    {
        if (fadeImage == null) yield break;

        fadeImage.gameObject.SetActive(true);
        SetAlpha(0f);

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            SetAlpha(Mathf.Clamp01(t / fadeDuration));
            yield return null;
        }

        SetAlpha(1f);
    }

    private void OnCutsceneStopped(PlayableDirector d)
    {
        if (!fadeOutAtEnd || started) return;
        started = true;
        StartCoroutine(FadeOutRoutine());
    }

    private IEnumerator FadeOutRoutine()
    {
        yield return StartCoroutine(FadeOut());
        // Le noir est en place : on rend le contrôle au joueur
        Debug.Log("Cutscene terminée — retour au gameplay.");
    }

    private void SetAlpha(float a)
    {
        if (fadeImage == null) return;
        Color c = fadeImage.color;
        c.a = a;
        fadeImage.color = c;
    }
}
