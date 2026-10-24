using UnityEngine;

/// <summary>
/// Holds the data for ONE slot symbol (name, picture, payout).
/// </summary>
[CreateAssetMenu(fileName = "NewSymbol", menuName = "Slot Game/Symbol Data")]
public class SymbolData : ScriptableObject
{
    [Header("Symbol Info")]
    public string symbolName;     
    public Sprite symbolSprite;  

    [Header("Payout")]
    public int payoutMultiplier; 

    [Header("Luck")]
    [Range(1, 100)]
    public int weight = 25;       
}