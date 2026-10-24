using System;
using UnityEngine;

/// <summary>
/// Keeps track of the player's coins: bets, wins, and game over.
/// Other scripts listen to OnCoinsChanged to update the screen.
/// </summary>
public class PayoutManager : MonoBehaviour
{
    [SerializeField] private int startingCoins = 100;

    public int Coins { get; private set; }

    /// Fires every time coins change. UI listens to this.
    public event Action<int> OnCoinsChanged;

    private void Awake()
    {
        ResetCoins();
    }

    public bool CanAfford(int bet)
    {
        return Coins >= bet;
    }

    /// Take the bet from the player. Returns false if too poor.
    public bool PlaceBet(int bet)
    {
        if (!CanAfford(bet)) return false;
        Coins -= bet;
        OnCoinsChanged?.Invoke(Coins);
        return true;
    }

    /// Give the player winnings.
    public void AddWin(int amount)
    {
        Coins += amount;
        OnCoinsChanged?.Invoke(Coins);
    }

    public void ResetCoins()
    {
        Coins = startingCoins;
        OnCoinsChanged?.Invoke(Coins);
    }
}