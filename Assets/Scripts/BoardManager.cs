using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gestiona la cuadrícula lógica del tablero de ajedrez (8x8) y la visualización de casillas y piezas.
/// Opera con reglas de ajedrez libre/punitivo: sin simulación de Jaques y con victoria por Captura del Rey.
/// </summary>
public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance { get; private set; }

    [Header("Matriz Lógica del Tablero")]
    public ChessPiece[,] board = new ChessPiece[8, 8];

    [Header("Prefabs de Casillas y Temas de Color")]
    public GameObject casillaPrefab;
    public Color colorClaro = new Color(0.85f, 0.85f, 0.85f);
    public Color colorOscuro = new Color(0.3f, 0.4f, 0.5f);
    public Color colorSeleccion = new Color(0.9f, 0.8f, 0.2f);       // Amarillo
    public Color colorMovimientoValido = new Color(0.4f, 0.8f, 0.4f); // Verde suave

    [Header("Prefabs Piezas Blancas / Doradas (Fila 0 y 1)")]
    public GameObject peonBlanco;
    public GameObject torreBlanca;
    public GameObject caballoBlanco;
    public GameObject alfilBlanco;
    public GameObject reinaBlanca;
    public GameObject reyBlanco;

    [Header("Prefabs Piezas Negras / Azules (Fila 6 y 7)")]
    public GameObject peonAzul;
    public GameObject torreAzul;
    public GameObject caballoAzul;
    public GameObject alfilAzul;
    public GameObject reinaAzul;
    public GameObject reyAzul;

    // Referencias a los renderers visuales de las casillas
    protected SpriteRenderer[,] casillasRenderers = new SpriteRenderer[8, 8];

    /// <summary>
    /// Casilla vulnerable a captura al paso durante el turno actual (null si ninguna).
    /// </summary>
    public Vector2Int? EnPassantTile { get; set; } = null;

    protected virtual void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    /// <summary>
    /// Genera la cuadrícula visual de casillas 8x8 y centra la cámara si es la principal.
    /// </summary>
    public void GenerarCuadriculaVisual()
    {
        if (casillaPrefab == null)
        {
            Debug.LogWarning("[BoardManager] No se asignó el casillaPrefab.");
            return;
        }

        // Limpiar casillas previas hijas
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                Transform prev = transform.Find($"Casilla {x},{y}");
                if (prev != null)
                {
                    Destroy(prev.gameObject);
                }
            }
        }

        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                Vector2 posicion = new Vector2(x, y);
                GameObject nuevaCasilla = Instantiate(casillaPrefab, posicion, Quaternion.identity);
                nuevaCasilla.name = $"Casilla {x},{y}";
                nuevaCasilla.transform.parent = this.transform;

                SpriteRenderer renderizador = nuevaCasilla.GetComponent<SpriteRenderer>();
                casillasRenderers[x, y] = renderizador;
                RestaurarColorCasilla(x, y);
            }
        }

        if (Camera.main != null)
        {
            Camera.main.transform.position = new Vector3(3.5f, 3.5f, -10f);
        }
    }

    /// <summary>
    /// Limpia todas las piezas activas en la escena y restablece la matriz lógica.
    /// </summary>
    public void LimpiarTablero()
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                if (board[x, y] != null)
                {
                    Destroy(board[x, y].gameObject);
                    board[x, y] = null;
                }
            }
        }

        EnPassantTile = null;
    }

    /// <summary>
    /// Instancia el conjunto reglamentario de 32 piezas en sus posiciones iniciales.
    /// </summary>
    public void ColocarPiezasIniciales()
    {
        LimpiarTablero();

        // Peones
        for (int x = 0; x < 8; x++)
        {
            InstanciarPieza<Pawn>(peonBlanco, PieceColor.White, ChessPieceType.Pawn, x, 1);
            InstanciarPieza<Pawn>(peonAzul, PieceColor.Black, ChessPieceType.Pawn, x, 6);
        }

        // Torres
        InstanciarPieza<Rook>(torreBlanca, PieceColor.White, ChessPieceType.Rook, 0, 0);
        InstanciarPieza<Rook>(torreBlanca, PieceColor.White, ChessPieceType.Rook, 7, 0);
        InstanciarPieza<Rook>(torreAzul, PieceColor.Black, ChessPieceType.Rook, 0, 7);
        InstanciarPieza<Rook>(torreAzul, PieceColor.Black, ChessPieceType.Rook, 7, 7);

        // Caballos
        InstanciarPieza<Knight>(caballoBlanco, PieceColor.White, ChessPieceType.Knight, 1, 0);
        InstanciarPieza<Knight>(caballoBlanco, PieceColor.White, ChessPieceType.Knight, 6, 0);
        InstanciarPieza<Knight>(caballoAzul, PieceColor.Black, ChessPieceType.Knight, 1, 7);
        InstanciarPieza<Knight>(caballoAzul, PieceColor.Black, ChessPieceType.Knight, 6, 7);

        // Alfiles
        InstanciarPieza<Bishop>(alfilBlanco, PieceColor.White, ChessPieceType.Bishop, 2, 0);
        InstanciarPieza<Bishop>(alfilBlanco, PieceColor.White, ChessPieceType.Bishop, 5, 0);
        InstanciarPieza<Bishop>(alfilAzul, PieceColor.Black, ChessPieceType.Bishop, 2, 7);
        InstanciarPieza<Bishop>(alfilAzul, PieceColor.Black, ChessPieceType.Bishop, 5, 7);

        // Reinas
        InstanciarPieza<Queen>(reinaBlanca, PieceColor.White, ChessPieceType.Queen, 3, 0);
        InstanciarPieza<Queen>(reinaAzul, PieceColor.Black, ChessPieceType.Queen, 3, 7);

        // Reyes
        InstanciarPieza<King>(reyBlanco, PieceColor.White, ChessPieceType.King, 4, 0);
        InstanciarPieza<King>(reyAzul, PieceColor.Black, ChessPieceType.King, 4, 7);

        ActualizarColoresTablero();
    }

    private T InstanciarPieza<T>(GameObject prefab, PieceColor color, ChessPieceType tipo, int x, int y) where T : ChessPiece
    {
        if (prefab == null) return null;

        GameObject obj = Instantiate(prefab);
        obj.name = $"{tipo}_{(color == PieceColor.White ? "Blanco" : "Negro")}_{x}_{y}";

        T componente = obj.GetComponent<T>();
        if (componente == null)
        {
            componente = obj.AddComponent<T>();
        }

        componente.SetupPiece(color, tipo, new Vector2Int(x, y));
        board[x, y] = componente;
        return componente;
    }

    public ChessPiece GetPieceAt(Vector2Int pos)
    {
        if (!ChessPiece.IsWithinBounds(pos)) return null;
        return board[pos.x, pos.y];
    }

    public ChessPiece GetPieceAt(int x, int y)
    {
        if (!ChessPiece.IsWithinBounds(x, y)) return null;
        return board[x, y];
    }

    /// <summary>
    /// Retorna los movimientos válidos según el patrón geométrico de la pieza.
    /// En este modo punitivo y libre, no se aplican filtros de simulación ni restricciones de Jaque.
    /// </summary>
    public List<Vector2Int> GetLegalMoves(ChessPiece piece)
    {
        if (piece == null) return new List<Vector2Int>();
        return piece.GetAvailableMoves(ref board);
    }

    /// <summary>
    /// Ejecuta de forma definitiva un movimiento en el tablero:
    /// Resuelve capturas regulares (incluyendo el Rey para ganar), captura al paso, enroque y coronación.
    /// Devuelve true si la pieza capturada fue un Rey.
    /// </summary>
    public bool ExecuteMove(ChessPiece piece, Vector2Int targetPos)
    {
        Vector2Int origin = piece.currentPosition;
        int dx = targetPos.x - origin.x;
        int dy = targetPos.y - origin.y;
        bool fueCapturadoRey = false;

        // --- 1. GESTIÓN DE EN PASSANT ---
        bool wasEnPassantCapture = false;
        if (piece is Pawn && board[targetPos.x, targetPos.y] == null && EnPassantTile.HasValue && targetPos == EnPassantTile.Value)
        {
            wasEnPassantCapture = true;
            Vector2Int enemyPawnPos = new Vector2Int(targetPos.x, origin.y);
            ChessPiece capturedPawn = board[enemyPawnPos.x, enemyPawnPos.y];
            if (capturedPawn != null)
            {
                Destroy(capturedPawn.gameObject);
                board[enemyPawnPos.x, enemyPawnPos.y] = null;
            }
        }

        Vector2Int? nextEnPassantTile = null;
        if (piece is Pawn && Mathf.Abs(dy) == 2)
        {
            nextEnPassantTile = new Vector2Int(origin.x, origin.y + (dy / 2));
        }

        // --- 2. GESTIÓN DE ENROQUE (Mover la Torre complementaria) ---
        if (piece is King && Mathf.Abs(dx) == 2)
        {
            int row = origin.y;
            if (targetPos.x == 6) // Enroque Corto: Torre va de 7 a 5
            {
                ChessPiece rook = board[7, row];
                if (rook != null)
                {
                    board[7, row] = null;
                    board[5, row] = rook;
                    rook.MoveTo(new Vector2Int(5, row), true);
                }
            }
            else if (targetPos.x == 2) // Enroque Largo: Torre va de 0 a 3
            {
                ChessPiece rook = board[0, row];
                if (rook != null)
                {
                    board[0, row] = null;
                    board[3, row] = rook;
                    rook.MoveTo(new Vector2Int(3, row), true);
                }
            }
        }

        // --- 3. CAPTURA REGULAR (INCLUYENDO REY) ---
        ChessPiece destinationPiece = board[targetPos.x, targetPos.y];
        if (destinationPiece != null && !wasEnPassantCapture)
        {
            // Si la pieza a eliminar es el Rey enemigo, registramos la condición de victoria inmediata
            if (destinationPiece.pieceType == ChessPieceType.King || destinationPiece is King)
            {
                fueCapturadoRey = true;
            }

            Destroy(destinationPiece.gameObject);
        }

        // Mover pieza principal en matriz lógica y transform
        board[origin.x, origin.y] = null;
        board[targetPos.x, targetPos.y] = piece;
        piece.MoveTo(targetPos, true);

        // Actualizar casilla de En Passant
        EnPassantTile = nextEnPassantTile;

        // --- 4. CORONACIÓN / PROMOCIÓN DEL PEÓN ---
        if (piece is Pawn && (targetPos.y == 7 || targetPos.y == 0))
        {
            PromotePawnToQueen(piece, targetPos);
        }

        return fueCapturadoRey;
    }

    /// <summary>
    /// Transforma automáticamente al Peón en Reina al alcanzar la última fila.
    /// </summary>
    private void PromotePawnToQueen(ChessPiece pawn, Vector2Int pos)
    {
        PieceColor color = pawn.pieceColor;
        GameObject queenPrefab = (color == PieceColor.White) ? reinaBlanca : reinaAzul;

        Destroy(pawn.gameObject);
        board[pos.x, pos.y] = null;

        if (queenPrefab != null)
        {
            InstanciarPieza<Queen>(queenPrefab, color, ChessPieceType.Queen, pos.x, pos.y);
        }
    }

    // --- MÉTODOS DE COLORES Y SELECCIÓN VISUAL DEL TABLERO ---

    public void RestaurarColorCasilla(int x, int y)
    {
        if (casillasRenderers[x, y] == null) return;
        bool esClara = (x + y) % 2 != 0;
        casillasRenderers[x, y].color = esClara ? colorClaro : colorOscuro;
    }

    public void ActualizarColoresTablero()
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                RestaurarColorCasilla(x, y);
            }
        }
    }

    public void ResaltarCasilla(Vector2Int pos, Color col)
    {
        if (ChessPiece.IsWithinBounds(pos) && casillasRenderers[pos.x, pos.y] != null)
        {
            casillasRenderers[pos.x, pos.y].color = col;
        }
    }

    public void ResaltarMovimientosValidos(List<Vector2Int> moves)
    {
        foreach (Vector2Int move in moves)
        {
            ResaltarCasilla(move, colorMovimientoValido);
        }
    }

    public void CambiarTema(Color nuevoClaro, Color nuevoOscuro)
    {
        colorClaro = nuevoClaro;
        colorOscuro = nuevoOscuro;
        ActualizarColoresTablero();
    }
}
