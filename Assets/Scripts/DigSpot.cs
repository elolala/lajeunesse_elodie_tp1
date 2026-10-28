using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class DigSpot : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private bool hasShell;
    [SerializeField] private GameObject highlightObject;
    [SerializeField] private GameObject hiddenObject;
    [SerializeField] private GameObject dugPrefab;

    [Header("Digging")]
    [SerializeField] private int clicksToDig = 15;
    [SerializeField] private float drainSpeed = 2f;

    [Header("Events")]
    [SerializeField] private UnityEvent onPlayerEnter;
    [SerializeField] private UnityEvent onPlayerExit;
    [SerializeField] private UnityEvent onDigComplete;

    private float progress;
    private bool playerInRange;
    private bool dug;

    public bool IsDug => dug;

    void Start()
    {
        GetComponent<Collider>().isTrigger = true;
        if (highlightObject != null) highlightObject.SetActive(false);
        if (hiddenObject != null) hiddenObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (dug || !other.CompareTag("Player")) return;

        playerInRange = true;
        onPlayerEnter.Invoke();

        if (DigManager.Instance != null && DigManager.Instance.HasShovel && highlightObject != null)
            highlightObject.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;
        onPlayerExit.Invoke();

        if (highlightObject != null) highlightObject.SetActive(false);
        if (DigManager.Instance != null) DigManager.Instance.HideProgress();
        progress = 0f;
    }

    void Update()
    {
        if (dug || !playerInRange) return;
        if (DigManager.Instance == null || !DigManager.Instance.HasShovel) return;

        if (highlightObject != null && !highlightObject.activeSelf)
            highlightObject.SetActive(true);

        if (progress > 0f)
        {
            progress -= drainSpeed * Time.deltaTime;
            if (progress < 0f) progress = 0f;
        }

        bool clicked = Mouse.current != null
                    && Mouse.current.leftButton.wasPressedThisFrame;

        if (clicked)
        {
            progress += 1f;
            DigManager.Instance.ShowProgress(progress / clicksToDig);

            if (progress >= clicksToDig)
                CompleteDig();
        }
    }

    private void CompleteDig()
    {
        dug = true;
        DigManager.Instance.ShowProgress(1f);

        if (dugPrefab != null)
            Instantiate(dugPrefab, transform.position, transform.rotation);

        if (hiddenObject != null)
            hiddenObject.SetActive(true);

        if (hasShell)
            DigManager.Instance.CollectShell(transform.position);

        onDigComplete.Invoke();
        gameObject.SetActive(false);
    }
}
