using UnityEngine;
using UnityEngine.InputSystem;

public class CamLookGate : MonoBehaviour
{
    [Tooltip("Le composant Cinemachine Input Axis Controller de la camera.")]
    [SerializeField] private Behaviour inputAxisController;

    [Tooltip("Le PlayerInput du personnage.")]
    [SerializeField] private PlayerInput playerInput;

    [Tooltip("Delai avant de reactiver le look apres l'arret.")]
    [SerializeField] private float releaseDelay = 0.15f;

    [Tooltip("Compter aussi la rotation A/D comme un deplacement.")]
    [SerializeField] private bool includeTurning = false;

    private InputAction moveAction;
    private float idleTimer;

    void Awake()
    {
        if (playerInput != null && playerInput.actions != null)
            moveAction = playerInput.actions.FindAction("Move");
    }

    void Update()
    {
        if (moveAction == null || inputAxisController == null) return;

        Vector2 move = moveAction.ReadValue<Vector2>();

        bool moving = includeTurning
            ? move.sqrMagnitude > 0.01f
            : Mathf.Abs(move.y) > 0.01f;

        if (moving)
        {
            idleTimer = 0f;
            inputAxisController.enabled = false;
        }
        else
        {
            idleTimer += Time.deltaTime;

            if (idleTimer >= releaseDelay)
                inputAxisController.enabled = true;
        }
    }
}
