using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shows coins, locks buttons during spin, shows win message and game over popup.
/// Only deals with the screen. No game rules here.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("Game References")]
    [SerializeField] private SlotMachine slotMachine;
    [SerializeField] private PayoutManager payoutManager;

    [Header("Coin Display")]
    [SerializeField] private TMP_Text coinText;

    [Header("Bet Buttons (same order as bet amounts)")]
    [SerializeField] private Button[] betButtons;
    [SerializeField] private int[] betAmounts = { 10, 50, 100 };

    [Header("Win Message")]
    [SerializeField] private TMP_Text winText;
    [SerializeField] private float winMessageTime = 2f;

    [Header("Game Over")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button restartButton;

    [Header("Exit")]
    [SerializeField] private Button exitButton;

    private void Start()
    {
        // Listen to the game events
        payoutManager.OnCoinsChanged += UpdateCoinText;
        slotMachine.OnSpinStateChanged += HandleSpinStateChanged;
        slotMachine.OnSpinFinished += HandleSpinFinished;

        restartButton.onClick.AddListener(RestartGame);
        exitButton.onClick.AddListener(ExitGame);

        // Show starting state
        UpdateCoinText(payoutManager.Coins);
        RefreshBetButtons();
    }

    private void OnDestroy()
    {
        // Stop listening, prevents errors when scene closes
        if (payoutManager != null) payoutManager.OnCoinsChanged -= UpdateCoinText;
        if (slotMachine != null)
        {
            slotMachine.OnSpinStateChanged -= HandleSpinStateChanged;
            slotMachine.OnSpinFinished -= HandleSpinFinished;
        }
    }

    private void UpdateCoinText(int coins)
    {
        coinText.text = coins.ToString();
    }

    private void HandleSpinStateChanged(bool isSpinning)
    {
        if (isSpinning) winText.gameObject.SetActive(false);
        RefreshBetButtons();
    }

    private void HandleSpinFinished(bool isWin, int winAmount, SymbolData winningSymbol)
    {
        if (isWin)
        {
            StopAllCoroutines();
            StartCoroutine(ShowWinMessage(winAmount));
        }

        // Cannot afford even the smallest bet? Game over.
        if (!payoutManager.CanAfford(Mathf.Min(betAmounts)))
        {
            gameOverPanel.SetActive(true);
        }
    }

    /// <summary>Bet button works only if NOT spinning and player can afford it.</summary>
    private void RefreshBetButtons()
    {
        for (int i = 0; i < betButtons.Length; i++)
        {
            betButtons[i].interactable =
                !slotMachine.IsSpinning && payoutManager.CanAfford(betAmounts[i]);
        }
    }

    private IEnumerator ShowWinMessage(int winAmount)
    {
        winText.text = "WIN! +" + winAmount;
        winText.gameObject.SetActive(true);
        yield return new WaitForSeconds(winMessageTime);
        winText.gameObject.SetActive(false);
    }

    private void RestartGame()
    {
        gameOverPanel.SetActive(false);
        payoutManager.ResetCoins();
        RefreshBetButtons();
    }

    private void ExitGame()
    {
        // Application.Quit does nothing in WebGL or the editor, so stop Play mode in editor
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}