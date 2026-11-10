using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class CrabQuestGiver : MonoBehaviour
{
    [Header("Prompt")]
    [SerializeField] private GameObject promptUI;
    [SerializeField] private GameObject highlightObject;

    [Header("Dialogue")]
    [SerializeField] private GameObject dialogueCanvas;
    [SerializeField] private TMPro.TMP_Text speakerName;
    [SerializeField] private TMPro.TMP_Text dialogueText;

    [Header("Lignes de dialogue")]
    [TextArea(2, 4)]
    [SerializeField]
    private string[] lines = new string[]
    {
        "Ohé ! Toi là-bas !",
        "J'ai perdu mon coquillage préféré quelque part sur cette plage...",
        "Prends cette pelle et creuse les tas de sable, tu le trouveras !",
        "Rapporte-le-moi et je te donnerai ta première perle."
    };

    [Header("Récompense")]
    [SerializeField] private int pearlsGiven = 1;
    [SerializeField] private bool onlyOnce = true;

    [Header("Événements")]
    [SerializeField] private UnityEvent onDialogueStart;
    [SerializeField] private UnityEvent onDialogueEnd;

    private readonly System.Collections.Generic.HashSet<Collider>
        playerColliders = new System.Collections.Generic.HashSet<Collider>();

    private bool questGiven;
    private bool talking;
    private int lineIndex;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
        SetVisuals(false);

        if (dialogueCanvas != null)
            dialogueCanvas.SetActive(false);
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
        if (!IsPlayer(other)) return;

        bool wasOutside = playerColliders.Count == 0;
        playerColliders.Add(other);

        if (wasOutside && !talking)
            SetVisuals(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!playerColliders.Remove(other)) return;

        if (playerColliders.Count == 0 && !talking)
            SetVisuals(false);
    }

    private void Update()
    {
        if (talking)
        {
            bool next = (Mouse.current != null &&
                         Mouse.current.leftButton.wasPressedThisFrame)
                     || (Keyboard.current != null &&
                         Keyboard.current.eKey.wasPressedThisFrame);

            if (next) NextLine();
            return;
        }

        if (playerColliders.Count == 0) return;
        if (onlyOnce && questGiven) return;

        bool pressed = Keyboard.current != null &&
                       Keyboard.current.eKey.wasPressedThisFrame;

        if (pressed) StartDialogue();
    }

    private void StartDialogue()
    {
        if (lines == null || lines.Length == 0) return;

        talking = true;
        lineIndex = 0;
        SetVisuals(false);

        if (dialogueCanvas != null)
            dialogueCanvas.SetActive(true);

        if (speakerName != null)
            speakerName.text = "Crabe";

        ShowLine();
        onDialogueStart.Invoke();

        MovementLock(true);
    }

    private void NextLine()
    {
        lineIndex++;

        if (lineIndex >= lines.Length)
        {
            EndDialogue();
            return;
        }

        ShowLine();
    }

    private void ShowLine()
    {
        if (dialogueText != null)
            dialogueText.text = lines[lineIndex];
    }

    private void EndDialogue()
    {
        talking = false;

        if (dialogueCanvas != null)
            dialogueCanvas.SetActive(false);

        if (!questGiven)
        {
            questGiven = true;

            if (PearlCounter.Instance != null)
                PearlCounter.Instance.AddPearl(pearlsGiven);
        }

        MovementLock(false);
        onDialogueEnd.Invoke();

        if (playerColliders.Count > 0)
            SetVisuals(true);
    }

    private void SetVisuals(bool visible)
    {
        if (promptUI != null && promptUI.activeSelf != visible)
            promptUI.SetActive(visible);

        if (highlightObject != null && highlightObject.activeSelf != visible)
            highlightObject.SetActive(visible);
    }

    // Bloque le personnage pendant la discussion.
    private void MovementLock(bool locked)
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        var controllers = player.GetComponentsInChildren<MonoBehaviour>(true);
        foreach (var c in controllers)
        {
            if (c == null) continue;
            if (c.GetType().Name.Contains("ThirdPersonController"))
                c.enabled = !locked;
        }
    }
}
