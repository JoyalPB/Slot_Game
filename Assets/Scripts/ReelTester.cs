using UnityEngine;

public class ReelTester : MonoBehaviour
{
    [SerializeField] private Reel reel;
    [SerializeField] private SymbolData target;

    private void Update()
    {
        // Press SPACE to spin
        if (Input.GetKeyDown(KeyCode.Space) && !reel.IsSpinning)
            StartCoroutine(reel.SpinAndStop(2f, target));
    }
}