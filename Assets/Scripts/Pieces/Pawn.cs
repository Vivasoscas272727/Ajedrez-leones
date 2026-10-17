using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Implementación del Peón en ajedrez:
/// - Avanza 1 casilla hacia adelante.
/// - Avanza 2 casillas en su primer movimiento si el camino está libre.
/// - Captura en diagonal 1 casilla hacia adelante.
/// - Soporte para Captura al Paso (En Passant).
/// </summary>
public class Pawn : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board)
    {
        List<Vector2Int> moves = new List<Vector2Int>();
        int forwardDirection = (pieceColor == PieceColor.White) ? 1 : -1;
        int startRow = (pieceColor == PieceColor.White) ? 1 : 6;

        // 1. Avance simple hacia adelante (1 casilla)
        int oneStepY = currentPosition.y + forwardDirection;
        if (IsWithinBounds(currentPosition.x, oneStepY))
        {
            if (board[currentPosition.x, oneStepY] == null)
            {
                moves.Add(new Vector2Int(currentPosition.x, oneStepY));

                // 2. Avance doble inicial (2 casillas)
                int twoStepY = currentPosition.y + (2 * forwardDirection);
                if (!hasMoved && currentPosition.y == startRow && IsWithinBounds(currentPosition.x, twoStepY))
                {
                    if (board[currentPosition.x, twoStepY] == null)
                    {
                        moves.Add(new Vector2Int(currentPosition.x, twoStepY));
                    }
                }
            }
        }

        // 3. Capturas diagonales convencionales
        int[] diagonalOffsets = new int[] { -1, 1 };
        foreach (int dx in diagonalOffsets)
        {
            int targetX = currentPosition.x + dx;
            int targetY = currentPosition.y + forwardDirection;

            if (IsWithinBounds(targetX, targetY))
            {
                ChessPiece pieceAtTarget = board[targetX, targetY];

                // Captura regular: casilla ocupada por enemigo
                if (pieceAtTarget != null && pieceAtTarget.pieceColor != this.pieceColor)
                {
                    moves.Add(new Vector2Int(targetX, targetY));
                }
                // Captura al paso (En Passant): casilla vacía pero marcada como vulnerable
                else if (pieceAtTarget == null && BoardManager.Instance != null && BoardManager.Instance.EnPassantTile.HasValue)
                {
                    if (BoardManager.Instance.EnPassantTile.Value == new Vector2Int(targetX, targetY))
                    {
                        moves.Add(new Vector2Int(targetX, targetY));
                    }
                }
            }
        }

        return moves;
    }
}

