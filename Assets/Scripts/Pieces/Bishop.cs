using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Implementación del Alfil en ajedrez:
/// Movimiento diagonal continuo a lo largo de las 4 esquinas hasta topar con el borde o una pieza.
/// </summary>
public class Bishop : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board)
    {
        List<Vector2Int> moves = new List<Vector2Int>();

        // Las 4 diagonales
        Vector2Int[] diagonalDirections = new Vector2Int[]
        {
            new Vector2Int(1, 1),
            new Vector2Int(1, -1),
            new Vector2Int(-1, 1),
            new Vector2Int(-1, -1)
        };

        foreach (Vector2Int dir in diagonalDirections)
        {
            AddRayMoves(ref board, moves, dir);
        }

        return moves;
    }
}

