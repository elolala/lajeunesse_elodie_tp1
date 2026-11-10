using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Boutons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button storyButton;

    [Header("Panneaux")]
    [SerializeField] private GameObject storyPanel;
    [SerializeField] private GameObject menuPanel;

    [Header("Fondu visuel")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1.5f;

    [Header("Musique")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private float musicFadeDuration = 1.5f;

    [Header("Souris")]
    [SerializeField] private bool lockCursorOnStart = true;

    [Header("Scène")]
    [SerializeField] private string gameSceneName = "ile";

    private bool busy;

    void Start()
    {
        // Dans le menu, le curseur doit rester visible et cliquable.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (storyPanel != null) storyPanel.SetActive(false);

        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
            fadeImage.gameObject.SetActive(false);
        }

        if (startButton != null) startButton.onClick.AddListener(StartGame);
        if (storyButton != null) storyButton.onClick.AddListener(OpenStory);
    }

    public void OpenStory()
    {
        if (storyPanel != null) storyPanel.SetActive(true);
        if (menuPanel != null) menuPanel.SetActive(false);
    }

    public void CloseStory()
    {
        if (storyPanel != null) storyPanel.SetActive(false);
        if (menuPanel != null) menuPanel.SetActive(true);
    }

    public void StartGame()
    {
        if (busy) return;
        busy = true;
        StartCoroutine(StartGameRoutine());
    }

    private IEnumerator StartGameRoutine()
    {
        // Verrouille la souris pour le jeu.
        if (lockCursorOnStart)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        StartCoroutine(FadeMusic());
        yield return StartCoroutine(FadeTo(1f));

        SceneManager.LoadScene(gameSceneName);
    }

    private IEnumerator FadeTo(float targetAlpha)
    {
        if (fadeImage == null) yield break;

        fadeImage.gameObject.SetActive(true);
        float start = fadeImage.color.a;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            Color c = fadeImage.color;
            c.a = Mathf.Lerp(start, targetAlpha, t / fadeDuration);
            fadeImage.color = c;
            yield return null;
        }

        Color final = fadeImage.color;
        final.a = targetAlpha;
        fadeImage.color = final;
    }

    private IEnumerator FadeMusic()
    {
        if (musicSource == null) yield break;

        float startVolume = musicSource.volume;
        float t = 0f;

        while (t < musicFadeDuration)
        {
            t += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, t / musicFadeDuration);
            yield return null;
        }

        musicSource.volume = 0f;
        musicSource.Stop();
    }
}
