using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCameraController : MonoBehaviour
{
    [Header("Cible")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 1.5f, -4f);
    [SerializeField] private float followSmoothTime = 0f;

    [Header("Orbite souris")]
    [SerializeField] private InputActionReference lookAction;
    [SerializeField] private float orbitSensitivity = 90f;
    [SerializeField] private float minPitch = -60f;
    [SerializeField] private float maxPitch = 85f;

    [Tooltip("Demander de maintenir un bouton pour tourner la camera.")]
    [SerializeField] private bool requireHoldForOrbit = false;
    [SerializeField] private InputActionReference orbitHoldAction;

    [Header("Debug")]
    [SerializeField] private bool logLookInput = false;

    [Header("Limites de terrain")]
    [SerializeField] private Terrain terrainLimit;
    [SerializeField] private float terrainClearance = 0.1f;

    private Vector3 followVelocity;
    private float yaw;
    private float pitch;

    private void Awake()
    {
        if (target == null)
            Debug.LogWarning("ThirdPersonCameraController : aucune cible assignee.");

        if (terrainLimit == null)
            terrainLimit = FindFirstObjectByType<Terrain>();
    }

    private void OnEnable()
    {
        if (lookAction != null)
            lookAction.action.Enable();
        else
            Debug.LogWarning("ThirdPersonCameraController : Look Action est vide !");

        if (orbitHoldAction != null)
            orbitHoldAction.action.Enable();

        Vector3 euler = transform.eulerAngles;
        yaw = euler.y;
        pitch = euler.x;
    }

    private void OnDisable()
    {
        lookAction?.action.Disable();
        orbitHoldAction?.action.Disable();
    }

    private void LateUpdate()
    {
        if (target == null) return;

        UpdateOrbit();
        UpdatePosition();
    }

    private void UpdateOrbit()
    {
        bool allowOrbit = !requireHoldForOrbit;

        if (requireHoldForOrbit)
        {
            if (orbitHoldAction != null)
                allowOrbit = orbitHoldAction.action.IsPressed();
            else if (Mouse.current != null)
                allowOrbit = Mouse.current.rightButton.isPressed;
        }

        Vector2 rawLook = Vector2.zero;
        bool actionReady = lookAction != null && lookAction.action.enabled;

        if (actionReady)
            rawLook = lookAction.action.ReadValue<Vector2>();

        Vector2 lookInput = allowOrbit ? rawLook : Vector2.zero;

        if (logLookInput)
        {
            Debug.Log("Look=" + rawLook + " | allow=" + allowOrbit +
                      " | enabled=" + actionReady +
                      " | cursor=" + Cursor.lockState);
        }

        yaw += lookInput.x * orbitSensitivity * Time.deltaTime;
        pitch -= lookInput.y * orbitSensitivity * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    private void UpdatePosition()
    {
        Vector3 desiredPosition = target.position + transform.rotation * offset;
        ApplyTerrainClamp(ref desiredPosition);

        if (followSmoothTime < 0.001f)
        {
            transform.position = desiredPosition;
            followVelocity = Vector3.zero;
        }
        else
        {
            float smoothTime = Mathf.Max(0.0001f, followSmoothTime);
            transform.position = Vector3.SmoothDamp(
                transform.position, desiredPosition, ref followVelocity, smoothTime
            );
        }
    }

    private void ApplyTerrainClamp(ref Vector3 position)
    {
        if (terrainLimit == null) return;

        Vector3 terrainOrigin = terrainLimit.transform.position;
        float terrainHeight = terrainLimit.SampleHeight(position) + terrainOrigin.y + terrainClearance;

        if (position.y < terrainHeight)
            position.y = terrainHeight;
    }
}
