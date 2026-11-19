using UnityEngine;
using UnityEngine.InputSystem;

public class SwimLook : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Transform cameraRoot;
    [SerializeField] private float mouseSensitivity = 0.15f;

    [Tooltip("Limite de rotation verticale du point de visee.")]
    [SerializeField] private float pitchLimit = 60f;

    private float yaw;
    private float pitch;

    void Start()
    {
        yaw = transform.eulerAngles.y;
    }

    void LateUpdate()
    {
        if (Mouse.current == null) return;

        Vector2 delta = Mouse.current.delta.ReadValue();

        // Rotation horizontale : tout le personnage tourne
        yaw += delta.x * mouseSensitivity;
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);

        // Rotation verticale : seulement le point de visee
        if (cameraRoot != null)
        {
            pitch -= delta.y * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, -pitchLimit, pitchLimit);
            cameraRoot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }
    }
}
