using UnityEngine;
using UnityEngine.UI;

public class DigManager : MonoBehaviour
{
    public static DigManager Instance { get; private set; }

    public bool HasShovel { get; set; }

    [Header("UI")]
    [SerializeField] private Image progressBar;
    [SerializeField] private GameObject promptText;

    [Header("Objective")]
    [SerializeField] private int shellsNeeded = 1;
    private int shellsFound;

    public event System.Action OnShellFound;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (progressBar != null)
            progressBar.transform.parent.gameObject.SetActive(false);
    }

    public void ShowProgress(float t)
    {
        if (progressBar == null) return;
        progressBar.transform.parent.gameObject.SetActive(true);
        progressBar.fillAmount = Mathf.Clamp01(t);
    }

    public void HideProgress()
    {
        if (progressBar == null) return;
        progressBar.transform.parent.gameObject.SetActive(false);
    }

    public void CollectShell(Vector3 pos)
    {
        shellsFound++;
        OnShellFound?.Invoke();

        if (shellsFound >= shellsNeeded)
        {
            Debug.Log("All shells found!");
            // Hook your cutscene / win UI here
        }
    }
}
