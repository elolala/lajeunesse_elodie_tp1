using UnityEngine;

public class MonkeyAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;

    [Tooltip("Force l'etat de nage en permanence (niveau sous-marin).")]
    [SerializeField] private bool alwaysSwim = true;

    [Tooltip("Nom du parametre bool de nage.")]
    [SerializeField] private string swimParam = "Swim";

    [Header("Nage mesuree (si Always Swim est decoche)")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private float walkThreshold = 0.1f;
    [SerializeField] private float runThreshold = 8f;
    [SerializeField] private float smooth = 10f;

    private Vector3 lastPos;
    private float speed;

    void Start()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (controller == null) controller = GetComponent<CharacterController>();
        lastPos = transform.position;

        if (animator != null && alwaysSwim && !string.IsNullOrEmpty(swimParam))
            animator.SetBool(swimParam, true);
    }

    void Update()
    {
        if (alwaysSwim) return;
        if (animator == null) return;

        float raw = Vector3.Distance(
            new Vector3(transform.position.x, 0, transform.position.z),
            new Vector3(lastPos.x, 0, lastPos.z)
        ) / Mathf.Max(Time.deltaTime, 0.0001f);

        lastPos = transform.position;

        speed = Mathf.Lerp(speed, raw, Time.deltaTime * smooth);
        animator.SetFloat("Speed", speed);
    }
}
