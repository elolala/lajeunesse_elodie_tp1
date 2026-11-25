using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class SharkHit : MonoBehaviour
{
    [Header("Degats")]
    [SerializeField] private int damage = 1;

    [Tooltip("Temps minimal entre deux impacts.")]
    [SerializeField] private float hitCooldown = 2f;

    [Header("Recul")]
    [Tooltip("Distance de la charge vers le joueur au moment de l'impact.")]
    [SerializeField] private float lungeDistance = 2f;

    [Tooltip("Hauteur conservee pendant la charge (pour ne pas plonger).")]
    [SerializeField] private bool keepHeight = true;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    [Tooltip("Trigger qui declenche l'attaque (layer Attack).")]
    [SerializeField] private string attackTrigger = "isAttacking";

    [Tooltip("Parametre int qui choisit l'attaque.")]
    [SerializeField] private string attackIndexParam = "attackIndex";
    [SerializeField] private int attackIndex = 0;

    [Tooltip("Layer de l'attaque dans le controller du requin.")]
    [SerializeField] private string attackLayerName = "Attack";

    [Tooltip("Poids du layer pendant l'animation.")]
    [SerializeField, Range(0f, 1f)] private float attackLayerWeight = 1f;

    [Tooltip("Duree de l'animation d'attaque, en secondes.")]
    [SerializeField] private float attackDuration = 1.5f;

    [Header("Effets")]
    [SerializeField] private UnityEvent onHit;
    [SerializeField] private AudioClip hitSound;

    [Header("Debug")]
    [SerializeField] private bool logHits = true;

    private float lastHitTime = -99f;
    private int attackLayerIndex = -1;

    private void Awake()
    {
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;

        if (animator == null)
            animator = GetComponentInParent<Animator>();

        if (animator != null && !string.IsNullOrEmpty(attackLayerName))
            attackLayerIndex = animator.GetLayerIndex(attackLayerName);
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
        if (logHits)
            Debug.Log("REQUIN: touche par '" + other.name + "' | isPlayer=" + IsPlayer(other));

        if (!IsPlayer(other)) return;

        if (Time.time - lastHitTime < hitCooldown)
        {
            if (logHits) Debug.Log("REQUIN: cooldown actif, ignore.");
            return;
        }

        lastHitTime = Time.time;

        if (logHits) Debug.Log("REQUIN: JOUEUR TOUCHE ! -" + damage + " vie");

        PlayAttack(other.transform.position);
        LungeTowards(other.transform.position);

        if (hitSound != null)
            AudioSource.PlayClipAtPoint(hitSound, transform.position);

        if (PlayerLives.Instance != null)
            PlayerLives.Instance.LoseLife(damage);
        else if (logHits)
            Debug.LogWarning("REQUIN: PlayerLives.Instance est null !");

        onHit.Invoke();
    }

    private void PlayAttack(Vector3 playerPos)
    {
        if (animator == null)
        {
            if (logHits) Debug.LogWarning("REQUIN: Animator est null !");
            return;
        }

        if (attackLayerIndex >= 0)
        {
            animator.SetLayerWeight(attackLayerIndex, attackLayerWeight);

            if (logHits)
                Debug.Log("REQUIN: layer '" + attackLayerName +
                          "' (index " + attackLayerIndex + ") poids = " + attackLayerWeight);
        }
        else if (logHits)
        {
            Debug.LogWarning("REQUIN: layer '" + attackLayerName + "' introuvable.");
        }

        if (!string.IsNullOrEmpty(attackIndexParam))
            animator.SetInteger(attackIndexParam, attackIndex);

        if (!string.IsNullOrEmpty(attackTrigger))
            animator.SetTrigger(attackTrigger);

        if (logHits)
            Debug.Log("REQUIN: attaque envoyee. index=" + attackIndex +
                      " | trigger=" + attackTrigger);

        CancelInvoke(nameof(ResetAttackLayer));
        Invoke(nameof(ResetAttackLayer), attackDuration);
    }

    // A-coup visible vers le joueur : rend l'attaque lisible meme si
    // le clip ne bouge que la tete.
    private void LungeTowards(Vector3 playerPos)
    {
        if (lungeDistance <= 0f) return;

        Vector3 dir = playerPos - transform.position;

        if (keepHeight)
            dir.y = 0f;

        if (dir.sqrMagnitude < 0.001f) return;

        transform.position += dir.normalized * lungeDistance;

        if (logHits)
            Debug.Log("REQUIN: charge de " + lungeDistance + " unites vers le joueur.");
    }

    private void ResetAttackLayer()
    {
        if (animator != null && attackLayerIndex >= 0)
            animator.SetLayerWeight(attackLayerIndex, 0f);
    }
}
