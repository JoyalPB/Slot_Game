using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Takes a bet, picks random symbols (weighted RNG),
/// spins the reels, checks for a win, and pays the player.
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

    public bool IsSpinning { get; private set; }

    /// Fires when spin starts (true) or ends (false). UI uses this to lock buttons.
    public event Action<bool> OnSpinStateChanged;

    /// Fires when spin ends. Gives: did win, win amount, winning symbol (null if lost).
    public event Action<bool, int, SymbolData> OnSpinFinished;

    /// Called by Bet buttons and the lever.
    public void Spin(int bet)
    {
        if (IsSpinning) return;                       // No double-click bug
        if (!payoutManager.PlaceBet(bet)) return;     // Not enough coins
        StartCoroutine(SpinRoutine(bet));
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

        return symbols[0];  
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