using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Implementación del Rey en ajedrez (Modo Libre / Captura de Rey):
/// - Mueve 1 casilla en cualquiera de las 8 direcciones adyacentes.
/// - Puede capturar cualquier pieza enemiga (incluyendo el Rey contrario).
/// - Enroque corto y largo disponible si ni el Rey ni la Torre se han movido y el camino está despejado.
/// - Sin restricciones de Jaque: el Rey puede moverse a casillas atacadas bajo el riesgo del jugador.
/// </summary>
public class King : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board)
    {
        List<Vector2Int> moves = new List<Vector2Int>();

        // Las 8 direcciones adyacentes (1 paso)
        Vector2Int[] kingOffsets = new Vector2Int[]
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

        foreach (Vector2Int offset in kingOffsets)
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

        // Validación de Enroque (Castling) libre de restricciones de Jaque
        if (!hasMoved)
        {
            CheckCastlingMoves(ref board, moves);
        }

        return moves;
    }

    private void CheckCastlingMoves(ref ChessPiece[,] board, List<Vector2Int> moves)
    {
        int row = currentPosition.y;

        // 1. Enroque Corto (Flanco de Rey: columnas 5 y 6 vacías, Torre en columna 7 sin mover)
        if (CanCastleKingside(ref board, row))
        {
            moves.Add(new Vector2Int(6, row));
        }

        // 2. Enroque Largo (Flanco de Dama: columnas 1, 2 y 3 vacías, Torre en columna 0 sin mover)
        if (CanCastleQueenside(ref board, row))
        {
            moves.Add(new Vector2Int(2, row));
        }
    }

    private bool CanCastleKingside(ref ChessPiece[,] board, int row)
    {
        ChessPiece rook = board[7, row];
        if (rook == null || rook.pieceType != ChessPieceType.Rook || rook.pieceColor != this.pieceColor || rook.hasMoved)
        {
            return false;
        }

        // Casillas intermedias libres: 5 y 6
        return board[5, row] == null && board[6, row] == null;
    }

    private bool CanCastleQueenside(ref ChessPiece[,] board, int row)
    {
        ChessPiece rook = board[0, row];
        if (rook == null || rook.pieceType != ChessPieceType.Rook || rook.pieceColor != this.pieceColor || rook.hasMoved)
        {
            return false;
        }

        // Casillas intermedias libres: 1, 2 y 3
        return board[1, row] == null && board[2, row] == null && board[3, row] == null;
    }
}
