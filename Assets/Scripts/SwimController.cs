using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class SwimController : MonoBehaviour
{
    [Header("Nage")]
    [SerializeField] private float swimSpeed = 4f;
    [SerializeField] private float verticalSpeed = 3f;
    [SerializeField] private float acceleration = 8f;

    [Tooltip("Vitesse de rotation quand le joueur tourne la souris.")]
    [SerializeField] private float turnSpeed = 120f;

    [Header("Flottabilite")]
    [SerializeField] private float bobbing = 0.15f;
    [SerializeField] private float bobSpeed = 1.5f;

    private Rigidbody rb;
    private Vector3 inputDir;
    private bool moving;
    private float bobPhase;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.freezeRotation = false;
        rb.constraints = RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationZ;
    }

    void Update()
    {
        if (Keyboard.current == null) return;

        float h = 0f, v = 0f;

        if (Keyboard.current.aKey.isPressed ||
            Keyboard.current.leftArrowKey.isPressed) h -= 1f;
        if (Keyboard.current.dKey.isPressed ||
            Keyboard.current.rightArrowKey.isPressed) h += 1f;
        if (Keyboard.current.wKey.isPressed ||
            Keyboard.current.upArrowKey.isPressed) v += 1f;
        if (Keyboard.current.sKey.isPressed ||
            Keyboard.current.downArrowKey.isPressed) v -= 1f;

        inputDir = new Vector3(h, 0f, v).normalized;
        moving = inputDir.sqrMagnitude > 0.01f;

        // Monte / descend avec Espace et Ctrl
        float rise = 0f;
        if (Keyboard.current.spaceKey.isPressed) rise += 1f;
        if (Keyboard.current.leftCtrlKey.isPressed) rise -= 1f;

        Vector3 wish = transform.forward * inputDir.z * swimSpeed
                     + transform.right * inputDir.x * swimSpeed
                     + Vector3.up * rise * verticalSpeed;

        // Acceleration douce, comme dans l'eau
        Vector3 target = new Vector3(wish.x, rb.linearVelocity.y + rise * 0.4f, wish.z);
        rb.linearVelocity = Vector3.Lerp(
            rb.linearVelocity, target, Time.deltaTime * acceleration
        );

        // Rotation avec A/D quand on n'avance pas
        if (h != 0f)
            transform.Rotate(0f, h * turnSpeed * Time.deltaTime, 0f);

        // Flottement leger
        bobPhase += Time.deltaTime * bobSpeed;
    }

    void FixedUpdate()
    {
        if (!moving) return;

        // Legere derive verticale pour un effet "dans l'eau"
        Vector3 vel = rb.linearVelocity;
        vel.y += Mathf.Sin(bobPhase) * bobbing;
        rb.linearVelocity = vel;
    }
}
