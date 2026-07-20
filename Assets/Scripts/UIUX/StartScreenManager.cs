using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartScreenManager : MonoBehaviour
{
    [Header("Canvas 1 - Story")]
    public GameObject canvas1;
    public GameObject regiebuchClosed;
    public GameObject regiebuchStory;
    public Button rightClickButton;
    public AudioSource storyAudioSource;

    [Header("Hintergrundmusik (Audio Ducking)")]
    public AudioSource backgroundAudioSource;
    [Range(0f, 1f)] public float normalVolume = 0.5f;
    [Range(0f, 1f)] public float duckedVolume = 0.1f;
    public float fadeSpeed = 1.5f; 

    [Header("UI Sounds (NEU)")]
    public AudioSource sfxAudioSource; 
    public AudioClip clickSound;

    [Header("Canvas 2 - Weiterleitung")]
    public GameObject canvas2;
    public Button canvas2Button;

    [Header("Timings (in Sekunden)")]
    public float audioStartVerzoegerung = 1.0f;
    public float pufferNachAudio = 0.5f;

    [Header("Ziel-Szene")]
    public string mainSceneName = "MainScene";

    private bool isStoryActive = false;
    private Coroutine storyRoutine;
    private Coroutine fadeRoutine;

    void Start()
    {
        canvas1.SetActive(true);
        canvas2.SetActive(false);

        regiebuchClosed.SetActive(true);
        regiebuchStory.SetActive(false);

        rightClickButton.onClick.AddListener(OnRightClickAction);
        canvas2Button.onClick.AddListener(LoadMainScene);

        if (backgroundAudioSource != null)
        {
            backgroundAudioSource.volume = normalVolume;
        }
    }

    private void OnRightClickAction()
    {
        // Klick-Sound abspielen!
        if (sfxAudioSource != null && clickSound != null)
        {
            sfxAudioSource.PlayOneShot(clickSound);
        }

        if (!isStoryActive)
        {
            isStoryActive = true;
            
            regiebuchClosed.SetActive(false);
            regiebuchStory.SetActive(true);

            storyRoutine = StartCoroutine(StorySequenceRoutine());
        }
        else
        {
            SkipStory();
        }
    }

    private IEnumerator StorySequenceRoutine()
    {
        yield return new WaitForSeconds(audioStartVerzoegerung);

        if (storyAudioSource != null && storyAudioSource.clip != null)
        {
            FadeToVolume(duckedVolume);
            
            storyAudioSource.Play();

            while (storyAudioSource.isPlaying)
            {
                yield return null;
            }
        }

        FadeToVolume(normalVolume);

        yield return new WaitForSeconds(pufferNachAudio);

        TransitionToCanvas2();
    }

    private void SkipStory()
    {
        if (storyRoutine != null)
        {
            StopCoroutine(storyRoutine);
        }

        if (storyAudioSource != null && storyAudioSource.isPlaying)
        {
            storyAudioSource.Stop();
        }

        FadeToVolume(normalVolume);

        TransitionToCanvas2();
    }

    private void FadeToVolume(float targetVolume)
    {
        if (backgroundAudioSource == null) return;
        
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }
        fadeRoutine = StartCoroutine(FadeRoutine(targetVolume));
    }

    private IEnumerator FadeRoutine(float targetVolume)
    {
        while (Mathf.Abs(backgroundAudioSource.volume - targetVolume) > 0.01f)
        {
            backgroundAudioSource.volume = Mathf.MoveTowards(backgroundAudioSource.volume, targetVolume, fadeSpeed * Time.deltaTime);
            yield return null; 
        }
        backgroundAudioSource.volume = targetVolume;
    }

    private void TransitionToCanvas2()
    {
        canvas1.SetActive(false);
        canvas2.SetActive(true);
    }

    private void LoadMainScene()
    {
        // Optional: Auch hier beim Szenenwechsel den Sound abspielen, falls gewollt
        if (sfxAudioSource != null && clickSound != null)
        {
            sfxAudioSource.PlayOneShot(clickSound);
        }
        
        SceneManager.LoadScene(mainSceneName);
    }
}