using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Implementación de la Reina en ajedrez:
/// Combina el rango completo de la Torre y el Alfil (8 direcciones continuas).
/// </summary>
public class Queen : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board)
    {
        List<Vector2Int> moves = new List<Vector2Int>();

        // Las 8 direcciones posibles (ortogonales y diagonales)
        Vector2Int[] queenDirections = new Vector2Int[]
        {
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(1, 1),
            new Vector2Int(1, -1),
            new Vector2Int(-1, 1),
            new Vector2Int(-1, -1)
        };

        foreach (Vector2Int dir in queenDirections)
        {
            AddRayMoves(ref board, moves, dir);
        }

        return moves;
    }
}

