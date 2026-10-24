using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The lever. Click it: picture swaps to "pulled down", reels spin, then lever goes back up.
/// Locked while reels spin.
/// </summary>
public class LeverController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SlotMachine slotMachine;
    [SerializeField] private Button leverButton;
    [SerializeField] private Image leverImage;

    [Header("Lever Pictures")]
    [SerializeField] private Sprite leverUpSprite;     // slot-machine2
    [SerializeField] private Sprite leverDownSprite;   // slot-machine3

    [Header("Feel")]
    [SerializeField] private float pullTime = 0.3f;    // How long lever stays down

    private bool isPulling;

    private void Start()
    {
        leverButton.onClick.AddListener(OnLeverClicked);
        slotMachine.OnSpinStateChanged += HandleSpinStateChanged;
        leverImage.sprite = leverUpSprite;
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
            leverImage.sprite = leverDownSprite;       // Lever down
            yield return new WaitForSeconds(pullTime);
            leverImage.sprite = leverUpSprite;         // Lever back up
        }

        isPulling = false;
    }

    /// <summary>Lock the lever while reels spin.</summary>
    private void HandleSpinStateChanged(bool isSpinning)
    {
        leverButton.interactable = !isSpinning;
    }
}