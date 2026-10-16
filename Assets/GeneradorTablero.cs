using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement; // Permite reiniciar la partida al terminar

public class GeneradorTablero : MonoBehaviour
{
    [Header("Tablero")]
    public GameObject casillaPrefab;
    public Color colorClaro = new Color(0.85f, 0.85f, 0.85f);
    public Color colorOscuro = new Color(0.3f, 0.4f, 0.5f);
    public Color colorSeleccion = new Color(0.9f, 0.8f, 0.2f); // Amarillo
    public Color colorJaque = new Color(0.85f, 0.15f, 0.15f);   // Rojo intenso para el Jaque

    [Header("Prefabs Piezas Blancas / Doradas (Abajo: Filas 0 y 1)")]
    public GameObject peonBlanco;
    public GameObject torreBlanca;
    public GameObject caballoBlanco;
    public GameObject alfilBlanco;
    public GameObject reinaBlanca;
    public GameObject reyBlanco;

    [Header("Prefabs Piezas Azules / Negras (Arriba: Filas 6 y 7)")]
    public GameObject peonAzul;
    public GameObject torreAzul;
    public GameObject caballoAzul;
    public GameObject alfilAzul;
    public GameObject reinaAzul;
    public GameObject reyAzul;

    // Memoria interna del juego
    private Pieza[,] matrizPiezas = new Pieza[8, 8];
    private SpriteRenderer[,] casillasRenderers = new SpriteRenderer[8, 8];
    private Pieza piezaSeleccionada;
    private bool turnoBlancas = true;
    
    // Control de estado de la partida
    private bool juegoTerminado = false;
    private string mensajePantalla = "";

    public bool EsTurnoBlancas => turnoBlancas;
    public bool JuegoTerminado => juegoTerminado;
    public string MensajePantalla => mensajePantalla;

    void Awake()
    {
        if (FindFirstObjectByType<MenuPrincipal>() == null)
        {
            gameObject.AddComponent<MenuPrincipal>();
        }
    }

    void Start()
    {
        GenerarCuadricula();
        ColocarPiezasIniciales();
    }

