using UnityEngine;
using UnityEngine.UI;

public class DigManager : MonoBehaviour
{
    public static DigManager Instance { get; private set; }

    public bool HasShovel { get; set; }

    [Header("UI")]
    [SerializeField] private Image progressBar;

    [Header("Objective")]
    [SerializeField] private int shellsNeeded = 1;
    private int shellsFound;

    [Header("Randomisation")]
    [SerializeField] private int shellsToHide = 1;
    [SerializeField] private bool newLayoutOnRestart = true;

    public event System.Action OnShellFound;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (newLayoutOnRestart)
            Random.InitState(System.Environment.TickCount);
        else
            Random.InitState(12345);
    }

    void Start()
    {
        if (progressBar != null)
            progressBar.transform.parent.gameObject.SetActive(false);

        AssignLoot();
    }

    private void AssignLoot()
    {
        var spots = FindObjectsByType<DigSpot>(FindObjectsInactive.Exclude);
        if (spots.Length == 0) return;

        for (int i = 0; i < spots.Length; i++)
        {
            int r = Random.Range(i, spots.Length);
            var temp = spots[i];
            spots[i] = spots[r];
            spots[r] = temp;
        }

        for (int i = 0; i < spots.Length; i++)
            spots[i].AssignLoot(i < shellsToHide);
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
