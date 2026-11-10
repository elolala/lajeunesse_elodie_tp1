using UnityEngine;
using UnityEngine.Playables;

public class CutsceneMovementLock : MonoBehaviour
{
    [SerializeField] private PlayableDirector director;
    [SerializeField] private MonoBehaviour movementScript;

    private bool originalMovementState;
    private bool initialized;

    private void Awake()
    {
        if (director == null || movementScript == null)
        {
            Debug.LogError(
                "Assigne Director et Movement Script dans l'Inspector.",
                this
            );

            enabled = false;
            return;
        }

        originalMovementState = movementScript.enabled;
        initialized = true;

        // Bloque le personnage avant le lancement automatique.
        movementScript.enabled = false;
    }

    private void OnEnable()
    {
        if (!initialized || director == null)
            return;

        director.played += OnCutsceneStarted;
        director.stopped += OnCutsceneFinished;
    }

    private void OnDisable()
    {
        if (director != null)
        {
            director.played -= OnCutsceneStarted;
            director.stopped -= OnCutsceneFinished;
        }

        RestoreMovement();
    }

    private void OnCutsceneStarted(PlayableDirector source)
    {
        if (this == null || movementScript == null)
            return;

        movementScript.enabled = false;
    }

    private void OnCutsceneFinished(PlayableDirector source)
    {
        if (this == null)
            return;

        RestoreMovement();
    }

    private void RestoreMovement()
    {
        if (initialized && movementScript != null)
            movementScript.enabled = originalMovementState;
    }
}
