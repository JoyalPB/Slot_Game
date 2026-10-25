using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shows coins, locks buttons during spin, highlights the chosen bet,
/// shows the win board and game over popup, and triggers win effects and sounds.
/// Only deals with the screen. No game rules here.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("Game References")]
    [SerializeField] private SlotMachine slotMachine;
    [SerializeField] private PayoutManager payoutManager;

    [Header("Effects and Sound")]
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private PartyPopper partyPopper;
    

    [Header("Coin Display")]
    [SerializeField] private TMP_Text coinText;

    [Header("Bet Buttons (same order as bet amounts)")]
    [SerializeField] private Button[] betButtons;
    [SerializeField] private int[] betAmounts = { 10, 50, 100 };
    [SerializeField] private Color selectedColor = Color.yellow;   // Color of the chosen bet
    [SerializeField] private Color normalColor = Color.white;

    [Header("Win Message")]
    [SerializeField] private GameObject winBoard;    // The board that holds WinText
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
        slotMachine.OnBetChanged += HandleBetChanged;

        restartButton.onClick.AddListener(RestartGame);
        exitButton.onClick.AddListener(ExitGame);

        // Play a click on every bet button, the exit button and the restart button
        foreach (Button button in betButtons)
            button.onClick.AddListener(() => audioManager.PlayButtonClick());
        #if UNITY_WEBGL
        exitButton.gameObject.SetActive(false);   // Browsers cannot quit, so hide the button
        #endif
        exitButton.onClick.AddListener(() => audioManager.PlayButtonClick());
        restartButton.onClick.AddListener(() => audioManager.PlayButtonClick());

        // Show starting state
        UpdateCoinText(payoutManager.Coins);
        HandleBetChanged(slotMachine.CurrentBet);

    }

    private void OnDestroy()
    {
        // Stop listening, prevents errors when scene closes
        if (payoutManager != null) payoutManager.OnCoinsChanged -= UpdateCoinText;
        if (slotMachine != null)
        {
            slotMachine.OnSpinStateChanged -= HandleSpinStateChanged;
            slotMachine.OnSpinFinished -= HandleSpinFinished;
            slotMachine.OnBetChanged -= HandleBetChanged;
        }
    }

    private void UpdateCoinText(int coins)
    {
        coinText.text = coins.ToString();
        RefreshBetButtons();   // Coins changed, so some bets may now be too expensive
    }

    private void HandleSpinStateChanged(bool isSpinning)
    {
        if (isSpinning)
        {
            if (audioManager != null) audioManager.StartSpinLoop();

            StopAllCoroutines();
            winBoard.transform.localScale = Vector3.one;   // Reset in case pop was cut short
            winBoard.SetActive(false);
        }
        else
        {
            if (audioManager != null) audioManager.StopSpinLoop();
        }

        RefreshBetButtons();
    }

    private void HandleSpinFinished(bool isWin, int winAmount, SymbolData winningSymbol)
    {
        if (isWin)
        {
            StopAllCoroutines();
            StartCoroutine(ShowWinMessage());

            if (audioManager != null) audioManager.PlayWin();
            if (partyPopper != null) partyPopper.Play();
        }

        // Cannot afford even the smallest bet? Game over.
        if (!payoutManager.CanAfford(Mathf.Min(betAmounts)))
        {
            gameOverPanel.SetActive(true);
            if (audioManager != null) audioManager.PlayGameOver();
        }
    }

    /// <summary>Colors the chosen bet button so the player sees what the lever will use.</summary>
    private void HandleBetChanged(int bet)
    {
        for (int i = 0; i < betButtons.Length; i++)
        {
            Image img = betButtons[i].GetComponent<Image>();
            if (img != null)
            {
                img.color = (betAmounts[i] == bet) ? selectedColor : normalColor;
            }
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

    private IEnumerator ShowWinMessage()
    {
        winText.text = "WIN!";
        winBoard.SetActive(true);

        // Little pop: grow from small to normal size
        Transform board = winBoard.transform;
        float t = 0f;
        while (t < 0.25f)
        {
            t += Time.deltaTime;
            board.localScale = Vector3.one * Mathf.Lerp(0.5f, 1f, t / 0.25f);
            yield return null;
        }
        board.localScale = Vector3.one;

        yield return new WaitForSeconds(winMessageTime);
        winBoard.SetActive(false);
    }

    private void RestartGame()
    {
        gameOverPanel.SetActive(false);
        payoutManager.ResetCoins();
        RefreshBetButtons();
    }

    private void ExitGame()
    {
        // Application.Quit does nothing in WebGL, so stop Play mode in editor
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}