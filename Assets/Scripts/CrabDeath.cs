using UnityEngine;

public class CrabDeath : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private string deadParam = "IsDead";

    [Tooltip("Tuer le crabe des le debut de la scene.")]
    [SerializeField] private bool deadOnStart = true;

    private bool dead;

    void Awake()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        if (deadOnStart) Kill();
    }

    public void Kill()
    {
        if (dead) return;
        dead = true;

        if (animator != null && !string.IsNullOrEmpty(deadParam))
            animator.SetBool(deadParam, true);
    }

    public void Revive()
    {
        dead = false;

        if (animator != null && !string.IsNullOrEmpty(deadParam))
            animator.SetBool(deadParam, false);
    }
}
