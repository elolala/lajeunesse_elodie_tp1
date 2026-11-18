using UnityEngine;

public class MonkeyAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private CharacterController controller;
    [SerializeField] private float walkThreshold = 0.1f;
    [SerializeField] private float runThreshold = 5f;
    [SerializeField] private float smooth = 10f;

    private Vector3 lastPos;
    private float speed;
    private bool wasGrounded = true;

    void Start()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (controller == null) controller = GetComponent<CharacterController>();
        lastPos = transform.position;
    }

    void Update()
    {
        float raw = Vector3.Distance(
            new Vector3(transform.position.x, 0, transform.position.z),
            new Vector3(lastPos.x, 0, lastPos.z)
        ) / Mathf.Max(Time.deltaTime, 0.0001f);

        lastPos = transform.position;

        speed = Mathf.Lerp(speed, raw, Time.deltaTime * smooth);
        animator.SetFloat("Speed", speed);

        if (controller != null)
        {
            bool grounded = controller.isGrounded;

            if (wasGrounded && !grounded)
                animator.SetTrigger("Jump");

            wasGrounded = grounded;
        }
    }
}
