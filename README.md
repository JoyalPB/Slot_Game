# Slot Game (Unity)

A pixel-art slot machine game built in Unity for the Unity Slot Game Assignment.

**Play in browser:** https://joyalpb.github.io/Slot_Game/

## Game Overview

Pick a bet, pull the lever, and watch three reels spin. Land the same symbol on all three reels to win.
The player starts with 100 coins. When the coins run out, a Game Over popup offers a restart.

**Symbols and payouts (win = bet x multiplier):**

| Symbol | Multiplier | Weight (rarity) | Chance of 3 in a row |
|---|---|---|---|
| Cherry | 5 | 50 | 12.50% |
| Bell | 10 | 28 | 2.20% |
| Bar | 20 | 15 | 0.34% |
| Seven | 50 | 7 | 0.03% |

About 15% of spins win. The expected return to the player is about 93%.

## How to Run

**Option 1 (easiest):** open the play link above.

**Option 2 (local WebGL build):**
1. Open a terminal in `Build/WebGL`.
2. Run `python -m http.server 8000`.
3. Open `http://localhost:8000` in a browser.

Opening `index.html` by double-click does not work, because browsers block it.

**Option 3 (Unity):** open the project in Unity 6000.5.0f1, open `Assets/Scenes/Main`, press Play.

## Controls

- Click **Bet 10G / 50G / 100G** to choose the bet. The chosen bet turns yellow.
- Click the **lever** to spin.
- The Exit button works in the editor and is hidden in the WebGL build.

## Bonus Features

- Lever pull animation (full lever swaps to a pulled-down sprite)
- Weighted symbol rarity (RNG)
- Party popper confetti particles on win
- Twinkling background sparkle particles
- Win board popup with a pop animation
- Sound: spin loop, win, button click, game over, background ambience
- Game Over popup with restart

## Thought Process

**Structure (OOP, one job per script):**

| Script | Job |
|---|---|
| `SymbolData` | ScriptableObject: name, sprite, payout, weight |
| `Reel` | Spins one reel and stops on a chosen symbol |
| `SlotMachine` | Holds the bet, picks results (RNG), runs the spin, checks the win |
| `PayoutManager` | Coins, bets, winnings |
| `UIManager` | Screen only: coins, buttons, win board, game over |
| `LeverController` | Lever click and pull visuals |
| `AudioManager` | All sounds in one place |
| `PartyPopper` | Confetti on win |

Scripts talk through C# events (for example `OnCoinsChanged`, `OnSpinFinished`), so the game logic never depends on the UI.

**RNG:** the result is picked before the animation, with a weighted random roll. The reels then animate toward the chosen result, so the outcome is independent of the animation. Rare symbols have a lower weight and a higher payout.

**Reel animation:** each reel has 6 symbol slots that move down and wrap to the top. The final symbol is given to a slot while it is hidden above the window. That slot then slides down and eases to a stop, so the player never sees a picture change at the end. Reels stop one after another.

**Balance:** I first used weights 40/30/20/10, which gave only a 10% win rate and felt too rare. I raised the Cherry weight and tuned the payouts. The win rate is now about 15% and the return to player is about 93%, similar to a real machine.

**Challenges:** reel alignment and spacing, the symbol "pop" at landing, and the lever animation (solved with two sprite objects).

## Credits

**Art assets:** provided by the assignment.

**Sounds:** all from [Pixabay](https://pixabay.com/sound-effects/), under the Pixabay Content License.
- `slot-machine-reels-sound-30276.mp3` (spin loop)
- `cash-register-purchase-87313.mp3` (win)
- `mouse-click-352734.mp3` (button click)
- `game-over-arcade-6435.mp3` (game over)
- `casino-ambiance-19130.mp3` (background ambience)

## Project Structure

    Assets/
      Scripts/  Prefabs/  Animations/  UI/  Sounds/  Sprites/  Data/  Scenes/
    Build/WebGL/
