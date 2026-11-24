using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class TreasurePickup : MonoBehaviour
{
    [Header("Prompt")]
    [SerializeField] private GameObject promptUI;
    [SerializeField] private GameObject highlightObject;
    [SerializeField] private Key pickupKey = Key.E;

    [Header("Événements")]
    [SerializeField] private UnityEvent onCollected;

    private readonly System.Collections.Generic.HashSet<Collider>
        playerColliders = new System.Collections.Generic.HashSet<Collider>();

    private bool collected;

    void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
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

        playerColliders.RemoveWhere(
            c => c == null || !c.enabled || !c.gameObject.activeInHierarchy
        );

        bool near = playerColliders.Count > 0;

        if (promptUI != null && promptUI.activeSelf != near)
            promptUI.SetActive(near);

        if (highlightObject != null && highlightObject.activeSelf != near)
            highlightObject.SetActive(near);

        if (!near) return;
        if (Keyboard.current == null) return;

        if (Keyboard.current[pickupKey].wasPressedThisFrame)
            Collect();
    }

    private void Collect()
    {
        collected = true;

        if (promptUI != null) promptUI.SetActive(false);
        if (highlightObject != null) highlightObject.SetActive(false);

        Debug.Log("Trésor récupéré !");

        onCollected.Invoke();
        gameObject.SetActive(false);
    }
}
