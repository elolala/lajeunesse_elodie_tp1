using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class DigSpot : MonoBehaviour
{
    [Header("Loot")]
    [SerializeField] private GameObject shellObject;
    [SerializeField] private GameObject[] junkObjects;
    [SerializeField] private Transform revealPoint;
    [SerializeField] private GameObject dugPrefab;

    [Header("Visuals")]
    [SerializeField] private Transform sandVisual;
    [SerializeField] private GameObject highlightObject;
    [SerializeField] private GameObject promptUI;

    [Header("Son de creusage")]
    [SerializeField] private AudioSource digSound;
    [SerializeField] private AudioClip digClip;
    [SerializeField, Range(1, 3)] private int playSoundEvery = 1;

    [Header("Son de decouverte")]
    [Tooltip("Joue uniquement sur le tas qui contient le coquillage.")]
    [SerializeField] private AudioSource revealSound;
    [SerializeField] private AudioClip revealClip;

    [Header("Digging")]
    [SerializeField, Min(1)] private int clicksToDig = 10;

    [Tooltip("Taille du sable juste avant qu'il disparaisse.")]
    [SerializeField, Range(0.05f, 1f)]
    private float finalScale = 0.15f;

    [Header("Highlight")]
    [Tooltip("Allumer le highlight des que le joueur a la pelle, " +
             "et non seulement quand il est proche.")]
    [SerializeField] private bool highlightWhenShovelHeld = true;

    [Header("Events")]
    [SerializeField] private UnityEvent onPlayerEnter;
    [SerializeField] private UnityEvent onPlayerExit;
    [SerializeField] private UnityEvent onDigComplete;

    private readonly System.Collections.Generic.HashSet<Collider>
        playerColliders = new System.Collections.Generic.HashSet<Collider>();

    private Vector3 originalSandScale;
    private GameObject myItem;
    private bool hasShell;
    private bool dug;
    private int clicks;

    public bool IsDug => dug;
    public bool HasShell => hasShell;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;

        if (sandVisual == transform)
        {
            Debug.LogWarning(
                name + ": Sand Visual doit être un enfant séparé, " +
                "pas l'objet qui porte DigSpot.",
                this
            );

            sandVisual = null;
        }

        if (sandVisual != null)
            originalSandScale = sandVisual.localScale;

        if (promptUI != null)
            promptUI.SetActive(false);

        if (highlightObject != null)
            highlightObject.SetActive(false);

        // Cache les objets de scène servant de modèles de loot.
        HideSceneTemplate(shellObject);

        if (junkObjects != null)
        {
            foreach (GameObject item in junkObjects)
                HideSceneTemplate(item);
        }
    }

    private void HideSceneTemplate(GameObject template)
    {
        if (template != null && template.scene.IsValid())
            template.SetActive(false);
    }

    // Appelé par DigManager au début de la scène.
    public void AssignLoot(bool shell)
    {
        hasShell = shell;

        if (myItem != null)
            Destroy(myItem);

        GameObject template = null;

        if (shell)
        {
            template = shellObject;
        }
        else if (junkObjects != null && junkObjects.Length > 0)
        {
            template = junkObjects[Random.Range(0, junkObjects.Length)];
        }

        if (template == null)
        {
            Debug.LogWarning(
                name + ": aucun objet de loot assigné à ce tas.",
                this
            );
            return;
        }

        Transform destination = revealPoint != null
            ? revealPoint
            : transform;

        // Sans parent : l'objet survit quand le tas est désactivé.
        myItem = Instantiate(
            template,
            destination.position,
            destination.rotation
        );

        if (template.scene.IsValid())
            myItem.transform.localScale = template.transform.lossyScale;

        myItem.SetActive(false);
    }

    private bool IsPlayer(Collider other)
    {
        Transform current = other.transform;

        while (current != null)
        {
            if (current.CompareTag("Player"))
                return true;

            current = current.parent;
        }

        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (dug || !IsPlayer(other))
            return;

        bool wasOutside = playerColliders.Count == 0;
        playerColliders.Add(other);

        if (wasOutside)
            onPlayerEnter.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!playerColliders.Remove(other))
            return;

        if (playerColliders.Count == 0)
        {
            if (promptUI != null)
                promptUI.SetActive(false);

            if (DigManager.Instance != null)
                DigManager.Instance.ResetProgress();

            onPlayerExit.Invoke();
        }
    }

    private void Update()
    {
        if (dug)
            return;

        playerColliders.RemoveWhere(
            collider => collider == null ||
                        !collider.enabled ||
                        !collider.gameObject.activeInHierarchy
        );

        DigManager manager = DigManager.Instance;

        bool hasShovel = manager != null && manager.HasShovel;
        bool playerNear = playerColliders.Count > 0;

        // Highlight : allume des que le joueur a la pelle.
        bool highlightOn = hasShovel &&
                           (highlightWhenShovelHeld || playerNear);

        if (highlightObject != null &&
            highlightObject.activeSelf != highlightOn)
        {
            highlightObject.SetActive(highlightOn);
        }

        // Prompt souris : seulement quand le joueur est proche.
        bool promptOn = hasShovel && playerNear;

        if (promptUI != null && promptUI.activeSelf != promptOn)
            promptUI.SetActive(promptOn);

        if (!promptOn)
            return;

        bool clicked = Mouse.current != null &&
                       Mouse.current.leftButton.wasPressedThisFrame;

        if (!clicked)
            return;

        clicks++;

        // Son de creusage
        if (digSound != null && digClip != null)
        {
            if (playSoundEvery <= 1 || clicks % playSoundEvery == 0)
                digSound.PlayOneShot(digClip);
        }

        float progress = Mathf.Clamp01(
            (float)clicks / Mathf.Max(1, clicksToDig)
        );

        if (sandVisual != null)
        {
            float multiplier = Mathf.Lerp(1f, finalScale, progress);
            sandVisual.localScale = originalSandScale * multiplier;
        }

        manager.ShowProgress(progress);

        if (clicks >= Mathf.Max(1, clicksToDig))
            CompleteDig();
    }

    private void CompleteDig()
    {
        dug = true;

        if (promptUI != null)
            promptUI.SetActive(false);

        if (highlightObject != null)
            highlightObject.SetActive(false);

        DigManager manager = DigManager.Instance;

        if (manager != null)
            manager.ResetProgress();

        if (dugPrefab != null)
        {
            Instantiate(
                dugPrefab,
                transform.position,
                transform.rotation
            );
        }

        if (myItem != null)
            myItem.SetActive(true);

        // Son de decouverte : uniquement si c'est le coquillage.
        if (hasShell && revealSound != null && revealClip != null)
            revealSound.PlayOneShot(revealClip);

        if (hasShell && myItem != null && manager != null)
            manager.CollectShell(transform.position);

        onDigComplete.Invoke();
        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        playerColliders.Clear();
    }
}
