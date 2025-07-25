using System;
using System.Collections.Generic;
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
        public ICommand ExitCommand { get; private set; }

        // List to track moves for save/load functionality
        private List<MoveRecord> _moveHistory = new List<MoveRecord>();

        private void InitializeCommands()
        {
            NewGameCommand = new RelayCommand(_ => NewGame());
            SaveGameCommand = new RelayCommand(_ => SaveGame());
            LoadGameCommand = new RelayCommand(_ => LoadGame());
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
                _moveHistory.Clear();
                GameState = GameState.InProgress;
                StatusMessage = "White's turn to move";

                // Notify UI that game state has changed
                OnPropertyChanged(nameof(Squares));
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
                    Moves = _moveHistory
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

                    // Replay all moves
                    foreach (var move in gameState.Moves)
                    {
                        Square fromSquare = GetSquareAt(move.FromRow, move.FromColumn);
                        Square toSquare = GetSquareAt(move.ToRow, move.ToColumn);

                        // Execute the move
                        MovePiece(fromSquare, toSquare);
                    }

                    // Restore game state
                    _currentPlayer = gameState.CurrentPlayer;
                    GameState = gameState.GameStatus;
                    _moveHistory = new List<MoveRecord>(gameState.Moves);

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
            _moveHistory.Add(new MoveRecord
            {
                FromRow = from.Position.Row,
                FromColumn = from.Position.Column,
                ToRow = to.Position.Row,
                ToColumn = to.Position.Column,
                PieceType = to.Piece.Type,
                PlayerColor = to.Piece.Color
            });
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