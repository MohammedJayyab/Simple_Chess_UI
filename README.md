# Simple Chess

A clean, elegant chess implementation built with C# and WPF.

## Overview

Simple Chess is a fully functional chess game with a clean, minimalist UI that follows standard chess rules. It's designed to be both beginner-friendly and feature-complete for chess enthusiasts.

## Features

- Complete implementation of chess rules
- Elegant, minimalist user interface
- Visual indicators for valid moves
- Support for special moves:
  - Castling (kingside and queenside)
  - Pawn promotion
  - En passant
- Check and checkmate detection
- Stalemate detection
- Game state tracking
- Turn-based gameplay with clear indicators

## Screenshots

*[Screenshots would go here]*

## How to Play

1. **Starting a Game**
   - Launch the application
   - White always moves first

2. **Making a Move**
   - Click on the piece you want to move
   - Valid destination squares will be highlighted
   - Click on a valid square to move the piece

3. **Special Moves**
   - **Castling**: Click the king, then click two squares to the left or right
   - **Pawn Promotion**: Move a pawn to the last rank, then select a piece from the dialog
   - **En Passant**: Capture an opponent pawn that has just moved two squares forward by moving your pawn diagonally behind it

4. **Game End Conditions**
   - **Checkmate**: When a king is in check and has no legal moves
   - **Stalemate**: When a player has no legal moves but their king is not in check
   - **Draw**: Not currently implemented but could include: insufficient material, threefold repetition, fifty-move rule

## Technical Details

- Built with C# and WPF
- MVVM architecture for clean separation of concerns
- Object-oriented design representing chess concepts
- Custom controls and styles for chess board and pieces

## Future Enhancements

- Save/load game functionality
- Game clock with time controls
- Move notation display
- Game history viewer
- AI opponent with adjustable difficulty
- Networked play
- Visual animations for improved user experience

## License

[Your chosen license here]

## Acknowledgements

- Chess piece images from [appropriate source]
- Sound effects from [appropriate source] (if applicable)