    void Update()
    {
        // Si el menú está abierto, bloqueamos la interacción con el tablero
        if (MenuPrincipal.Instance != null && MenuPrincipal.Instance.EstaMenuAbierto())
            return;

        // Si el juego terminó, bloqueamos todos los clics en el tablero
        if (juegoTerminado) return;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (MenuPrincipal.Instance != null && MenuPrincipal.Instance.EsClicEnHUD(Mouse.current.position.ReadValue()))
                return;

            DetectarClic();
        }
    }

    void GenerarCuadricula()
    {
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
        Camera.main.transform.position = new Vector3(3.5f, 3.5f, -10f);
    }

    void RestaurarColorCasilla(int x, int y)
    {
        bool esClara = (x + y) % 2 != 0;
        casillasRenderers[x, y].color = esClara ? colorClaro : colorOscuro;
    }

    void ActualizarColoresTablero()
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                RestaurarColorCasilla(x, y);
            }
        }

        // Si el Rey del turno actual está en Jaque, pintamos su casilla de ROJO
        if (EstaEnJaque(turnoBlancas))
        {
            Pieza rey = BuscarRey(turnoBlancas);
            if (rey != null)
            {
                casillasRenderers[rey.xActual, rey.yActual].color = colorJaque;
            }
        }

        // Si hay una pieza seleccionada, mantenemos su casilla amarilla
        if (piezaSeleccionada != null)
        {
            casillasRenderers[piezaSeleccionada.xActual, piezaSeleccionada.yActual].color = colorSeleccion;
        }
    }

    void ColocarPiezasIniciales()
    {
        for (int x = 0; x < 8; x++)
        {
            InstanciarPieza(peonBlanco, TipoPieza.Peon, true, x, 1);
            InstanciarPieza(peonAzul, TipoPieza.Peon, false, x, 6);
        }

        InstanciarPieza(torreBlanca, TipoPieza.Torre, true, 0, 0);
        InstanciarPieza(torreBlanca, TipoPieza.Torre, true, 7, 0);
        InstanciarPieza(torreAzul, TipoPieza.Torre, false, 0, 7);
        InstanciarPieza(torreAzul, TipoPieza.Torre, false, 7, 7);

        InstanciarPieza(caballoBlanco, TipoPieza.Caballo, true, 1, 0);
        InstanciarPieza(caballoBlanco, TipoPieza.Caballo, true, 6, 0);
        InstanciarPieza(caballoAzul, TipoPieza.Caballo, false, 1, 7);
        InstanciarPieza(caballoAzul, TipoPieza.Caballo, false, 6, 7);

        InstanciarPieza(alfilBlanco, TipoPieza.Alfil, true, 2, 0);
        InstanciarPieza(alfilBlanco, TipoPieza.Alfil, true, 5, 0);
        InstanciarPieza(alfilAzul, TipoPieza.Alfil, false, 2, 7);
        InstanciarPieza(alfilAzul, TipoPieza.Alfil, false, 5, 7);

        InstanciarPieza(reinaBlanca, TipoPieza.Reina, true, 3, 0);
        InstanciarPieza(reyBlanco, TipoPieza.Rey, true, 4, 0);
        InstanciarPieza(reinaAzul, TipoPieza.Reina, false, 3, 7);
        InstanciarPieza(reyAzul, TipoPieza.Rey, false, 4, 7);
    }

    void InstanciarPieza(GameObject prefab, TipoPieza tipo, bool esBlanca, int x, int y)
    {
        if (prefab == null) return;

        GameObject obj = Instantiate(prefab);
        obj.name = $"{tipo}_{(esBlanca ? "Blanco" : "Azul")}_{x}_{y}";

        Pieza componentePieza = obj.GetComponent<Pieza>();
        if (componentePieza == null)
        {
            componentePieza = obj.AddComponent<Pieza>();
        }

        componentePieza.Configurar(tipo, esBlanca, x, y);
        matrizPiezas[x, y] = componentePieza;
    }

    void DetectarClic()
    {
        Vector2 posMousePantalla = Mouse.current.position.ReadValue();
        Vector3 posMundo = Camera.main.ScreenToWorldPoint(new Vector3(posMousePantalla.x, posMousePantalla.y, 10f));

        int x = Mathf.RoundToInt(posMundo.x);
        int y = Mathf.RoundToInt(posMundo.y);

        if (x < 0 || x >= 8 || y < 0 || y >= 8) return;

        if (piezaSeleccionada == null)
        {
            Pieza piezaEnCasilla = matrizPiezas[x, y];
            if (piezaEnCasilla != null && piezaEnCasilla.esBlanca == turnoBlancas)
            {
                piezaSeleccionada = piezaEnCasilla;
                ActualizarColoresTablero();
            }
        }
        else
        {
            // Cambiar de pieza si hace clic en otra de su mismo equipo
            Pieza piezaEnCasilla = matrizPiezas[x, y];
            if (piezaEnCasilla != null && piezaEnCasilla.esBlanca == turnoBlancas)
            {
                piezaSeleccionada = piezaEnCasilla;
                ActualizarColoresTablero();
                return;
            }

            // Verificar que el movimiento sea válido Y que no deje a nuestro Rey en Jaque
            if (EsMovimientoLegalYSinJaque(piezaSeleccionada, x, y))
            {
                MoverPieza(piezaSeleccionada, x, y);
                piezaSeleccionada = null;
                
                // Cambiamos el turno y verificamos el estado del siguiente jugador
                turnoBlancas = !turnoBlancas;
                VerificarEstadoDelJuego();
                ActualizarColoresTablero();
            }
            else
            {
                // Si el movimiento es ilegal o deja al Rey en Jaque, cancelamos selección
                if (EstaEnJaque(turnoBlancas))
                {
                    mensajePantalla = "¡Movimiento inválido! Debes proteger a tu Rey del Jaque.";
                }
                piezaSeleccionada = null;
                ActualizarColoresTablero();
            }
        }
    }

    // Simula el movimiento en memoria para comprobar que el Rey propio no quede en Jaque
    bool EsMovimientoLegalYSinJaque(Pieza pieza, int destinoX, int destinoY)
    {
        if (!pieza.EsMovimientoValido(destinoX, destinoY, matrizPiezas))
            return false;

        int origenX = pieza.xActual;
        int origenY = pieza.yActual;
        Pieza piezaEnDestino = matrizPiezas[destinoX, destinoY];

        // 1. Simulamos el movimiento temporalmente
        matrizPiezas[origenX, origenY] = null;
        matrizPiezas[destinoX, destinoY] = pieza;
        pieza.xActual = destinoX;
        pieza.yActual = destinoY;

        // 2. ¿Nuestro Rey quedó en Jaque tras hacer este movimiento?
        bool reyQuedaEnPeligro = EstaEnJaque(pieza.esBlanca);

        // 3. Deshacemos la simulación para dejar todo como estaba
        matrizPiezas[origenX, origenY] = pieza;
        matrizPiezas[destinoX, destinoY] = piezaEnDestino;
        pieza.xActual = origenX;
        pieza.yActual = origenY;

        // Solo permitimos el movimiento si el Rey NO queda en peligro
        return !reyQuedaEnPeligro;
    }

    Pieza BuscarRey(bool esBlanco)
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                Pieza p = matrizPiezas[x, y];
                if (p != null && p.esBlanca == esBlanco && p.tipo == TipoPieza.Rey)
                {
                    return p;
                }
            }
        }
        return null;
    }

    bool EstaEnJaque(bool equipoBlanco)
    {
        Pieza rey = BuscarRey(equipoBlanco);
        if (rey == null) return false;

        // Revisamos si alguna pieza enemiga puede atacar la casilla donde está el Rey
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                Pieza piezaEnemiga = matrizPiezas[x, y];
                if (piezaEnemiga != null && piezaEnemiga.esBlanca != equipoBlanco)
                {
                    if (piezaEnemiga.EsMovimientoValido(rey.xActual, rey.yActual, matrizPiezas))
                    {
                        return true; // ¡El Rey está bajo ataque!
                    }
                }
            }
        }
        return false;
    }

    bool TieneMovimientosSalvadores(bool equipoBlanco)
    {
        // Probamos todas las piezas del jugador con todas las casillas del tablero
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                Pieza p = matrizPiezas[x, y];
                if (p != null && p.esBlanca == equipoBlanco)
                {
                    for (int destX = 0; destX < 8; destX++)
                    {
                        for (int destY = 0; destY < 8; destY++)
                        {
                            if (EsMovimientoLegalYSinJaque(p, destX, destY))
                            {
                                return true; // Encontró al menos una jugada que salva al Rey
                            }
                        }
                    }
                }
            }
        }
        return false; // No tiene escapatoria
    }

    void VerificarEstadoDelJuego()
    {
        bool enJaque = EstaEnJaque(turnoBlancas);
        bool puedeMoverse = TieneMovimientosSalvadores(turnoBlancas);

        if (!puedeMoverse)
        {
            juegoTerminado = true;
            if (enJaque)
            {
                string ganador = turnoBlancas ? "Azules / Negras" : "Blancas / Doradas";
                mensajePantalla = $"¡JAQUE MATE! Ganaron las piezas {ganador}";
            }
            else
            {
                mensajePantalla = "¡TABLAS! Fin del juego por Rey Ahogado";
            }
        }
        else if (enJaque)
        {
            string equipoEnPeligro = turnoBlancas ? "Blanco / Dorado" : "Azul / Negro";
            mensajePantalla = $"¡JAQUE al Rey {equipoEnPeligro}! Debes defenderlo.";
        }
        else
        {
            mensajePantalla = "";
        }
    }

    void MoverPieza(Pieza pieza, int nuevoX, int nuevoY)
    {
        int viejoX = pieza.xActual;
        int viejoY = pieza.yActual;

        if (matrizPiezas[nuevoX, nuevoY] != null)
        {
            Pieza piezaCapturada = matrizPiezas[nuevoX, nuevoY];
            Destroy(piezaCapturada.gameObject);
        }

        matrizPiezas[viejoX, viejoY] = null;
        matrizPiezas[nuevoX, nuevoY] = pieza;
        pieza.Posicionar(nuevoX, nuevoY);
    }

    public void CambiarTema(Color nuevoColorClaro, Color nuevoColorOscuro)
    {
        colorClaro = nuevoColorClaro;
        colorOscuro = nuevoColorOscuro;
        ActualizarColoresTablero();
    }

    public void ReiniciarTablero()
    {
        // 1. Destruir todas las piezas existentes
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                if (matrizPiezas[x, y] != null)
                {
                    Destroy(matrizPiezas[x, y].gameObject);
                    matrizPiezas[x, y] = null;
                }
            }
        }

        // 2. Restaurar estado de juego
        piezaSeleccionada = null;
        turnoBlancas = true;
        juegoTerminado = false;
        mensajePantalla = "";

        // 3. Volver a colocar piezas y refrescar colores
        ColocarPiezasIniciales();
        ActualizarColoresTablero();
    }

    // Dibuja el cartel de advertencia de Jaque y el cartel de Fin de Juego sin tener que crear UI manual
    void OnGUI()
    {
        // Si el menú principal, pausa u opciones están abiertos, no dibujamos encima
        if (MenuPrincipal.Instance != null && MenuPrincipal.Instance.EstaMenuAbierto())
            return;

        if (!string.IsNullOrEmpty(mensajePantalla))
        {
            GUIStyle estiloBanner = new GUIStyle(GUI.skin.box);
            estiloBanner.fontSize = 20;
            estiloBanner.fontStyle = FontStyle.Bold;
            estiloBanner.alignment = TextAnchor.MiddleCenter;
            estiloBanner.normal.textColor = new Color(0.831f, 0.686f, 0.216f); // #D4AF37 Dorado

            // Caja superior con fondo #0A0A0A
            GUI.backgroundColor = new Color(0.039f, 0.039f, 0.039f, 0.96f);
            GUI.Box(new Rect(Screen.width / 2 - 280, 15, 560, 50), mensajePantalla.ToUpperInvariant(), estiloBanner);

            // Si terminó la partida, mostramos botones minimalistas
            if (juegoTerminado)
            {
                GUIStyle estiloBoton = new GUIStyle(GUI.skin.button);
                estiloBoton.fontSize = 15;
                estiloBoton.fontStyle = FontStyle.Bold;
                estiloBoton.alignment = TextAnchor.MiddleCenter;
                estiloBoton.normal.textColor = new Color(0.831f, 0.686f, 0.216f); // #D4AF37

                GUI.backgroundColor = new Color(0.078f, 0.078f, 0.078f, 0.98f); // #141414
                if (GUI.Button(new Rect(Screen.width / 2 - 245, 75, 230, 46), "J U G A R   D E   N U E V O", estiloBoton))
                {
                    ReiniciarTablero();
                }

                if (GUI.Button(new Rect(Screen.width / 2 + 15, 75, 230, 46), "M E N Ú   P R I N C I P A L", estiloBoton))
                {
                    if (MenuPrincipal.Instance != null)
                    {
                        MenuPrincipal.Instance.VolverAlMenuPrincipal();
                    }
                    else
                    {
                        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                    }
                }
            }
        }
    }
}