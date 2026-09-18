using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartScreenManager : MonoBehaviour
{
    [Header("Canvas 1 – Brief")]
    public GameObject canvas1;
    public Button letterButton;
    public Button skipButton;
    public GameObject[] letterFrames;

    [Header("Stop Motion")]
    [Min(0f)]
    public float firstFrameHold = 0.25f;

    [Min(0.01f)]
    public float frameDuration = 0.45f;

    [Min(0f)]
    public float finalFrameHold = 0.4f;

    [Header("Stop-Motion-Sounds")]
    public AudioSource stopMotionAudioSource;
    public AudioClip[] letterFrameSounds;

    [Range(0.8f, 1.2f)]
    public float minimumFramePitch = 0.98f;

    [Range(0.8f, 1.2f)]
    public float maximumFramePitch = 1.02f;

    [Header("Brief-Audio")]
    public AudioSource letterAudioSource;

    [Header("Hintergrundmusik")]
    public AudioSource backgroundAudioSource;

    [Range(0f, 1f)]
    public float normalVolume = 0.5f;

    [Range(0f, 1f)]
    public float duckedVolume = 0.1f;

    [Min(0.01f)]
    public float fadeSpeed = 1.5f;

    [Header("UI-Sounds")]
    public AudioSource sfxAudioSource;
    public AudioClip clickSound;

    [Header("Canvas 2 – Vorhang auf")]
    public GameObject canvas2;
    public Button canvas2Button;

    [Header("Übergang")]
    [Min(0f)]
    public float delayAfterAudio = 0.5f;

    public string mainSceneName = "MainScene";

    private Coroutine storyRoutine;
    private Coroutine fadeRoutine;

    private bool sequenceStarted;
    private bool transitionStarted;

    private void Start()
    {
        if (canvas1 != null)
        {
            canvas1.SetActive(true);
        }

        if (canvas2 != null)
        {
            canvas2.SetActive(false);
        }

        ShowOnlyFrame(0);

        if (letterButton != null)
        {
            letterButton.interactable = true;

            letterButton.onClick.AddListener(
                StartLetterSequence
            );
        }

        if (skipButton != null)
        {
            skipButton.gameObject.SetActive(
                false
            );

            skipButton.onClick.AddListener(
                SkipLetterSequence
            );
        }

        if (canvas2Button != null)
        {
            canvas2Button.onClick.AddListener(
                LoadMainScene
            );
        }

        if (letterAudioSource != null)
        {
            letterAudioSource.Stop();
        }

        if (stopMotionAudioSource != null)
        {
            stopMotionAudioSource.Stop();
        }

        if (backgroundAudioSource != null)
        {
            backgroundAudioSource.volume =
                normalVolume;
        }
    }

    private void OnDestroy()
    {
        if (letterButton != null)
        {
            letterButton.onClick.RemoveListener(
                StartLetterSequence
            );
        }

        if (skipButton != null)
        {
            skipButton.onClick.RemoveListener(
                SkipLetterSequence
            );
        }

        if (canvas2Button != null)
        {
            canvas2Button.onClick.RemoveListener(
                LoadMainScene
            );
        }
    }

    private void StartLetterSequence()
    {
        if (sequenceStarted ||
            transitionStarted)
        {
            return;
        }

        sequenceStarted = true;

        if (letterButton != null)
        {
            letterButton.interactable = false;
        }

        if (skipButton != null)
        {
            skipButton.gameObject.SetActive(
                true
            );
        }

        PlayClickSound();
        PlayFrameSound(0);

        storyRoutine = StartCoroutine(
            LetterSequenceRoutine()
        );
    }

    private IEnumerator LetterSequenceRoutine()
    {
        yield return new WaitForSecondsRealtime(
            firstFrameHold
        );

        for (int i = 1;
             i < letterFrames.Length;
             i++)
        {
            ShowOnlyFrame(i);
            PlayFrameSound(i);

            if (i <
                letterFrames.Length - 1)
            {
                yield return
                    new WaitForSecondsRealtime(
                        frameDuration
                    );
            }
        }

        yield return new WaitForSecondsRealtime(
            finalFrameHold
        );

        if (stopMotionAudioSource != null)
        {
            stopMotionAudioSource.pitch = 1f;
        }

        if (letterAudioSource != null &&
            letterAudioSource.clip != null)
        {
            FadeBackgroundTo(
                duckedVolume
            );

            letterAudioSource.Play();

            while (letterAudioSource.isPlaying)
            {
                yield return null;
            }
        }

        FadeBackgroundTo(
            normalVolume
        );

        yield return new WaitForSecondsRealtime(
            delayAfterAudio
        );

        TransitionToCanvas2();
    }

    private void ShowOnlyFrame(
        int activeFrameIndex
    )
    {
        if (letterFrames == null)
        {
            return;
        }

        for (int i = 0;
             i < letterFrames.Length;
             i++)
        {
            if (letterFrames[i] != null)
            {
                letterFrames[i].SetActive(
                    i ==
                    activeFrameIndex
                );
            }
        }
    }

    private void PlayFrameSound(
        int frameIndex
    )
    {
        if (stopMotionAudioSource == null ||
            letterFrameSounds == null ||
            frameIndex < 0 ||
            frameIndex >=
            letterFrameSounds.Length)
        {
            return;
        }

        AudioClip frameSound =
            letterFrameSounds[frameIndex];

        if (frameSound == null)
        {
            return;
        }

        stopMotionAudioSource.pitch =
            Random.Range(
                minimumFramePitch,
                maximumFramePitch
            );

        stopMotionAudioSource.PlayOneShot(
            frameSound
        );
    }

    public void SkipLetterSequence()
    {
        if (transitionStarted)
        {
            return;
        }

        PlayClickSound();

        if (storyRoutine != null)
        {
            StopCoroutine(
                storyRoutine
            );

            storyRoutine = null;
        }

        if (letterAudioSource != null)
        {
            letterAudioSource.Stop();
        }

        if (stopMotionAudioSource != null)
        {
            stopMotionAudioSource.Stop();
            stopMotionAudioSource.pitch = 1f;
        }

        FadeBackgroundTo(
            normalVolume
        );

        TransitionToCanvas2();
    }

    private void TransitionToCanvas2()
    {
        if (transitionStarted)
        {
            return;
        }

        transitionStarted = true;

        if (skipButton != null)
        {
            skipButton.gameObject.SetActive(
                false
            );
        }

        if (canvas1 != null)
        {
            canvas1.SetActive(false);
        }

        if (canvas2 != null)
        {
            canvas2.SetActive(true);
        }
    }

    private void FadeBackgroundTo(
        float targetVolume
    )
    {
        if (backgroundAudioSource == null)
        {
            return;
        }

        if (fadeRoutine != null)
        {
            StopCoroutine(
                fadeRoutine
            );
        }

        fadeRoutine = StartCoroutine(
            FadeBackgroundRoutine(
                targetVolume
            )
        );
    }

    private IEnumerator FadeBackgroundRoutine(
        float targetVolume
    )
    {
        while (Mathf.Abs(
                   backgroundAudioSource.volume -
                   targetVolume
               ) > 0.01f)
        {
            backgroundAudioSource.volume =
                Mathf.MoveTowards(
                    backgroundAudioSource.volume,
                    targetVolume,
                    fadeSpeed *
                    Time.unscaledDeltaTime
                );

            yield return null;
        }

        backgroundAudioSource.volume =
            targetVolume;

        fadeRoutine = null;
    }

    private void LoadMainScene()
    {
        PlayClickSound();

        SceneManager.LoadScene(
            mainSceneName
        );
    }

    private void PlayClickSound()
    {
        if (sfxAudioSource != null &&
            clickSound != null)
        {
            sfxAudioSource.PlayOneShot(
                clickSound
            );
        }
    }
}