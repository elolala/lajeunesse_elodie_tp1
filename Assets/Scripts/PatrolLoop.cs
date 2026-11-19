using UnityEngine;

public class PatrolLoop : MonoBehaviour
{
    [Header("Trajet")]
    [Tooltip("Point de depart du requin.")]
    [SerializeField] private Transform pointA;

    [Tooltip("Point d'arrivee du requin.")]
    [SerializeField] private Transform pointB;

    [Header("Mouvement")]
    [Tooltip("Vitesse de deplacement du requin dans la scene.")]
    [SerializeField] private float speed = 3f;

    [Tooltip("Temps d'attente au bout du trajet, avant de repartir.")]
    [SerializeField] private float pauseAtEnds = 0.5f;

    [Header("Rotation")]
    [Tooltip("Coche seulement si aucun autre script ne gere l'orientation.")]
    [SerializeField] private bool rotateTowardsMovement = false;

    [Range(0f, 20f)]
    [SerializeField] private float turnSmoothness = 3f;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    [Tooltip("Nom exact du parametre bool qui declenche la nage.")]
    [SerializeField] private string moveParam = "isMoving";

    [Tooltip("Nom exact du parametre float de vitesse (vide si absent).")]
    [SerializeField] private string velocityParam = "velocity";

    [Tooltip("Vitesse envoyee au Blend Tree. Independante du deplacement.")]
    [SerializeField] private float animVelocity = 1.5f;

    private float t;
    private float pauseTimer;
    private bool goingForward = true;

    void Start()
    {
        if (pointA == null || pointB == null)
        {
            Debug.LogWarning("PatrolLoop : assigne pointA et pointB.", this);
            enabled = false;
            return;
        }

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        transform.position = pointA.position;

        // Force l'etat de nage des le debut
        if (animator != null)
        {
            if (!string.IsNullOrEmpty(moveParam))
                animator.SetBool(moveParam, true);

            if (!string.IsNullOrEmpty(velocityParam))
                animator.SetFloat(velocityParam, animVelocity);
        }
    }

    void OnDisable()
    {
        if (animator != null && !string.IsNullOrEmpty(moveParam))
            animator.SetBool(moveParam, false);
    }

    void Update()
    {
        // Pause au bout du trajet
        if (pauseTimer > 0f)
        {
            pauseTimer -= Time.deltaTime;
            return;
        }

        float distance = Vector3.Distance(pointA.position, pointB.position);
        if (distance < 0.01f) return;

        // Avance le long du segment
        t += (speed / distance) * Time.deltaTime * (goingForward ? 1f : -1f);
        t = Mathf.Clamp01(t);

        transform.position = Vector3.Lerp(pointA.position, pointB.position, t);

        // Le Blend Tree de nage suit animVelocity, pas le deplacement
        if (animator != null && !string.IsNullOrEmpty(velocityParam))
            animator.SetFloat(velocityParam, animVelocity);

        // Rotation optionnelle
        if (rotateTowardsMovement)
        {
            Vector3 dir = goingForward
                ? (pointB.position - pointA.position)
                : (pointA.position - pointB.position);

            dir.y = 0f;

            if (dir.sqrMagnitude > 0.001f)
            {
                Quaternion target = Quaternion.Euler(
                    0f,
                    Quaternion.LookRotation(dir).eulerAngles.y,
                    0f
                );

                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    target,
                    Time.deltaTime * turnSmoothness
                );
            }
        }

        // Demi-tour au bout du trajet
        if (t >= 1f && goingForward)
        {
            goingForward = false;
            pauseTimer = pauseAtEnds;
        }
        else if (t <= 0f && !goingForward)
        {
            goingForward = true;
            pauseTimer = pauseAtEnds;
        }
    }
}
