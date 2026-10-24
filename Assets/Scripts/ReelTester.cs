using System.Collections;
using UnityEngine;

public class ReelTester : MonoBehaviour
{
    [SerializeField] private Reel[] reels;        // Size 3: Reel_1, Reel_2, Reel_3
    [SerializeField] private SymbolData target;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !reels[0].IsSpinning)
        {
            for (int i = 0; i < reels.Length; i++)
            {
                // Each reel spins longer than the one before: 1.5s, 2.5s, 3.5s
                StartCoroutine(reels[i].SpinAndStop(1.5f + i, target));
            }
        }
    }
}