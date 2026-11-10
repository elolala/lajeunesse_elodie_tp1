using UnityEngine;

public class MovementLock : MonoBehaviour
{
    public static MovementLock Instance { get; private set; }

    [SerializeField] private MonoBehaviour movementScript;
    [SerializeField] private string scriptName = "ThirdPersonController";

    private bool originalState;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (movementScript == null)
            movementScript = FindMovementScript();

        if (movementScript != null)
        {
            originalState = movementScript.enabled;
            Debug.Log("MovementLock : controle sur " + movementScript.GetType().Name);
        }
        else
        {
            Debug.LogWarning("MovementLock : aucun script trouve. Tag Player manquant ?");
        }
    }

    private MonoBehaviour FindMovementScript()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return null;

        foreach (var mb in player.GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (mb != null && mb.GetType().Name.Contains(scriptName))
                return mb;
        }

        return null;
    }

    public void Lock()
    {
        if (movementScript != null) movementScript.enabled = false;
    }

    public void Unlock()
    {
        if (movementScript != null) movementScript.enabled = originalState;
    }
}
