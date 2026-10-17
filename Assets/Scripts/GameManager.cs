using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

/// <summary>
/// Gestor principal del flujo de juego bajo la regla de Captura de Rey (Modo Libre / Punitivo):
/// - Control y alternancia de turnos (Blancas / Negras).
/// - Selección y movimientos basados en los patrones de cada pieza, sin filtros restrictivos de Jaque.
/// - Nueva condición de victoria inmediata: captura y eliminación del Rey enemigo.
/// - Despliegue del panel modal de Fin de Partida en el Canvas minimalista al capturar al Rey.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Control de Turnos y Estado")]
    [SerializeField] private PieceColor currentTurn = PieceColor.White;
    private bool juegoTerminado = false;
    private string mensajePantalla = "";

    // Selección de pieza actual
    private ChessPiece selectedPiece = null;
    private List<Vector2Int> currentLegalMoves = new List<Vector2Int>();

    // Propiedades públicas para sincronización con HUD y Menús
    public PieceColor CurrentTurn => currentTurn;
    public bool EsTurnoBlancas => currentTurn == PieceColor.White;
    public bool JuegoTerminado => juegoTerminado;
    public string MensajePantalla => mensajePantalla;

    private void Awake()
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

    private void Start()
    {
        ActualizarVisualesTablero();
    }

    private void Update()
    {
        // 1. Si algún menú modal (Principal, Pausa, Opciones) está abierto, bloquear tablero
        if (MenuPrincipal.Instance != null && MenuPrincipal.Instance.EstaMenuAbierto())
            return;

        // 2. Si la partida concluyó por Captura de Rey, no permitir más jugadas
        if (juegoTerminado)
            return;

        // 3. Procesar clics del ratón usando el nuevo Input System
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            // Evitar interacción si el cursor está sobre un elemento interactivo del Canvas (botones)
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            Vector2 mousePos = Mouse.current.position.ReadValue();
            ProcesarClicEnTablero(mousePos);
        }
    }

    /// <summary>
    /// Convierte la posición de pantalla a coordenadas del tablero y gestiona la selección libre.
    /// </summary>
    private void ProcesarClicEnTablero(Vector2 mouseScreenPos)
    {
        if (Camera.main == null || BoardManager.Instance == null) return;

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, 10f));
        int x = Mathf.RoundToInt(worldPos.x);
        int y = Mathf.RoundToInt(worldPos.y);

        // Clic fuera de los límites del tablero: deseleccionar limpiamente
        if (!ChessPiece.IsWithinBounds(x, y))
        {
            DeseleccionarPieza();
            return;
        }

        Vector2Int clickedCoord = new Vector2Int(x, y);
        ChessPiece pieceAtTile = BoardManager.Instance.GetPieceAt(clickedCoord);

        // CASO 1: No hay pieza seleccionada todavía
        if (selectedPiece == null)
        {
            if (pieceAtTile != null && pieceAtTile.pieceColor == currentTurn)
            {
                SeleccionarPieza(pieceAtTile);
            }
        }
        // CASO 2: Ya existe una pieza seleccionada
        else
        {
            // Clic en otra pieza aliada: cambiar selección
            if (pieceAtTile != null && pieceAtTile.pieceColor == currentTurn)
            {
                SeleccionarPieza(pieceAtTile);
                return;
            }

            // Clic en casilla válida del patrón de movimiento: ejecutar movimiento
            if (currentLegalMoves.Contains(clickedCoord))
            {
                RealizarMovimiento(selectedPiece, clickedCoord);
            }
            else
            {
                // Clic en casilla vacía o inválida: deseleccionar sin bloqueos ni advertencias
                DeseleccionarPieza();
            }
        }
    }

    private void SeleccionarPieza(ChessPiece piece)
    {
        selectedPiece = piece;
        // Obtenemos los movimientos según el patrón propio de la pieza (sin restricciones de Jaque)
        currentLegalMoves = BoardManager.Instance.GetLegalMoves(piece);

        ActualizarVisualesTablero();
    }

    private void DeseleccionarPieza()
    {
        selectedPiece = null;
        currentLegalMoves.Clear();
        ActualizarVisualesTablero();
    }

    private void RealizarMovimiento(ChessPiece piece, Vector2Int destination)
    {
        // 1. Ejecutar movimiento en BoardManager (resuelve desplazamientos, capturas, enroque, etc.)
        bool reyFueCapturado = BoardManager.Instance.ExecuteMove(piece, destination);

        // 2. Limpiar selección
        selectedPiece = null;
        currentLegalMoves.Clear();

        // 3. Evaluar la nueva condición de fin de juego: Captura del Rey
        if (reyFueCapturado)
        {
            FinalizarPorCapturaDeRey(currentTurn);
            return;
        }

        // 4. Alternar el turno
        currentTurn = (currentTurn == PieceColor.White) ? PieceColor.Black : PieceColor.White;

        // 5. Refrescar visuales del tablero
        ActualizarVisualesTablero();
    }

    /// <summary>
    /// Finaliza inmediatamente la partida al ser capturado el Rey.
    /// Desactiva interacción y abre el panel modal oscuro y dorado de Fin de Partida.
    /// </summary>
    private void FinalizarPorCapturaDeRey(PieceColor ganador)
    {
        juegoTerminado = true;
        string nombreGanador = (ganador == PieceColor.White) ? "Blancas / Doradas" : "Azules / Negras";
        mensajePantalla = $"¡VICTORIA! Las piezas {nombreGanador} capturaron al Rey.";
        Debug.Log($"<color=gold>[GameManager]</color> <b>{mensajePantalla}</b>");

        if (MenuPrincipal.Instance != null)
        {
            MenuPrincipal.Instance.ActualizarAvisoJaque("", false);
            MenuPrincipal.Instance.MostrarFinDePartida(
                "¡ V I C T O R I A !",
                $"E L   R E Y   H A   S I D O   C A P T U R A D O\n\nGANADOR: {nombreGanador.ToUpperInvariant()}"
            );
        }

        ActualizarVisualesTablero();
    }

    /// <summary>
    /// Actualiza los colores de las casillas: selección (amarillo) y movimientos válidos (verde).
    /// </summary>
    public void ActualizarVisualesTablero()
    {
        if (BoardManager.Instance == null) return;

        BoardManager.Instance.ActualizarColoresTablero();

        // Resaltar casilla de la pieza seleccionada y sus destinos geométricamente disponibles
        if (selectedPiece != null)
        {
            BoardManager.Instance.ResaltarCasilla(selectedPiece.currentPosition, BoardManager.Instance.colorSeleccion);
            BoardManager.Instance.ResaltarMovimientosValidos(currentLegalMoves);
        }
    }

    /// <summary>
    /// Reinicia la partida restaurando el tablero, turnos e interfaz.
    /// </summary>
    public void ReiniciarPartida()
    {
        currentTurn = PieceColor.White;
        juegoTerminado = false;
        mensajePantalla = "";
        selectedPiece = null;
        currentLegalMoves.Clear();

        if (MenuPrincipal.Instance != null)
        {
            MenuPrincipal.Instance.OcultarFinDePartida();
            MenuPrincipal.Instance.ActualizarAvisoJaque("", false);
        }

        if (BoardManager.Instance != null)
        {
            BoardManager.Instance.ColocarPiezasIniciales();
        }

        ActualizarVisualesTablero();
    }
}
