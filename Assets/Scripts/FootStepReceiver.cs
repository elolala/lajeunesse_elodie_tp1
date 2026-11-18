using UnityEngine;

public class FootstepReceiver : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip footstepClip;
    [SerializeField] private float volume = 0.5f;

    // Appele par les AnimationEvents des clips du pack Suriyun
    public void OnFootstep()
    {
        if (audioSource != null && footstepClip != null)
            audioSource.PlayOneShot(footstepClip, volume);
        else if (footstepClip != null)
            AudioSource.PlayClipAtPoint(footstepClip, transform.position, volume);
    }

    public void OnLand() { }   // meme chose pour l'atterrissage
    public void OnFootstepLeft() { OnFootstep(); }
    public void OnFootstepRight() { OnFootstep(); }
}
