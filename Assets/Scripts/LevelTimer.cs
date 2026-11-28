using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class LevelTimer : MonoBehaviour
{
    public static LevelTimer Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject timerPanel;
    [SerializeField] private TMP_Text timerText;

    [Header("Duree")]
    [SerializeField, Min(1f)] private float duration = 30f;

    [Header("Couleurs")]
    [SerializeField] private Color normalColor = new Color(1f, 0.95f, 0.86f);   // ivoire #FFF3DC
    [SerializeField] private Color warningColor = new Color(0.91f, 0.51f, 0.42f); // corail #E8836B
    [SerializeField] private float warningThreshold = 10f;

    [Header("Fin du temps")]
    [SerializeField] private string failScene = "Echec";
    [SerializeField] private bool loadSceneOnFail = true;
    [SerializeField] private UnityEvent onTimeUp;

    private float remaining;
    private bool running;
    private bool failed;

    public bool IsRunning => running;
    public float Remaining => remaining;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (timerPanel != null) timerPanel.SetActive(false);
    }

    void Update()
    {
        if (!running || failed) return;

        remaining -= Time.deltaTime;

        if (remaining <= 0f)
        {
            remaining = 0f;
            UpdateText();
            TimeUp();
            return;
        }

        UpdateText();
    }

    // Appele quand le joueur ramasse la pelle
    public void StartTimer()
    {
        if (running) return;

        running = true;
        failed = false;
        remaining = duration;

        if (timerPanel != null) timerPanel.SetActive(true);
        UpdateText();

        Debug.Log("Timer lance : " + duration + " secondes");
    }

    public void StopTimer()
    {
        running = false;
        if (timerPanel != null) timerPanel.SetActive(false);
    }

    // Ajoute du temps (bonus)
    public void AddTime(float seconds)
    {
        if (!running) return;
        remaining += seconds;
        UpdateText();
    }

    private void UpdateText()
    {
        if (timerText == null) return;

        int seconds = Mathf.CeilToInt(remaining);
        timerText.text = seconds + " s";
        timerText.color = seconds <= warningThreshold ? warningColor : normalColor;
    }

    private void TimeUp()
    {
        failed = true;
        running = false;

        Debug.Log("Temps ecoule !");

        onTimeUp.Invoke();

        if (loadSceneOnFail && !string.IsNullOrEmpty(failScene))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            SceneManager.LoadScene(failScene);
        }
    }
}
