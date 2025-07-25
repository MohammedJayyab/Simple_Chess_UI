using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using SimpleChess.Commands;
using SimpleChess.Models;

namespace SimpleChess.ViewModels
{
    public partial class MainViewModel
    {
        // Command properties
        public ICommand NewGameCommand { get; private set; }

        public ICommand SaveGameCommand { get; private set; }
        public ICommand LoadGameCommand { get; private set; }
        public ICommand UndoMoveCommand { get; private set; }
        public ICommand ExitCommand { get; private set; }

        // List to track moves for save/load functionality
        private List<MoveRecord> _savedMoves = new List<MoveRecord>();

        // Stack to track board states for undo functionality
        private Stack<BoardState> _undoStack = new Stack<BoardState>();

        // Class to store the complete board state for undo
        private class BoardState
        {
            public List<PieceState> Pieces { get; set; } = new List<PieceState>();
            public PlayerColor CurrentPlayer { get; set; }
            public GameState GameState { get; set; }
            public List<MoveRecord> SavedMoves { get; set; }
            public ObservableCollection<NotationRecord> MoveHistory { get; set; }
        }

        // Class to store piece information
        private class PieceState
        {
            public PieceType Type { get; set; }
            public PlayerColor Color { get; set; }
            public Position Position { get; set; }
            public bool HasMoved { get; set; }
        }

        private void InitializeCommands()
        {
            NewGameCommand = new RelayCommand(_ => NewGame());
            SaveGameCommand = new RelayCommand(_ => SaveGame());
            LoadGameCommand = new RelayCommand(_ => LoadGame());
            UndoMoveCommand = new RelayCommand(_ => UndoMove(), _ => _undoStack.Count > 0);
            ExitCommand = new RelayCommand(_ => Application.Current.Shutdown());
        }

        private void NewGame()
        {
            // Show confirmation dialog
            MessageBoxResult result = MessageBox.Show(
                "Are you sure you want to start a new game? Current progress will be lost.",
                "Confirm New Game",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Clear all pieces from the board first
                foreach (var square in Squares)
                {
                    square.Piece = null;
                    square.IsSelected = false;
                    square.IsValidMove = false;
                }

                // Reset the board
                InitializeBoard();
                _currentPlayer = PlayerColor.White;
                _savedMoves.Clear();
                GameState = GameState.InProgress;
                
                // Clear move history
                _moveHistory = new MoveHistory();
                OnPropertyChanged(nameof(MoveHistory));
                
                // Clear undo stack
                _undoStack.Clear();
                
                StatusMessage = "White's turn to move";

                // Notify UI that game state has changed
                OnPropertyChanged(nameof(Squares));
                
                // Command manager needs to be notified that CanExecute state might have changed
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private void SaveGame()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Chess Game (*.chess)|*.chess",
                Title = "Save Chess Game"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                // Create game state to save
                var gameState = new GameSaveState
                {
                    CurrentPlayer = _currentPlayer,
                    GameStatus = GameState,
                    Moves = _savedMoves
                };

                // Serialize and save
                string jsonString = JsonSerializer.Serialize(gameState);
                File.WriteAllText(saveFileDialog.FileName, jsonString);

                StatusMessage = "Game saved successfully!";
            }
        }

        private void LoadGame()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Chess Game (*.chess)|*.chess",
                Title = "Load Chess Game"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    // Load the saved game
                    string jsonString = File.ReadAllText(openFileDialog.FileName);
                    var gameState = JsonSerializer.Deserialize<GameSaveState>(jsonString);

                    // Reset to initial state
                    InitializeBoard();

                    // Clear move history
                    _moveHistory = new MoveHistory();

                    // Temporarily disable saving to avoid duplicates
                    var tempSavedMoves = _savedMoves;
                    _savedMoves = new List<MoveRecord>();

                    // Replay all moves
                    foreach (var move in gameState.Moves)
                    {
                        Square fromSquare = GetSquareAt(move.FromRow, move.FromColumn);
                        Square toSquare = GetSquareAt(move.ToRow, move.ToColumn);

                        // Record original piece info for notation
                        PlayerColor movingColor = fromSquare.Piece.Color;
                        PieceType movingType = fromSquare.Piece.Type;
                        Position fromPos = fromSquare.Position;
                        Position toPos = toSquare.Position;
                        bool isCapture = toSquare.Piece != null;

                        // Generate notation
                        string notation = GenerateMoveNotation(movingColor, movingType, fromPos, toPos, isCapture);

                        // Execute the move - NOTE: This moves the piece on the board
                        _ = MovePiece(fromSquare, toSquare);

                        // Notify UI to update the board after each move
                        OnPropertyChanged(nameof(Squares));

                        // Add to appropriate column in notation history
                        if (movingColor == PlayerColor.White)
                        {
                            // Add white move
                            _moveHistory.Moves.Add(new NotationRecord
                            {
                                MoveNumber = _moveHistory.Moves.Count + 1,
                                WhiteMove = notation
                            });
                        }
                        else // Black move
                        {
                            // Find the last record or create a new one
                            if (_moveHistory.Moves.Count > 0)
                            {
                                _moveHistory.Moves[_moveHistory.Moves.Count - 1].BlackMove = notation;
                            }
                            else
                            {
                                _moveHistory.Moves.Add(new NotationRecord
                                {
                                    MoveNumber = 1,
                                    BlackMove = notation
                                });
                            }
                        }
                    }

