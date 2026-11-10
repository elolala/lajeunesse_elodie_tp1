using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class CrabQuestGiver : MonoBehaviour
{
    [Header("Prompt")]
    [SerializeField] private GameObject promptUI;

    [Header("Dialogue")]
    [SerializeField] private GameObject dialogueCanvas;
    [SerializeField] private TMPro.TMP_Text dialogueText;

    [Header("Lignes - premiere rencontre")]
    [TextArea(2, 4)]
    [SerializeField]
    private string[] questLines = new string[]
    {
        "Ohé ! Toi là-bas !",
        "J'ai perdu mon coquillage préféré quelque part sur cette plage...",
        "Prends cette pelle et creuse les tas de sable, tu le trouveras !",
        "Rapporte-le-moi et je te donnerai ta première perle."
    };

    [Header("Lignes - retour du coquillage")]
    [TextArea(2, 4)]
    [SerializeField]
    private string[] rewardLines = new string[]
    {
        "Tu l'as trouvé ! Mon coquillage !",
        "Bravo, tu es un vrai chercheur de trésors !",
        "Voici ta première perle, tu l'as bien méritée.",
        "Il t'en reste deux à trouver. Bonne chance !"
    };

    [Header("Recompense")]
    [SerializeField] private int pearlsGiven = 1;

    [Header("Evenements")]
    [SerializeField] private UnityEvent onQuestStart;
    [SerializeField] private UnityEvent onRewardStart;
    [SerializeField] private UnityEvent onDialogueEnd;

    private readonly System.Collections.Generic.HashSet<Collider>
        playerColliders = new System.Collections.Generic.HashSet<Collider>();

    private bool questGiven;
    private bool rewardGiven;
    private bool talking;
    private string[] activeLines;
    private int lineIndex;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;

        if (promptUI != null) promptUI.SetActive(false);
        if (dialogueCanvas != null) dialogueCanvas.SetActive(false);
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
        playerColliders.Add(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!playerColliders.Remove(other)) return;

        if (playerColliders.Count == 0 && !talking)
        {
            if (promptUI != null) promptUI.SetActive(false);
        }
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

        bool canReward = ShellPickup.HasShell && !rewardGiven;
        bool canQuest = !questGiven;
        bool interactable = canReward || canQuest;

        if (promptUI != null && promptUI.activeSelf != interactable)
            promptUI.SetActive(interactable);

        if (!interactable) return;
        if (Keyboard.current == null) return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (canReward) StartReward();
            else StartQuest();
        }
    }

    private void StartQuest()
    {
        talking = true;
        lineIndex = 0;
        activeLines = questLines;
        OpenDialogue();
        onQuestStart.Invoke();
        LockMovement(true);
    }

    private void StartReward()
    {
        talking = true;
        lineIndex = 0;
        activeLines = rewardLines;
        OpenDialogue();
        onRewardStart.Invoke();
        LockMovement(true);
    }

    private void OpenDialogue()
    {
        if (promptUI != null) promptUI.SetActive(false);
        if (dialogueCanvas != null) dialogueCanvas.SetActive(true);
        ShowLine();
    }

    private void ShowLine()
    {
        if (dialogueText != null && activeLines != null &&
            lineIndex < activeLines.Length)
        {
            dialogueText.text = activeLines[lineIndex];
        }
    }

    private void NextLine()
    {
        lineIndex++;

        if (activeLines == null || lineIndex >= activeLines.Length)
        {
            EndDialogue();
            return;
        }

        ShowLine();
    }

    private void EndDialogue()
    {
        talking = false;

        if (dialogueCanvas != null)
            dialogueCanvas.SetActive(false);

        bool wasReward = ShellPickup.HasShell && !rewardGiven;

        if (wasReward)
        {
            rewardGiven = true;
            ShellPickup.HasShell = false;

            if (PearlCounter.Instance != null)
                PearlCounter.Instance.AddPearl(pearlsGiven);

            Debug.Log("Perle obtenue !");
        }
        else
        {
            questGiven = true;
        }

        LockMovement(false);
        onDialogueEnd.Invoke();

        if (playerColliders.Count > 0 && promptUI != null)
            promptUI.SetActive(true);
    }

    // Bloque / debloque le personnage en cherchant le controleur par son nom.
    private void LockMovement(bool locked)
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        foreach (var mb in player.GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (mb == null) continue;
            if (mb.GetType().Name.Contains("ThirdPersonController"))
                mb.enabled = !locked;
        }
    }
}
