using TMPro;
using UnityEngine;

public class PearlCounter : MonoBehaviour
{
    public static PearlCounter Instance { get; private set; }

    [SerializeField] private TMP_Text counterText;
    [SerializeField] private int totalPearls = 3;

    [Tooltip("Perles deja gagnees au demarrage de la scene.")]
    [SerializeField] private int startingPearls = 0;

    private int collected;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        collected = Mathf.Clamp(startingPearls, 0, totalPearls);
        UpdateText();
    }

    public void AddPearl(int amount = 1)
    {
        collected += amount;
        collected = Mathf.Clamp(collected, 0, totalPearls);
        UpdateText();
    }

    public bool HasAllPearls => collected >= totalPearls;

    private void UpdateText()
    {
        if (counterText != null)
            counterText.text = collected + "/" + totalPearls;
    }
}
