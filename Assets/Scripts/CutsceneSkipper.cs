using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.InputSystem;

public class CutsceneSkipper : MonoBehaviour
{
    [Tooltip("Le Playable Director de la cutscene.")]
    [SerializeField] private PlayableDirector director;

    [Tooltip("Touche pour passer la cutscene.")]
    [SerializeField] private Key skipKey = Key.Enter;

    [Tooltip("Petit delai avant de pouvoir passer (pour eviter un skip accidentel).")]
    [SerializeField] private float startDelay = 0.5f;

    [Tooltip("Empecher de passer apres un certain temps.")]
    [SerializeField] private float skipUntilTime = -1f;

    [Header("Apres le skip (optionnel)")]
    [Tooltip("Canvas de fondu noir a activer en sautant.")]
    [SerializeField] private GameObject fadeOverlay;

    [SerializeField] private float fadeOutDuration = 0.5f;

    [Header("Debug")]
    [SerializeField] private bool logSkip = true;

    private bool skipped;
    private float elapsed;

    void Start()
    {
        if (director == null)
        {
            director = FindFirstObjectByType<PlayableDirector>();
            if (director == null)
            {
                Debug.LogWarning("CutsceneSkipper : aucun PlayableDirector trouve.");
                enabled = false;
                return;
            }
        }
    }

    void Update()
    {
        if (skipped) return;
        if (director == null) return;

        elapsed += Time.deltaTime;

        if (elapsed < startDelay) return;
        if (skipUntilTime > 0f && elapsed > skipUntilTime) return;

        if (Keyboard.current == null) return;

        if (Keyboard.current[skipKey].wasPressedThisFrame)
            Skip();
    }

    public void Skip()
    {
        if (skipped) return;
        skipped = true;

        if (logSkip) Debug.Log("Cutscene passee.");

        // Saute a la fin du Timeline
        if (director.playableAsset != null)
        {
            director.time = director.playableAsset.duration;
            director.Evaluate();
        }

        director.Stop();

        if (fadeOverlay != null)
            fadeOverlay.SetActive(true);
    }
}