                    // Restore saved moves from file
                    _savedMoves = new List<MoveRecord>(gameState.Moves);

                    // Restore game state
                    _currentPlayer = gameState.CurrentPlayer;
                    GameState = gameState.GameStatus;

                    // Notify about UI changes
                    OnPropertyChanged(nameof(MoveHistory));
                    OnPropertyChanged(nameof(Squares));

                    // Update status message based on whose turn it is and game state
                    UpdateStatusMessage();

                    UpdateStatusMessage();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Error loading game: {ex.Message}",
                        "Load Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        private void RecordMove(Square from, Square to)
        {
            // Safety check to prevent null reference exceptions
            if (from?.Piece == null)
            {
                return; // Cannot record move without a piece
            }

            // Save the current board state for undo before making the move
            SaveBoardState();

            _savedMoves.Add(new MoveRecord
            {
                FromRow = from.Position.Row,
                FromColumn = from.Position.Column,
                ToRow = to.Position.Row,
                ToColumn = to.Position.Column,
                PieceType = from.Piece.Type, // Use from.Piece instead of to.Piece
                PlayerColor = from.Piece.Color // Use from.Piece instead of to.Piece
            });
        }

        // Save the current board state for undoing moves
        private void SaveBoardState()
        {
            var boardState = new BoardState
            {
                CurrentPlayer = _currentPlayer,
                GameState = GameState,
                SavedMoves = new List<MoveRecord>(_savedMoves),
                MoveHistory = CloneMoveHistory()
            };

            // Capture all pieces on the board
            foreach (var square in Squares)
            {
                if (square.Piece != null)
                {
                    boardState.Pieces.Add(new PieceState
                    {
                        Type = square.Piece.Type,
                        Color = square.Piece.Color,
                        Position = new Position(square.Position.Row, square.Position.Column),
                        HasMoved = square.Piece.HasMoved
                    });
                }
            }

            _undoStack.Push(boardState);

            // Command manager needs to be notified that CanExecute state might have changed
            CommandManager.InvalidateRequerySuggested();
        }

        // Clone the current move history
        private ObservableCollection<NotationRecord> CloneMoveHistory()
        {
            var clonedHistory = new ObservableCollection<NotationRecord>();
            foreach (var record in _moveHistory.Moves)
            {
                clonedHistory.Add(new NotationRecord
                {
                    MoveNumber = record.MoveNumber,
                    WhiteMove = record.WhiteMove,
                    BlackMove = record.BlackMove
                });
            }
            return clonedHistory;
        }

        // Undo the last move
        private void UndoMove()
        {
            if (_undoStack.Count == 0)
            {
                return;
            }

            var previousState = _undoStack.Pop();

            // Clear the board first
            foreach (var square in Squares)
            {
                square.Piece = null;
            }

            // Restore all pieces to their previous positions
            foreach (var pieceState in previousState.Pieces)
            {
                var square = GetSquareAt(pieceState.Position.Row, pieceState.Position.Column);
                square.Piece = new Piece(pieceState.Type, pieceState.Color, pieceState.Position)
                {
                    HasMoved = pieceState.HasMoved
                };
            }

            // Restore game state
            _currentPlayer = previousState.CurrentPlayer;
            GameState = previousState.GameState;
            _savedMoves = previousState.SavedMoves;

            // Restore move history
            _moveHistory = new MoveHistory();
            foreach (var record in previousState.MoveHistory)
            {
                _moveHistory.Moves.Add(record);
            }

            // Update UI
            OnPropertyChanged(nameof(Squares));
            OnPropertyChanged(nameof(MoveHistory));
            UpdateStatusMessage();

            // Command manager needs to be notified that CanExecute state might have changed
            CommandManager.InvalidateRequerySuggested();
        }

        private void UpdateStatusMessage()
        {
            switch (GameState)
            {
                case GameState.Check:
                    StatusMessage = $"{_currentPlayer} is in check!";
                    break;

                case GameState.Checkmate:
                    StatusMessage = $"Checkmate! {(_currentPlayer == PlayerColor.White ? "Black" : "White")} wins!";
                    break;

                case GameState.Stalemate:
                    StatusMessage = "Stalemate! The game is a draw.";
                    break;

                default:
                    StatusMessage = $"{_currentPlayer}'s turn to move";
                    break;
            }
        }
    }
}