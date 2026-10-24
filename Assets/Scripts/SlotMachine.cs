using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// The boss. Holds the chosen bet, picks random symbols (weighted RNG),
/// spins the reels when the lever is pulled, checks for a win, and pays the player.
/// </summary>
public class SlotMachine : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Reel[] reels;               // Reel_1, Reel_2, Reel_3
    [SerializeField] private SymbolData[] symbols;       // The 4 symbol data files
    [SerializeField] private PayoutManager payoutManager;

    [Header("Timing")]
    [SerializeField] private float firstReelSpinTime = 1.5f;
    [SerializeField] private float extraTimePerReel = 0.7f;   // Each reel stops later than the last

    [Header("Bet")]
    [SerializeField] private int defaultBet = 10;             // Smallest bet. Used at start and as fallback.

    public bool IsSpinning { get; private set; }

    /// <summary>The bet the lever will use.</summary>
    public int CurrentBet => currentBet;

    /// <summary>Fires when spin starts (true) or ends (false). UI uses this to lock buttons.</summary>
    public event Action<bool> OnSpinStateChanged;

    /// <summary>Fires when spin ends. Gives: did win, win amount, winning symbol (null if lost).</summary>
    public event Action<bool, int, SymbolData> OnSpinFinished;

    /// <summary>Fires when the chosen bet changes. UI uses this to highlight the button.</summary>
    public event Action<int> OnBetChanged;

    private int currentBet;

    private void Awake()
    {
        currentBet = defaultBet;
    }

    /// <summary>Called by Bet buttons (OnClick). Only chooses the bet. Does NOT spin.</summary>
    public void SetBet(int bet)
    {
        if (IsSpinning || !payoutManager.CanAfford(bet)) return;
        currentBet = bet;
        OnBetChanged?.Invoke(currentBet);
    }

    /// <summary>Called by the lever. Spins using the chosen bet. Returns true if spin really started.</summary>
    public bool TrySpinWithCurrentBet()
    {
        return TrySpin(currentBet);
    }

    private bool TrySpin(int bet)
    {
        if (IsSpinning) return false;                    // No double-click bug
        if (!payoutManager.PlaceBet(bet)) return false;  // Not enough coins

        StartCoroutine(SpinRoutine(bet));
        return true;
    }

    private IEnumerator SpinRoutine(int bet)
    {
        IsSpinning = true;
        OnSpinStateChanged?.Invoke(true);

        // 1) Pick the result FIRST (RNG), then animate toward it
        SymbolData[] results = new SymbolData[reels.Length];
        for (int i = 0; i < reels.Length; i++)
        {
            results[i] = PickWeightedSymbol();
        }

        // 2) Start all reels. Each one stops a little later than the one before
        for (int i = 0; i < reels.Length; i++)
        {
            float spinTime = firstReelSpinTime + i * extraTimePerReel;
            StartCoroutine(reels[i].SpinAndStop(spinTime, results[i]));
        }

        // 3) Wait until every reel has stopped
        while (AnyReelSpinning())
        {
            yield return null;
        }

        // 4) Check win: all symbols the same
        bool isWin = AllSame(results);
        int winAmount = 0;

        if (isWin)
        {
            winAmount = bet * results[0].payoutMultiplier;
            payoutManager.AddWin(winAmount);
        }

        Debug.Log(isWin ? "WIN! " + results[0].symbolName + " pays " + winAmount : "No win");

        // 5) If player can no longer afford the chosen bet, drop back to the smallest bet
        if (!payoutManager.CanAfford(currentBet))
        {
            currentBet = defaultBet;
            OnBetChanged?.Invoke(currentBet);
        }

        IsSpinning = false;
        OnSpinStateChanged?.Invoke(false);
        OnSpinFinished?.Invoke(isWin, winAmount, isWin ? results[0] : null);
    }

    /// <summary>
    /// Weighted random pick. Higher 'weight' = shows up more often.
    /// Example weights 40, 30, 20, 10 -> total 100.
    /// Roll 0-99. Roll 0-39 = first symbol, 40-69 = second, and so on.
    /// </summary>
    private SymbolData PickWeightedSymbol()
    {
        int totalWeight = 0;
        foreach (SymbolData symbol in symbols)
        {
            totalWeight += symbol.weight;
        }

        int roll = UnityEngine.Random.Range(0, totalWeight);

        foreach (SymbolData symbol in symbols)
        {
            if (roll < symbol.weight) return symbol;
            roll -= symbol.weight;
        }

        return symbols[0];   // Should never happen, but keeps compiler happy
    }

    private bool AllSame(SymbolData[] results)
    {
        for (int i = 1; i < results.Length; i++)
        {
            if (results[i] != results[0]) return false;
        }
        return true;
    }

    private bool AnyReelSpinning()
    {
        foreach (Reel reel in reels)
        {
            if (reel.IsSpinning) return true;
        }
        return false;
    }
}