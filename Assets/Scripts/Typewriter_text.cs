using TMPro;
using UnityEngine;


public class TypewriterEffect : MonoBehaviour
{
    [SerializeField] private float charactersPerSecond = 30f;

    private TMP_Text textComponent;
    private string fullText;
    private float timer;
    private int visibleCount;
    private bool finished;
    [SerializeField] private AudioClip typingSound;
    [SerializeField] private AudioSource audioSource;


    void Awake()
    {
        textComponent = GetComponent<TMP_Text>();
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

    }

    void OnEnable()
    {
        fullText = textComponent.text;
        textComponent.text = "";
        visibleCount = 0;
        timer = 0f;
        finished = false;
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource != null && typingSound != null)
            audioSource.PlayOneShot(typingSound);

    }

    void Update()
    {
        if (finished) return;

        timer += Time.deltaTime * charactersPerSecond;
        int target = Mathf.Min(Mathf.FloorToInt(timer), fullText.Length);

        if (target != visibleCount)
        {
            visibleCount = target;
            textComponent.text = fullText.Substring(0, visibleCount);
        }

        if (visibleCount >= fullText.Length)
        {
            finished = true;
        }
    }
}
