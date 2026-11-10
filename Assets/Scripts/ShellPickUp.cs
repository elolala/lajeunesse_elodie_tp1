using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class ShellPickup : MonoBehaviour
{
    [Header("Effet")]
    [SerializeField] private float floatAmplitude = 0.15f;
    [SerializeField] private float floatSpeed = 2f;
    [SerializeField] private float spinSpeed = 60f;

    [Header("Prompt")]
    [SerializeField] private GameObject promptUI;
    [SerializeField] private GameObject highlightObject;
    [SerializeField] private Key pickupKey = Key.E;

    [Header("Son de ramassage")]
    [SerializeField] private AudioClip pickupClip;
    [SerializeField] private float pickupVolume = 0.8f;

    [Header("Événements")]
    [SerializeField] private UnityEvent onCollected;

    /// <summary>Vrai quand le joueur possede le coquillage.</summary>
    public static bool HasShell;

    private readonly System.Collections.Generic.HashSet<Collider>
        playerColliders = new System.Collections.Generic.HashSet<Collider>();

    private Vector3 startPos;
    private bool collected;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
        startPos = transform.position;

        if (promptUI != null) promptUI.SetActive(false);
        if (highlightObject != null) highlightObject.SetActive(false);
    }

    private bool IsPlayer(Collider other)
    {
        Transform current = other.transform;
        while (current != null)
        {
            if (current.CompareTag("Player")) return true;
            current = current.parent;
        }
        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (collected || !IsPlayer(other)) return;

        playerColliders.Add(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!playerColliders.Remove(other)) return;

        if (playerColliders.Count == 0)
        {
            if (promptUI != null) promptUI.SetActive(false);
            if (highlightObject != null) highlightObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (collected) return;

        // Flottement et rotation pour attirer l'oeil
        transform.position = startPos +
            Vector3.up * Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;

        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);

        playerColliders.RemoveWhere(
            c => c == null || !c.enabled || !c.gameObject.activeInHierarchy
        );

        bool near = playerColliders.Count > 0;

        if (promptUI != null && promptUI.activeSelf != near)
            promptUI.SetActive(near);

        if (highlightObject != null &&
            highlightObject.activeSelf != near)
        {
            highlightObject.SetActive(near);
        }

        if (!near) return;
        if (Keyboard.current == null) return;

        // Appuie sur E pour ramasser
        if (Keyboard.current[pickupKey].wasPressedThisFrame)
            Collect();
    }

    private void Collect()
    {
        collected = true;
        HasShell = true;

        if (promptUI != null) promptUI.SetActive(false);
        if (highlightObject != null) highlightObject.SetActive(false);

        // Le son survit a la desactivation de l'objet.
        if (pickupClip != null)
            AudioSource.PlayClipAtPoint(
                pickupClip,
                transform.position,
                pickupVolume
            );

        Debug.Log("Coquillage ramasse ! Va le porter au crabe.");

        onCollected.Invoke();
        gameObject.SetActive(false);
    }
}
