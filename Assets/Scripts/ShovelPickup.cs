using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class ShovelPickup : MonoBehaviour
{
    [SerializeField] private GameObject highlightObject;
    [SerializeField] private Key pickupKey = Key.E;
    [SerializeField] private UnityEvent onPickedUp;

    private bool playerInRange;
    private bool pickedUp;

    void Start()
    {
        GetComponent<Collider>().isTrigger = true;
        if (highlightObject != null) highlightObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (pickedUp || !other.CompareTag("Player")) return;
        playerInRange = true;
        if (highlightObject != null) highlightObject.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;
        if (highlightObject != null) highlightObject.SetActive(false);
    }

    void Update()
    {
        if (pickedUp || !playerInRange) return;
        if (Keyboard.current == null) return;

        if (Keyboard.current[pickupKey].wasPressedThisFrame)
        {
            pickedUp = true;
            if (DigManager.Instance != null)
                DigManager.Instance.HasShovel = true;

            onPickedUp.Invoke();
            gameObject.SetActive(false);
        }
    }
}
