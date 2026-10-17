using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Colores tradicionales de las piezas de ajedrez.
/// </summary>
public enum PieceColor
{
    White,
    Black
}

/// <summary>
/// Tipos de piezas reglamentarias en ajedrez.
/// </summary>
public enum ChessPieceType
{
    Pawn,
    Rook,
    Knight,
    Bishop,
    Queen,
    King
}

/// <summary>
/// Clase base abstracta para todas las piezas de ajedrez.
/// Maneja la identidad lógica, posición en la cuadrícula y animación visual suave.
/// </summary>
public abstract class ChessPiece : MonoBehaviour
{
    [Header("Propiedades de la Pieza")]
    public PieceColor pieceColor;
    public ChessPieceType pieceType;
    public Vector2Int currentPosition;
    public bool hasMoved = false;

    [Header("Configuración Visual")]
    [SerializeField] protected float moveSpeed = 12f;
    protected Vector3 targetVisualPosition;

    protected virtual void Awake()
    {
        targetVisualPosition = transform.position;
    }

    protected virtual void Update()
    {
        // Desplazamiento visual suave hacia la casilla destino
        if (Vector3.Distance(transform.position, targetVisualPosition) > 0.001f)
        {
            transform.position = Vector3.Lerp(transform.position, targetVisualPosition, Time.deltaTime * moveSpeed);
        }
        else
        {
            transform.position = targetVisualPosition;
        }
    }

    /// <summary>
    /// Inicializa la posición lógica y visual directa al comienzo de la partida.
    /// </summary>
    public virtual void SetupPiece(PieceColor color, ChessPieceType type, Vector2Int position)
    {
        pieceColor = color;
        pieceType = type;
        currentPosition = position;
        hasMoved = false;

        targetVisualPosition = new Vector3(position.x, position.y, -1f);
        transform.position = targetVisualPosition;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingOrder = 1;
        }
    }

    /// <summary>
    /// Desplaza la pieza a una nueva coordenada de la cuadrícula.
    /// </summary>
    public virtual void MoveTo(Vector2Int newPosition, bool registerMove = true)
    {
        currentPosition = newPosition;
        targetVisualPosition = new Vector3(newPosition.x, newPosition.y, -1f);

        if (registerMove)
        {
            hasMoved = true;
        }
    }

    /// <summary>
    /// Coloca la pieza de forma instantánea sin interpolación (para reinicios o simulación).
    /// </summary>
    public virtual void SetPositionDirect(Vector2Int newPosition)
    {
        currentPosition = newPosition;
        targetVisualPosition = new Vector3(newPosition.x, newPosition.y, -1f);
        transform.position = targetVisualPosition;
    }

    /// <summary>
    /// Devuelve la lista de movimientos pseudo-legales de la pieza en el estado actual del tablero.
    /// </summary>
    public virtual List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board)
    {
        return new List<Vector2Int>();
    }

    /// <summary>
    /// Verifica si una coordenada se encuentra dentro de los límites 8x8 del tablero.
    /// </summary>
    public static bool IsWithinBounds(int x, int y)
    {
        return x >= 0 && x < 8 && y >= 0 && y < 8;
    }

    /// <summary>
    /// Sobrecarga para Vector2Int.
    /// </summary>
    public static bool IsWithinBounds(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < 8 && pos.y >= 0 && pos.y < 8;
    }

    /// <summary>
    /// Añade casillas en línea recta o diagonal hasta alcanzar un obstáculo o el borde.
    /// Utilizado por Torres, Alfiles y Reinas.
    /// </summary>
    protected void AddRayMoves(ref ChessPiece[,] board, List<Vector2Int> moves, Vector2Int direction)
    {
        int x = currentPosition.x + direction.x;
        int y = currentPosition.y + direction.y;

        while (IsWithinBounds(x, y))
        {
            ChessPiece target = board[x, y];
            if (target == null)
            {
                // Casilla vacía: movimiento válido
                moves.Add(new Vector2Int(x, y));
            }
            else
            {
                // Casilla ocupada: si es enemiga se puede capturar, pero el rayo se detiene
                if (target.pieceColor != this.pieceColor)
                {
                    moves.Add(new Vector2Int(x, y));
                }
                break;
            }

            x += direction.x;
            y += direction.y;
        }
    }
}

