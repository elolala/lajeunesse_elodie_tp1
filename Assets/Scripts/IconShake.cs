using UnityEngine;

public class IconShake : MonoBehaviour
{
    [SerializeField] private float shakeSpeed = 12f;
    [SerializeField] private float shakeAngle = 8f;
    [SerializeField] private bool pulse = true;
    [SerializeField] private float pulseAmount = 0.06f;

    private Quaternion startRot;
    private Vector3 startScale;

    void Start()
    {
        startRot = transform.localRotation;
        startScale = transform.localScale;
    }

    void Update()
    {
        float wave = Mathf.Sin(Time.unscaledTime * shakeSpeed);

        // Petite rotation de gauche à droite
        transform.localRotation = startRot * Quaternion.Euler(0f, 0f, wave * shakeAngle);

        // Grossit et rétrécit
        if (pulse)
            transform.localScale = startScale * (1f + wave * pulseAmount);
    }
}
