using UnityEngine;

/// <summary>
/// Fires confetti from one or more particle systems.
/// Other scripts just call Play().
/// </summary>
public class PartyPopper : MonoBehaviour
{
    [SerializeField] private ParticleSystem[] poppers;   // Left and right poppers

    /// <summary>Shoot the confetti.</summary>
    public void Play()
    {
        foreach (ParticleSystem popper in poppers)
        {
            if (popper == null) continue;
            popper.Clear();   // Restart clean if one is still falling
            popper.Play();
        }
    }
}