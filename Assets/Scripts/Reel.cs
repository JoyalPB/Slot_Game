using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls ONE reel: endless spin, then smooth stop on a chosen symbol.
/// </summary>
public class Reel : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private RectTransform[] symbolSlots;  // The child Images (drag them in order, top to bottom)
    [SerializeField] private SymbolData[] allSymbols;      // The 4 symbol data files
    [SerializeField] private float symbolHeight = 100f;    // Height of one symbol + gap

    [Header("Spin Feel")]
    [SerializeField] private float spinSpeed = 1500f;      // Pixels per second
    [SerializeField] private float slowDownTime = 0.6f;    // Time to ease to a stop

    /// <summary>The symbol sitting in the middle of the window when stopped.</summary>
    public SymbolData CurrentSymbol { get; private set; }

    public bool IsSpinning { get; private set; }

    private Image[] slotImages;
    private SymbolData[] slotSymbols;   // Which symbol each slot is showing

    private void Awake()
    {
        // Cache the Image of each slot, and give each slot a starting symbol
        slotImages = new Image[symbolSlots.Length];
        slotSymbols = new SymbolData[symbolSlots.Length];

        for (int i = 0; i < symbolSlots.Length; i++)
        {
            slotImages[i] = symbolSlots[i].GetComponent<Image>();
            SetSlotSymbol(i, allSymbols[i % allSymbols.Length]);
        }

        PlaceSlotsInStack();
        CurrentSymbol = slotSymbols[1];
    }

    /// <summary>Start spinning and stop later on 'target'.</summary>
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

        // 2) Decide the final picture BEFORE the stop animation.
        //    The slot that will land in the middle gets the target symbol.
        yield return StartCoroutine(SlowDownAndLand(target));

        IsSpinning = false;
    }

    private IEnumerator SlowDownAndLand(SymbolData target)
    {
        // Slow down smoothly: speed goes from spinSpeed to 0
        float timer = 0f;
        while (timer < slowDownTime)
        {
            float t = timer / slowDownTime;
            float currentSpeed = Mathf.Lerp(spinSpeed, spinSpeed * 0.15f, t);
            MoveSlots(currentSpeed * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }

        // Find the slot closest to the middle line (y = 0)
        int middleIndex = FindSlotClosestToMiddle();
        SetSlotSymbol(middleIndex, target);

        // Smoothly slide everything so that slot sits exactly at y = 0
        float offset = -symbolSlots[middleIndex].anchoredPosition.y;
        float moved = 0f;
        while (Mathf.Abs(offset - moved) > 0.5f)
        {
            float step = Mathf.MoveTowards(moved, offset, spinSpeed * 0.15f * Time.deltaTime) - moved;
            MoveSlots(step);
            moved += step;
            yield return null;
        }

        CurrentSymbol = target;
    }

    /// <summary>Move all slots down by 'amount'. Wrap slots that fall off the bottom to the top.</summary>
    private void MoveSlots(float amount)
    {
        float totalHeight = symbolHeight * symbolSlots.Length;
        float bottomLimit = -totalHeight / 2f;

        foreach (RectTransform slot in symbolSlots)
        {
            Vector2 pos = slot.anchoredPosition;
            pos.y -= amount;

            // Fell below the bottom? Teleport to the top and show a random picture
            if (pos.y < bottomLimit + symbolHeight / 2f - symbolHeight)
            {
                pos.y += totalHeight;
                int index = System.Array.IndexOf(symbolSlots, slot);
                SetSlotSymbol(index, allSymbols[Random.Range(0, allSymbols.Length)]);
            }

            slot.anchoredPosition = pos;
        }
    }

    private int FindSlotClosestToMiddle()
    {
        int best = 0;
        float bestDistance = float.MaxValue;
        for (int i = 0; i < symbolSlots.Length; i++)
        {
            float distance = Mathf.Abs(symbolSlots[i].anchoredPosition.y);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = i;
            }
        }
        return best;
    }

    private void SetSlotSymbol(int index, SymbolData symbol)
    {
        slotSymbols[index] = symbol;
        slotImages[index].sprite = symbol.symbolSprite;
    }

    /// <summary>Line the slots up in a vertical stack, centered on y = 0.</summary>
    private void PlaceSlotsInStack()
    {
        float startY = (symbolSlots.Length - 1) * symbolHeight / 2f;
        for (int i = 0; i < symbolSlots.Length; i++)
        {
            symbolSlots[i].anchoredPosition = new Vector2(0f, startY - i * symbolHeight);
        }
    }
}