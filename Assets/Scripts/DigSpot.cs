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

    [Header("Digging")]
    [SerializeField, Min(1)] private int clicksToDig = 15;

    [Tooltip("Size of the sand just before it disappears.")]
    [SerializeField, Range(0.05f, 1f)]
    private float finalScale = 0.15f;

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

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;

        if (sandVisual == transform)
        {
            Debug.LogWarning(
                name + ": Sand Visual must be a separate child, " +
                "not the object carrying DigSpot.",
                this
            );

            sandVisual = null;
        }

        if (sandVisual != null)
            originalSandScale = sandVisual.localScale;

        SetInteractionVisuals(false);

        // Hide scene objects used as loot templates.
        // Assets/prefabs in the Project window are not changed.
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

    // Called by DigManager at the beginning of the scene.
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
                name + ": No loot object assigned for this pile.",
                this
            );
            return;
        }

        Transform destination = revealPoint != null
            ? revealPoint
            : transform;

        // No parent: the item survives when the pile is disabled.
        myItem = Instantiate(
            template,
            destination.position,
            destination.rotation
        );

        // Preserve the visible size of a scene template.
        if (template.scene.IsValid())
            myItem.transform.localScale = template.transform.lossyScale;

        myItem.SetActive(false);
    }

    private bool IsPlayer(Collider other)
    {
        // Supports colliders on children of the tagged player.
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
            SetInteractionVisuals(false);

            if (DigManager.Instance != null)
                DigManager.Instance.HideProgress();

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

        bool canDig = playerColliders.Count > 0 &&
                      manager != null &&
                      manager.HasShovel;

        SetInteractionVisuals(canDig);

        if (!canDig)
            return;

        bool clicked = Mouse.current != null &&
                       Mouse.current.leftButton.wasPressedThisFrame;

        if (!clicked)
            return;

        clicks++;

        float progress = Mathf.Clamp01(
            (float)clicks / Mathf.Max(1, clicksToDig)
        );

        // Shrink only the sand visual, not the trigger or prompt.
        if (sandVisual != null)
        {
            float multiplier = Mathf.Lerp(1f, finalScale, progress);
            sandVisual.localScale = originalSandScale * multiplier;
        }

        manager.ShowProgress(progress);

        if (clicks >= Mathf.Max(1, clicksToDig))
            CompleteDig();
    }

    private void SetInteractionVisuals(bool visible)
    {
        if (highlightObject != null &&
            highlightObject.activeSelf != visible)
        {
            highlightObject.SetActive(visible);
        }

        if (promptUI != null && promptUI.activeSelf != visible)
            promptUI.SetActive(visible);
    }

    private void CompleteDig()
    {
        dug = true;
        SetInteractionVisuals(false);

        DigManager manager = DigManager.Instance;

        if (manager != null)
            manager.HideProgress();

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

        if (hasShell && myItem != null && manager != null)
            manager.CollectShell(transform.position);

        onDigComplete.Invoke();
        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        playerColliders.Clear();
        SetInteractionVisuals(false);
    }
}
