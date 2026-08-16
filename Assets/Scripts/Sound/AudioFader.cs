using UnityEngine;
using System.Collections;

public static class AudioFader
{
    public static IEnumerator FadeOut(AudioSource audioSource, float FadeTime)
    {
        float startVolume = audioSource.volume;

        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / FadeTime;

            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVolume;
    }

    public static IEnumerator FadeIn(AudioSource audioSource, float FadeTime)
    {
        //Debug.Log("Fading in audio source: " + audioSource.name);
        float startVolume = audioSource.volume;
        audioSource.volume = 0f;

        audioSource.Play();

        while (audioSource.volume < startVolume)
        {
            audioSource.volume += startVolume * Time.deltaTime / FadeTime;

            yield return null;
        }

        audioSource.volume = startVolume;
    }

    public static IEnumerator FadeIn(AudioSource audioSource, float FadeTime, float finalVolume)
    {
        //Debug.Log("Fading in audio source: " + audioSource.name);
        audioSource.volume = 0f;

        audioSource.Play();

        while (audioSource.volume < finalVolume)
        {
            audioSource.volume += finalVolume * Time.deltaTime / FadeTime;

            yield return null;
        }

        audioSource.volume = finalVolume;
    }

    public static IEnumerator FadeIn(AudioSource audioSource, float FadeTime, float finalVolume, float initialVolume)
    {
        //Debug.Log("Fading in audio source: " + audioSource.name);
        audioSource.volume = initialVolume;

        audioSource.Play();

        while (audioSource.volume < finalVolume)
        {
            audioSource.volume += finalVolume * Time.deltaTime / FadeTime;

            yield return null;
        }

        audioSource.volume = finalVolume;
    }

}
