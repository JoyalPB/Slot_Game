using UnityEngine;

/// <summary>
/// Plays all game sounds. Other scripts just call PlayLever(), PlayWin(), etc.
/// Keeps sound code in one place.
/// </summary>
public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource effectsSource;   // One-shot sounds
    [SerializeField] private AudioSource spinLoopSource;  // Looping spin sound
    [SerializeField] private AudioSource ambienceSource;  // Quiet background sound

    [Header("Clips")]
    [SerializeField] private AudioClip spinLoopClip;
    [SerializeField] private AudioClip winClip;
    [SerializeField] private AudioClip gameOverClip;
    [SerializeField] private AudioClip ambienceClip;
    [SerializeField] private AudioClip buttonClickClip;

    private void Awake()
    {
        if (effectsSource == null || spinLoopSource == null)
        {
            Debug.LogError("AudioManager: Audio Source slot is empty.", this);
            enabled = false;
            return;
        }

        spinLoopSource.clip = spinLoopClip;
        spinLoopSource.loop = true;
        spinLoopSource.playOnAwake = false;
        effectsSource.playOnAwake = false;
    }

    private void Start()
    {
        // Quiet background ambience (optional)
        if (ambienceClip != null && ambienceSource != null)
        {
            ambienceSource.clip = ambienceClip;
            ambienceSource.loop = true;
            ambienceSource.volume = 0.2f;
            ambienceSource.Play();
        }
    }

    public void PlayWin() { Play(winClip); }
    public void PlayGameOver() { Play(gameOverClip); }
    public void PlayButtonClick() { Play(buttonClickClip); }

    public void StartSpinLoop()
    {
        if (spinLoopClip != null && !spinLoopSource.isPlaying) spinLoopSource.Play();
    }

    public void StopSpinLoop()
    {
        spinLoopSource.Stop();
    }

    private void Play(AudioClip clip)
    {
        if (clip != null) effectsSource.PlayOneShot(clip);
    }
}