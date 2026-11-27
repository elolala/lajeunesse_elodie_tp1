using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class FlashbackZone : MonoBehaviour
{
    [SerializeField] private Volume flashbackVolume;
    [SerializeField] private float startTime = 10f;
    [SerializeField] private float endTime = 20f;
    [SerializeField] private float fadeDuration = 0.4f;

    private float timer;
    private bool fadedIn;
    private bool fadedOut;

    void Start()
    {
        if (flashbackVolume != null) flashbackVolume.weight = 0f;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (!fadedIn && timer >= startTime)
        {
            fadedIn = true;
            StartCoroutine(FadeTo(1f));
        }

        if (!fadedOut && timer >= endTime)
        {
            fadedOut = true;
            StartCoroutine(FadeTo(0f));
        }
    }

    private IEnumerator FadeTo(float target)
    {
        float start = flashbackVolume.weight;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            flashbackVolume.weight = Mathf.Lerp(start, target, t / fadeDuration);
            yield return null;
        }

        flashbackVolume.weight = target;
    }
}
