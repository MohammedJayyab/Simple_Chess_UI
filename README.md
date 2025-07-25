# Simple Chess

A clean, elegant chess implementation built with C# and WPF.

## Overview

Simple Chess is a fully functional chess game with a clean, minimalist UI that follows standard chess rules. It's designed to be both beginner-friendly and feature-complete for chess enthusiasts.

## Features

- Complete implementation of chess rules
- Elegant, minimalist user interface
- Visual indicators for valid moves
- Support for special moves:
  - Castling (kingside and queenside) with standard notation (O-O and O-O-O)
  - Pawn promotion with piece selection dialog
  - En passant
- Check and checkmate detection
- Stalemate detection
- Game state tracking
- Turn-based gameplay with clear indicators
- Move history with standard algebraic notation
- Save and load functionality for games
- Undo move functionality to take back moves

## Screenshots

_[![Screenshots would go here](https://github.com/MohammedJayyab/Simple_Chess_UI/blob/master/board.png)]_

## How to Play

1. **Starting a Game**

   - Launch the application
   - White always moves first
   - Use the Game menu to start a new game, save or load a game

2. **Making a Move**

   - Click on the piece you want to move
   - Valid destination squares will be highlighted
   - Click on a valid square to move the piece
   - The move will be recorded in standard algebraic notation in the move history

3. **Special Moves**

   - **Castling**: Click the king, then click two squares to the left or right (displayed as O-O or O-O-O in notation)
   - **Pawn Promotion**: Move a pawn to the last rank, then select a piece from the dialog
   - **En Passant**: Capture an opponent pawn that has just moved two squares forward by moving your pawn diagonally behind it

4. **Game Management**

   - **Undo Move**: Select "Undo Move" from the Game menu to take back the last move
   - **Save Game**: Select "Save Game" from the Game menu to save your current game state
   - **Load Game**: Select "Load Game" from the Game menu to resume a previously saved game

5. **Game End Conditions**
   - **Checkmate**: When a king is in check and has no legal moves
   - **Stalemate**: When a player has no legal moves but their king is not in check
   - **Draw**: Not currently implemented but could include: insufficient material, threefold repetition, fifty-move rule

## Technical Details

- Built with C# and WPF
- MVVM architecture for clean separation of concerns
- Object-oriented design representing chess concepts
- Custom controls and styles for chess board and pieces

## Future Enhancements

- Game clock with time controls
- AI opponent with adjustable difficulty
- Networked play
- Visual animations for improved user experience
- Draw offers and resignation functionality
- Analysis mode with engine evaluation
- Opening book integration
- Game export in PGN format
- Chess puzzles and training exercises

## License

[Your chosen license here]

## Acknowledgements

- Chess piece images from [appropriate source]
- Sound effects from [appropriate source] (if applicable)
