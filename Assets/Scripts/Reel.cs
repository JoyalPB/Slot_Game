using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls ONE reel: endless spin, then smooth stop on a chosen symbol.
/// The target symbol is given to a slot while it is hidden above the window,
/// then that slot slides down to the middle. This avoids a visible picture "pop".
/// </summary>
public class Reel : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private RectTransform[] symbolSlots;  // Child Images (use 6, top to bottom)
    [SerializeField] private SymbolData[] allSymbols;      // The 4 symbol data files
    [SerializeField] private float symbolHeight = 130f;    // Step: distance from one symbol to the next

    [Header("Spin Feel")]
    [SerializeField] private float spinSpeed = 1500f;      // Pixels per second while spinning fast

    [Header("Stop Feel")]
    [SerializeField] private float stopSharpness = 6f;     // Higher = stops faster and harder

    /// <summary>The symbol sitting in the middle of the window when stopped.</summary>
    public SymbolData CurrentSymbol { get; private set; }

    public bool IsSpinning { get; private set; }
    public event System.Action OnReelStopped;

    private Image[] slotImages;
    private SymbolData[] slotSymbols;   // Which symbol each slot is showing

    // Used for the hidden target swap
    private SymbolData pendingTarget;       // Symbol waiting to be given to a hidden slot
    private int landingSlotIndex = -1;      // The slot that will land in the middle

    private void Awake()
    {
        // Guard: tell us clearly if Inspector is not filled
        if (symbolSlots == null || symbolSlots.Length == 0 ||
            allSymbols == null || allSymbols.Length == 0)
        {
            Debug.LogError("Reel: Symbol Slots or All Symbols is empty. Fill them in Inspector.", this);
            enabled = false;
            return;
        }

        // Cache the Image of each slot, and give each slot a starting symbol
        slotImages = new Image[symbolSlots.Length];
        slotSymbols = new SymbolData[symbolSlots.Length];

        for (int i = 0; i < symbolSlots.Length; i++)
        {
            slotImages[i] = symbolSlots[i].GetComponent<Image>();
            SetSlotSymbol(i, allSymbols[i % allSymbols.Length]);
        }

        PlaceSlotsInStack();
        CurrentSymbol = slotSymbols[symbolSlots.Length / 2];
    }

    /// <summary>Spin fast, then stop on 'target'. Target appears only after wrapping above the window.</summary>
    public IEnumerator SpinAndStop(float spinDuration, SymbolData target)
    {
        IsSpinning = true;

        // 1) Fast spin for spinDuration seconds
        float timer = 0f;
        while (timer < spinDuration)
        {
            MoveSlots(spinSpeed * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }

        // 2) Keep spinning until a slot wraps to the top (hidden). That slot gets the target.
        pendingTarget = target;
        landingSlotIndex = -1;
        while (landingSlotIndex < 0)
        {
            MoveSlots(spinSpeed * Time.deltaTime);
            yield return null;
        }

        // 3) Slide that slot down to the middle, slowing down as it gets close
        RectTransform landingSlot = symbolSlots[landingSlotIndex];
        while (landingSlot.anchoredPosition.y > 0.5f)
        {
            float remaining = landingSlot.anchoredPosition.y;
            float speed = Mathf.Clamp(remaining * stopSharpness, 60f, spinSpeed);
            float step = Mathf.Min(speed * Time.deltaTime, remaining);
            MoveSlots(step);
            yield return null;
        }

        // 4) Final snap so every slot sits exactly on the grid (no tiny errors)
        MoveSlots(landingSlot.anchoredPosition.y);
        SnapToGrid(landingSlotIndex);

        CurrentSymbol = target;
        IsSpinning = false;
        OnReelStopped?.Invoke();
    }

    /// <summary>Move all slots down. Slots that fall off the bottom wrap to the top (hidden).</summary>
    private void MoveSlots(float amount)
    {
        float totalHeight = symbolHeight * symbolSlots.Length;
        float bottomLimit = -totalHeight / 2f - symbolHeight / 2f;

        for (int i = 0; i < symbolSlots.Length; i++)
        {
            Vector2 pos = symbolSlots[i].anchoredPosition;
            pos.y -= amount;

            // Fell below the bottom? Teleport to the top (hidden above the window)
            if (pos.y < bottomLimit)
            {
                pos.y += totalHeight;

                if (pendingTarget != null)
                {
                    // Hidden slot gets the final symbol now
                    SetSlotSymbol(i, pendingTarget);
                    landingSlotIndex = i;
                    pendingTarget = null;
                }
                else
                {
                    // Normal wrap: show a random picture
                    SetSlotSymbol(i, allSymbols[Random.Range(0, allSymbols.Length)]);
                }
            }

            symbolSlots[i].anchoredPosition = pos;
        }
    }

    /// <summary>Place all slots exactly 'symbolHeight' apart, with the landing slot at y = 0.</summary>
    private void SnapToGrid(int middleIndex)
    {
        // Sort slot numbers by current height (top to bottom)
        List<int> order = new List<int>();
        for (int i = 0; i < symbolSlots.Length; i++) order.Add(i);
        order.Sort((a, b) => symbolSlots[b].anchoredPosition.y.CompareTo(symbolSlots[a].anchoredPosition.y));

        int middlePosition = order.IndexOf(middleIndex);
        for (int p = 0; p < order.Count; p++)
        {
            float y = (middlePosition - p) * symbolHeight;
            symbolSlots[order[p]].anchoredPosition = new Vector2(0f, y);
        }
    }

    /// <summary>Line the slots up in a vertical stack. The slot in the middle of the list sits at y = 0.</summary>
    private void PlaceSlotsInStack()
    {
        int middle = symbolSlots.Length / 2;
        for (int i = 0; i < symbolSlots.Length; i++)
        {
            symbolSlots[i].anchoredPosition = new Vector2(0f, (middle - i) * symbolHeight);
        }
    }

    private void SetSlotSymbol(int index, SymbolData symbol)
    {
        // Guards: tell us clearly what is missing instead of a vague crash
        if (symbol == null)
        {
            Debug.LogError("Reel: symbol is NULL. Check the symbol list or the spin target.", this);
            return;
        }
        if (slotImages[index] == null)
        {
            Debug.LogError("Reel: slot " + index + " has no Image component.", this);
            return;
        }

        slotSymbols[index] = symbol;
        slotImages[index].sprite = symbol.symbolSprite;
    }
}