using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The lever. Click it: the full lever hides and the "pulled" picture shows
/// (only the ball top is visible, like the pole sank into the machine).
/// Reels spin, then the lever goes back up. Locked while reels spin.
/// </summary>
public class LeverController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SlotMachine slotMachine;
    [SerializeField] private Button leverButton;       // The Lever (button)
    [SerializeField] private Image leverUpImage;       // The Lever's Image (slot-machine2)
    [SerializeField] private GameObject leverPulled;   // LeverPulled (slot-machine3)
    [SerializeField] private AudioManager audioManager;

    [Header("Feel")]
    [SerializeField] private float pulledTime = 0.5f;  // How long the lever stays down

    private bool isPulling;

    private void Start()
    {
        // Guard: tell us clearly what is missing instead of a vague crash
        if (slotMachine == null || leverButton == null || leverUpImage == null || leverPulled == null)
        {
            Debug.LogError("LeverController: an Inspector slot is empty.", this);
            enabled = false;
            return;
        }

        ShowLeverUp();
        leverButton.onClick.AddListener(OnLeverClicked);
        slotMachine.OnSpinStateChanged += HandleSpinStateChanged;
    }

    private void OnDestroy()
    {
        if (slotMachine != null)
            slotMachine.OnSpinStateChanged -= HandleSpinStateChanged;
    }

    private void OnLeverClicked()
    {
        if (isPulling || slotMachine.IsSpinning) return;
        StartCoroutine(PullRoutine());
    }

    private IEnumerator PullRoutine()
    {
        isPulling = true;

        // Try to start the spin. If player is too poor, lever does nothing.
        if (slotMachine.TrySpinWithCurrentBet())
        {
            ShowLeverDown();
            if (audioManager != null) audioManager.PlayLever();

            yield return new WaitForSeconds(pulledTime);
            ShowLeverUp();
        }

        isPulling = false;
    }

    // Hide the pole picture (keep the object alive so the button and coroutine keep working)
    private void ShowLeverDown()
    {
        leverUpImage.enabled = false;
        leverPulled.SetActive(true);
    }

    private void ShowLeverUp()
    {
        leverUpImage.enabled = true;
        leverPulled.SetActive(false);
    }

    /// <summary>Lock the lever while reels spin.</summary>
    private void HandleSpinStateChanged(bool isSpinning)
    {
        leverButton.interactable = !isSpinning;
    }
}