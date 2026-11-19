using UnityEngine;

public class TreasureFall : MonoBehaviour
{
    [Header("Chute")]
    [Tooltip("Position de depart, en coordonnees locales au parent.")]
    [SerializeField] private Vector3 startOffset = new Vector3(0f, 8f, 0f);

    [Tooltip("Position d'arrivee, en coordonnees locales au parent.")]
    [SerializeField] private Vector3 endOffset = new Vector3(0f, -6f, 0f);

    [Tooltip("Duree de la descente en secondes.")]
    [SerializeField] private float duration = 8f;

    [Tooltip("Temps d'attente avant que la chute commence.")]
    [SerializeField] private float startDelay = 0f;

    [Header("Rotation")]
    [Tooltip("Inclinaison de base du tresor, en degres.")]
    [SerializeField] private Vector3 baseRotation = Vector3.zero;

    [Tooltip("Tours complets effectues pendant la descente.")]
    [SerializeField] private float spinTurns = 1f;

    [Header("Fin")]
    [Tooltip("Desactive l'objet une fois la chute terminee.")]
    [SerializeField] private bool disableWhenDone = false;

    private Vector3 basePos;
    private float timer;
    private bool running;

    void Awake()
    {
        basePos = transform.localPosition;
        ApplyOffsets(0f);
    }

    void OnEnable()
    {
        timer = -startDelay;
        running = true;
        ApplyOffsets(0f);
    }

    void Update()
    {
        if (!running) return;

        timer += Time.deltaTime;
        if (timer < 0f) return;

        float t = Mathf.Clamp01(timer / Mathf.Max(0.01f, duration));

        ApplyOffsets(t);

        if (t >= 1f)
        {
            running = false;

            if (disableWhenDone)
                gameObject.SetActive(false);
        }
    }

    private void ApplyOffsets(float t)
    {
        Vector3 pos = Vector3.Lerp(startOffset, endOffset, t);
        transform.localPosition = basePos + pos;

        transform.localRotation = Quaternion.Euler(
            baseRotation.x,
            baseRotation.y + 360f * spinTurns * t,
            baseRotation.z
        );
    }
}
