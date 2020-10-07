# oz_tic_tac_toe

May 2015
Simple tic-tac-toe game following Newel and Simon's 1972 strategy: https://en.wikipedia.org/wiki/Tic-tac-toe

1. Win: If the player has two in a row, they can place a third to get three in a row.
2. Block: If the opponent has two in a row, the player must play the third themselves to block the opponent.
3. Fork: Create an opportunity where the player has two threats to win (two non-blocked lines of 2).
4. Blocking an opponent's fork:
Option 1: 
The player should create two in a row to force the opponent into defending, as long as it doesn't result in them creating a fork.
Option 2: 
If there is a configuration where the opponent can fork, the player should block that fork.
4a. Prepare for a fork.
5. Center: A player marks the center.
6. Opposite corner: If the opponent is in the corner, the player plays the opposite corner.
7. Empty corner: The player plays in a corner square.
8. Empty side: The player plays in a middle square on any of the 4 sides.
