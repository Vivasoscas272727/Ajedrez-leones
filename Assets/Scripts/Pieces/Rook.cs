using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Implementación de la Torre en ajedrez:
/// Movimiento continuo horizontal y vertical en 4 direcciones hasta topar con el borde o una pieza.
/// </summary>
public class Rook : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board)
    {
        List<Vector2Int> moves = new List<Vector2Int>();

        // Las cuatro direcciones ortogonales: Arriba, Abajo, Derecha, Izquierda
        Vector2Int[] orthogonalDirections = new Vector2Int[]
        {
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0)
        };

        foreach (Vector2Int dir in orthogonalDirections)
        {
            AddRayMoves(ref board, moves, dir);
        }

        return moves;
    }
}

