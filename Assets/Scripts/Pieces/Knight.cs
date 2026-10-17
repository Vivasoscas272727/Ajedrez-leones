using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Implementación del Caballo en ajedrez:
/// Movimiento en forma de 'L'. Única pieza capaz de saltar por encima de otras piezas.
/// </summary>
public class Knight : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board)
    {
        List<Vector2Int> moves = new List<Vector2Int>();

        // Las 8 posibles posiciones en 'L'
        Vector2Int[] knightOffsets = new Vector2Int[]
        {
            new Vector2Int(1, 2),
            new Vector2Int(2, 1),
            new Vector2Int(2, -1),
            new Vector2Int(1, -2),
            new Vector2Int(-1, -2),
            new Vector2Int(-2, -1),
            new Vector2Int(-2, 1),
            new Vector2Int(-1, 2)
        };

        foreach (Vector2Int offset in knightOffsets)
        {
            int targetX = currentPosition.x + offset.x;
            int targetY = currentPosition.y + offset.y;

            if (IsWithinBounds(targetX, targetY))
            {
                ChessPiece pieceAtTarget = board[targetX, targetY];
                if (pieceAtTarget == null || pieceAtTarget.pieceColor != this.pieceColor)
                {
                    moves.Add(new Vector2Int(targetX, targetY));
                }
            }
        }

        return moves;
    }
}

