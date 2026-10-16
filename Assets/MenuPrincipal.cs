using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public enum EstadoMenu
{
    MenuPrincipal,
    EnPartida,
    Pausa,
    Opciones
}

public class MenuPrincipal : MonoBehaviour
{
    public static MenuPrincipal Instance { get; private set; }

    [Header("Referencias")]
    public GeneradorTablero generadorTablero;

    [Header("Modo de Interfaz")]
    [Tooltip("Genera y utiliza Canvas uGUI de alta fidelidad con micro-interacciones. Si es false, usa OnGUI minimalista.")]
    public bool usarCanvasUI = true;

    [Header("Estado Actual")]
    public EstadoMenu estadoActual = EstadoMenu.MenuPrincipal;
    private EstadoMenu estadoPrevio = EstadoMenu.MenuPrincipal;

    [Header("Configuraciones de Usuario")]
    [Range(0f, 1f)] public float volumenGeneral = 0.8f;
    public bool pantallaCompleta = true;
    public int temaSeleccionado = 0;

    // Paleta Minimalista Negro y Dorado
    public static readonly Color ColorFondoNegro = new Color(0.039f, 0.039f, 0.039f, 0.96f);       // #0A0A0A
    public static readonly Color ColorSuperficieCard = new Color(0.067f, 0.067f, 0.067f, 0.98f);    // #111111
    public static readonly Color ColorBotonNormal = new Color(0.078f, 0.078f, 0.078f, 0.95f);       // #141414
    public static readonly Color ColorDorado = new Color(0.831f, 0.686f, 0.216f, 1f);              // #D4AF37
    public static readonly Color ColorDoradoMetalico = new Color(0.773f, 0.627f, 0.349f, 1f);      // #C5A059
    public static readonly Color ColorDoradoOscuro = new Color(0.541f, 0.435f, 0.114f, 1f);        // #8A6F1D
    public static readonly Color ColorTextoHover = new Color(0.039f, 0.039f, 0.039f, 1f);          // #0A0A0A

    // Temas visuales para el tablero de ajedrez
    public readonly (string nombre, Color claro, Color oscuro)[] temasDisponibles = new (string, Color, Color)[]
    {
        ("Azul Moderno", new Color(0.85f, 0.85f, 0.85f), new Color(0.30f, 0.40f, 0.50f)),
        ("Madera Noble", new Color(0.92f, 0.82f, 0.65f), new Color(0.55f, 0.35f, 0.20f)),
        ("Verde Torneo", new Color(0.93f, 0.93f, 0.84f), new Color(0.30f, 0.48f, 0.30f)),
        ("Grafito Minimal", new Color(0.88f, 0.88f, 0.90f), new Color(0.22f, 0.25f, 0.30f)),
        ("Oro Real", new Color(0.96f, 0.89f, 0.70f), new Color(0.28f, 0.22f, 0.42f))
    };

    [Header("Referencias Canvas (Se autogeneran si están vacías)")]
    public Canvas canvasMenu;
    public GameObject panelMenuPrincipal;
    public GameObject panelOpciones;
    public GameObject panelPausa;
    public GameObject panelHUD;
    public Slider sliderVolumen;
    public Text textoPorcentajeVolumen;
    public Text textoTurnoHUD;
    public TextMeshProUGUI textoTurnoTMP;

    // Control de animaciones OnGUI
    private Dictionary<string, float> animacionesHover = new Dictionary<string, float>();
    private Texture2D texFondoNegro;
    private Texture2D texCardPanel;
    private Texture2D texBotonBase;
    private Texture2D texBotonHoverDorado;
    private Texture2D texBotonClickOscuro;
    private Texture2D texLineaDorada;

    private GUIStyle estiloTitulo;
    private GUIStyle estiloSubtitulo;
    private GUIStyle estiloPanel;
    private GUIStyle estiloBoton;
    private GUIStyle estiloEtiqueta;
    private GUIStyle estiloBotonHUD;
    private GUIStyle estiloBadgeTurno;
    private bool estilosInicializados = false;
    private Rect rectBotonHUD;

    void Awake()
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

        if (generadorTablero == null)
        {
            generadorTablero = FindFirstObjectByType<GeneradorTablero>();
        }

        CargarPreferencias();
    }

    void Start()
    {
        AudioListener.volume = volumenGeneral;
        Screen.fullScreen = pantallaCompleta;

        if (generadorTablero != null && temaSeleccionado >= 0 && temaSeleccionado < temasDisponibles.Length)
        {
            var tema = temasDisponibles[temaSeleccionado];
            generadorTablero.CambiarTema(tema.claro, tema.oscuro);
        }

        if (usarCanvasUI)
        {
            ConstruirCanvasSiEsNecesario();
            ActualizarPanelesCanvas();
        }
    }

    void Update()
    {
        // Atajo teclado Escape
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            GestionarTeclaEscape();
        }

        if (usarCanvasUI && estadoActual == EstadoMenu.EnPartida)
        {
            ActualizarTurnoCanvas();
        }
    }

    void OnDestroy()
    {
        LiberarTexturas();
    }

    private void GestionarTeclaEscape()
    {
        switch (estadoActual)
        {
            case EstadoMenu.Opciones:
                VolverDeOpciones();
                break;
            case EstadoMenu.EnPartida:
                AlternarPausa();
                break;
            case EstadoMenu.Pausa:
                ReanudarPartida();
                break;
        }
    }

    // --- ACCIONES DE NAVEGACIÓN ---

    public void IniciarPartida1vs1()
    {
        estadoActual = EstadoMenu.EnPartida;
        if (generadorTablero != null)
        {
            generadorTablero.ReiniciarTablero();
        }
        ActualizarPanelesCanvas();
    }

    public void AbrirOpciones()
    {
        estadoPrevio = estadoActual;
        estadoActual = EstadoMenu.Opciones;
        ActualizarPanelesCanvas();
    }

    public void VolverDeOpciones()
    {
        GuardarPreferencias();
        estadoActual = estadoPrevio;
        ActualizarPanelesCanvas();
    }

    public void AlternarPausa()
    {
        if (estadoActual == EstadoMenu.EnPartida)
        {
            estadoActual = EstadoMenu.Pausa;
        }
        else if (estadoActual == EstadoMenu.Pausa)
        {
            estadoActual = EstadoMenu.EnPartida;
        }
        ActualizarPanelesCanvas();
    }

    public void ReanudarPartida()
    {
        estadoActual = EstadoMenu.EnPartida;
        ActualizarPanelesCanvas();
    }

    public void ReiniciarPartida()
    {
        if (generadorTablero != null)
        {
            generadorTablero.ReiniciarTablero();
        }
        estadoActual = EstadoMenu.EnPartida;
        ActualizarPanelesCanvas();
    }

    public void VolverAlMenuPrincipal()
    {
        estadoActual = EstadoMenu.MenuPrincipal;
        ActualizarPanelesCanvas();
    }

    public void SalirDelJuego()
    {
        GuardarPreferencias();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void CambiarVolumen(float nuevoVolumen)
    {
        volumenGeneral = Mathf.Clamp01(nuevoVolumen);
        AudioListener.volume = volumenGeneral;
        if (sliderVolumen != null && Mathf.Abs(sliderVolumen.value - volumenGeneral) > 0.01f)
        {
            sliderVolumen.value = volumenGeneral;
        }
        if (textoPorcentajeVolumen != null)
        {
            textoPorcentajeVolumen.text = $"{Mathf.RoundToInt(volumenGeneral * 100)}%";
        }
    }

    public void AlternarPantallaCompleta()
    {
        pantallaCompleta = !Screen.fullScreen;
        Screen.fullScreen = pantallaCompleta;
    }

    public void SeleccionarTema(int indice)
    {
        if (indice < 0 || indice >= temasDisponibles.Length) return;
        temaSeleccionado = indice;
        if (generadorTablero != null)
        {
            var tema = temasDisponibles[indice];
            generadorTablero.CambiarTema(tema.claro, tema.oscuro);
        }
    }

    public bool EstaMenuAbierto()
    {
        return estadoActual == EstadoMenu.MenuPrincipal ||
               estadoActual == EstadoMenu.Pausa ||
               estadoActual == EstadoMenu.Opciones;
    }

    public bool EsClicEnHUD(Vector2 posMousePantalla)
    {
        Vector2 posGUI = new Vector2(posMousePantalla.x, Screen.height - posMousePantalla.y);
        return estadoActual == EstadoMenu.EnPartida && rectBotonHUD.Contains(posGUI);
    }

    private void CargarPreferencias()
    {
        volumenGeneral = PlayerPrefs.GetFloat("Ajedrez_Volumen", 0.8f);
        pantallaCompleta = PlayerPrefs.GetInt("Ajedrez_PantallaCompleta", 1) == 1;
        temaSeleccionado = PlayerPrefs.GetInt("Ajedrez_TemaTablero", 0);
    }

    private void GuardarPreferencias()
    {
        PlayerPrefs.SetFloat("Ajedrez_Volumen", volumenGeneral);
        PlayerPrefs.SetInt("Ajedrez_PantallaCompleta", pantallaCompleta ? 1 : 0);
        PlayerPrefs.SetInt("Ajedrez_TemaTablero", temaSeleccionado);
        PlayerPrefs.Save();
    }

    // --- CONSTRUCCIÓN DEL CANVAS MINIMALISTA NEGRO Y DORADO ---

    private void ConstruirCanvasSiEsNecesario()
    {
        if (canvasMenu != null && panelMenuPrincipal != null) return;

        // Buscar si ya existe en la escena
        Canvas existente = FindFirstObjectByType<Canvas>();
        if (existente != null && existente.name == "Canvas_MenuMinimalista")
        {
            canvasMenu = existente;
            return;
        }

        // Crear EventSystem si no existe
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<EventSystem>();
            esObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        // Crear Canvas
        GameObject canvasObj = new GameObject("Canvas_MenuMinimalista");
        canvasMenu = canvasObj.AddComponent<Canvas>();
        canvasMenu.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasMenu.sortingOrder = 50;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // Sprite 9-sliced con borde dorado de 2px
        Sprite spriteBoton = UIThemeApply.GenerarSpriteConBorde(32, 32, 2, ColorBotonNormal, ColorDorado);
        Sprite spriteCard = UIThemeApply.GenerarSpriteConBorde(32, 32, 1, ColorSuperficieCard, ColorDoradoMetalico);

        // --- 1. PANEL MENÚ PRINCIPAL ---
        panelMenuPrincipal = CrearPanelFondo(canvasObj.transform, "PanelMenuPrincipal");
        GameObject cardMenu = CrearCard(panelMenuPrincipal.transform, "CardPrincipal", 480, 520, spriteCard);

        CrearTexto(cardMenu.transform, "A  J  E  D  R  E  Z", 42, ColorDorado, new Vector2(0, 170));
        CrearTexto(cardMenu.transform, "D  U  E  L  O     1   V  S   1", 16, ColorDoradoMetalico, new Vector2(0, 125));
        CrearLineaDivisoria(cardMenu.transform, new Vector2(0, 95), 320, 1);

        CrearBotonCanvas(cardMenu.transform, "J  U  G  A  R     1   V  S   1", new Vector2(0, 25), 380, 60, spriteBoton, IniciarPartida1vs1);
        CrearBotonCanvas(cardMenu.transform, "O  P  C  I  O  N  E  S", new Vector2(0, -55), 380, 56, spriteBoton, AbrirOpciones);
        CrearBotonCanvas(cardMenu.transform, "S  A  L  I  R", new Vector2(0, -135), 380, 56, spriteBoton, SalirDelJuego);

        CrearTexto(cardMenu.transform, "S E L E C C I O N A   U N A   O P C I Ó N", 11, new Color(0.6f, 0.6f, 0.6f, 0.6f), new Vector2(0, -215));

        // --- 2. PANEL OPCIONES ---
        panelOpciones = CrearPanelFondo(canvasObj.transform, "PanelOpciones");
        GameObject cardOpciones = CrearCard(panelOpciones.transform, "CardOpciones", 560, 580, spriteCard);

        CrearTexto(cardOpciones.transform, "C  O  N  F  I  G  U  R  A  C  I  Ó  N", 34, ColorDorado, new Vector2(0, 210));
        CrearTexto(cardOpciones.transform, "P  A  L  E  T  A     Y     A  J  U  S  T  E  S", 15, ColorDoradoMetalico, new Vector2(0, 170));
        CrearLineaDivisoria(cardOpciones.transform, new Vector2(0, 145), 400, 1);

        // Sección Volumen
        CrearTexto(cardOpciones.transform, "V O L U M E N   G E N E R A L", 14, ColorDorado, new Vector2(-120, 100));
        textoPorcentajeVolumen = CrearTexto(cardOpciones.transform, $"{Mathf.RoundToInt(volumenGeneral * 100)}%", 14, ColorDoradoMetalico, new Vector2(160, 100));

        CrearBotonCanvas(cardOpciones.transform, "S I L E N C I A R", new Vector2(130, 50), 120, 36, spriteBoton, () => {
            CambiarVolumen(volumenGeneral > 0f ? 0f : 0.8f);
        });

        CrearBotonCanvas(cardOpciones.transform, "- 10%", new Vector2(-150, 50), 80, 36, spriteBoton, () => {
            CambiarVolumen(volumenGeneral - 0.1f);
        });
        CrearBotonCanvas(cardOpciones.transform, "+ 10%", new Vector2(-60, 50), 80, 36, spriteBoton, () => {
            CambiarVolumen(volumenGeneral + 0.1f);
        });

        // Sección Pantalla Completa
        CrearLineaDivisoria(cardOpciones.transform, new Vector2(0, 10), 400, 1);
        CrearBotonCanvas(cardOpciones.transform, "A L T E R N A R   P A N T A L L A   C O M P L E T A", new Vector2(0, -35), 420, 46, spriteBoton, AlternarPantallaCompleta);

        // Sección Temas
        CrearLineaDivisoria(cardOpciones.transform, new Vector2(0, -75), 400, 1);
        CrearTexto(cardOpciones.transform, "E S T I L O   D E L   T A B L E R O", 13, ColorDorado, new Vector2(0, -95));

        float xStart = -160;
        for (int i = 0; i < temasDisponibles.Length; i++)
        {
            int idx = i;
            string nombreCorto = temasDisponibles[i].nombre.ToUpperInvariant();
            CrearBotonCanvas(cardOpciones.transform, nombreCorto, new Vector2(xStart + (i * 80), -135), 75, 34, spriteBoton, () => {
                SeleccionarTema(idx);
            });
        }

        // Botón Guardar y Volver
        CrearBotonCanvas(cardOpciones.transform, "G  U  A  R  D  A  R     Y     V  O  L  V  E  R", new Vector2(0, -215), 420, 52, spriteBoton, VolverDeOpciones);

        // --- 3. PANEL PAUSA ---
        panelPausa = CrearPanelFondo(canvasObj.transform, "PanelPausa");
        GameObject cardPausa = CrearCard(panelPausa.transform, "CardPausa", 480, 520, spriteCard);

        CrearTexto(cardPausa.transform, "P  A  R  T  I  D  A     E  N     P  A  U  S  A", 32, ColorDorado, new Vector2(0, 185));
        CrearTexto(cardPausa.transform, "1   V  S   1", 14, ColorDoradoMetalico, new Vector2(0, 145));
        CrearLineaDivisoria(cardPausa.transform, new Vector2(0, 120), 320, 1);

        CrearBotonCanvas(cardPausa.transform, "C  O  N  T  I  N  U  A  R", new Vector2(0, 65), 380, 54, spriteBoton, ReanudarPartida);
        CrearBotonCanvas(cardPausa.transform, "R  E  I  N  I  C  I  A  R", new Vector2(0, 0), 380, 50, spriteBoton, ReiniciarPartida);
        CrearBotonCanvas(cardPausa.transform, "O  P  C  I  O  N  E  S", new Vector2(0, -60), 380, 50, spriteBoton, AbrirOpciones);
        CrearBotonCanvas(cardPausa.transform, "M  E  N  Ú     P  R  I  N  C  I  P  A  L", new Vector2(0, -120), 380, 50, spriteBoton, VolverAlMenuPrincipal);
        CrearBotonCanvas(cardPausa.transform, "S  A  L  I  R", new Vector2(0, -180), 380, 46, spriteBoton, SalirDelJuego);

        // --- 4. HUD EN PARTIDA ---
        panelHUD = new GameObject("PanelHUD");
        panelHUD.transform.SetParent(canvasObj.transform, false);
        RectTransform rtHUD = panelHUD.AddComponent<RectTransform>();
        rtHUD.anchorMin = Vector2.zero;
        rtHUD.anchorMax = Vector2.one;
        rtHUD.offsetMin = Vector2.zero;
        rtHUD.offsetMax = Vector2.zero;

        // Botón Menú superior derecho
        GameObject btnHUDObj = CrearBotonCanvas(panelHUD.transform, "M  E  N  Ú", Vector2.zero, 130, 40, spriteBoton, AlternarPausa);
        RectTransform rtBtn = btnHUDObj.GetComponent<RectTransform>();
        rtBtn.anchorMin = new Vector2(1, 1);
        rtBtn.anchorMax = new Vector2(1, 1);
        rtBtn.pivot = new Vector2(1, 1);
        rtBtn.anchoredPosition = new Vector2(-25, -20);

        // Badge Turno superior izquierdo
        GameObject badgeTurno = new GameObject("BadgeTurno");
        badgeTurno.transform.SetParent(panelHUD.transform, false);
        RectTransform rtBadge = badgeTurno.AddComponent<RectTransform>();
        rtBadge.anchorMin = new Vector2(0, 1);
        rtBadge.anchorMax = new Vector2(0, 1);
        rtBadge.pivot = new Vector2(0, 1);
        rtBadge.anchoredPosition = new Vector2(25, -20);
        rtBadge.sizeDelta = new Vector2(220, 40);

        Image imgBadge = badgeTurno.AddComponent<Image>();
        imgBadge.color = ColorSuperficieCard;
        imgBadge.sprite = spriteCard;
        imgBadge.type = Image.Type.Sliced;

        textoTurnoHUD = CrearTexto(badgeTurno.transform, "T U R N O :   B L A N C A S", 13, ColorDorado, Vector2.zero);

        ActualizarPanelesCanvas();
    }

    private GameObject CrearPanelFondo(Transform padre, string nombre)
    {
        GameObject fondoObj = new GameObject(nombre);
        fondoObj.transform.SetParent(padre, false);
        RectTransform rt = fondoObj.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image img = fondoObj.AddComponent<Image>();
        img.color = ColorFondoNegro;
        return fondoObj;
    }

    private GameObject CrearCard(Transform padre, string nombre, float ancho, float alto, Sprite spriteBorde)
    {
        GameObject card = new GameObject(nombre);
        card.transform.SetParent(padre, false);
        RectTransform rt = card.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(ancho, alto);
        rt.anchoredPosition = Vector2.zero;

        Image img = card.AddComponent<Image>();
        img.color = ColorSuperficieCard;
        img.sprite = spriteBorde;
        img.type = Image.Type.Sliced;
        return card;
    }

    private void CrearLineaDivisoria(Transform padre, Vector2 posicion, float ancho, float grosor)
    {
        GameObject linea = new GameObject("LineaDivisoria");
        linea.transform.SetParent(padre, false);
        RectTransform rt = linea.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(ancho, grosor);
        rt.anchoredPosition = posicion;

        Image img = linea.AddComponent<Image>();
        img.color = new Color(ColorDorado.r, ColorDorado.g, ColorDorado.b, 0.4f);
    }

    private Text CrearTexto(Transform padre, string contenido, int tamano, Color color, Vector2 posicion)
    {
        GameObject txtObj = new GameObject("Texto");
        txtObj.transform.SetParent(padre, false);
        RectTransform rt = txtObj.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(460, tamano + 16);
        rt.anchoredPosition = posicion;

        Text t = txtObj.AddComponent<Text>();
        t.text = contenido;
        t.fontSize = tamano;
        t.fontStyle = FontStyle.Bold;
        t.color = color;
        t.alignment = TextAnchor.MiddleCenter;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
        return t;
    }

    private GameObject CrearBotonCanvas(Transform padre, string texto, Vector2 pos, float ancho, float alto, Sprite spriteBorde, UnityEngine.Events.UnityAction accion)
    {
        GameObject btnObj = new GameObject("Boton_" + texto.Trim());
        btnObj.transform.SetParent(padre, false);
        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(ancho, alto);
        rt.anchoredPosition = pos;

        Image img = btnObj.AddComponent<Image>();
        img.color = ColorBotonNormal;
        img.sprite = spriteBorde;
        img.type = Image.Type.Sliced;

        Button btn = btnObj.AddComponent<Button>();
        btn.onClick.AddListener(accion);

        // Texto del botón
        GameObject txtObj = new GameObject("Texto");
        txtObj.transform.SetParent(btnObj.transform, false);
        RectTransform rtTxt = txtObj.AddComponent<RectTransform>();
        rtTxt.anchorMin = Vector2.zero;
        rtTxt.anchorMax = Vector2.one;
        rtTxt.offsetMin = Vector2.zero;
        rtTxt.offsetMax = Vector2.zero;

        Text t = txtObj.AddComponent<Text>();
        t.text = texto;
        t.fontSize = Mathf.Clamp(Mathf.RoundToInt(alto * 0.32f), 11, 16);
        t.fontStyle = FontStyle.Bold;
        t.color = ColorDorado;
        t.alignment = TextAnchor.MiddleCenter;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

        // Configuración y micro-interacción Negro y Dorado
        UIThemeApply.ConfigurarBotonMinimalista(btn);

        return btnObj;
    }

    private void ActualizarPanelesCanvas()
    {
        if (panelMenuPrincipal != null) panelMenuPrincipal.SetActive(estadoActual == EstadoMenu.MenuPrincipal);
        if (panelOpciones != null) panelOpciones.SetActive(estadoActual == EstadoMenu.Opciones);
        if (panelPausa != null) panelPausa.SetActive(estadoActual == EstadoMenu.Pausa);
        if (panelHUD != null) panelHUD.SetActive(estadoActual == EstadoMenu.EnPartida);
    }

    private void ActualizarTurnoCanvas()
    {
        if (generadorTablero == null || textoTurnoHUD == null) return;
        bool blancas = generadorTablero.EsTurnoBlancas;
        textoTurnoHUD.text = blancas ? "T U R N O :   B L A N C A S" : "T U R N O :   A Z U L E S";
        textoTurnoHUD.color = blancas ? ColorDorado : ColorDoradoMetalico;
    }

    // --- RENDERIZADO ONGUI (FALLBACK MINIMALISTA NEGRO Y DORADO) ---

    void OnGUI()
    {
        // Si el Canvas UI está activo y visible, no dibujamos OnGUI encima
        if (usarCanvasUI && canvasMenu != null && canvasMenu.gameObject.activeInHierarchy)
            return;

        InicializarEstilosOnGUI();

        switch (estadoActual)
        {
            case EstadoMenu.MenuPrincipal:
                DibujarFondoNegro();
                DibujarMenuPrincipalOnGUI();
                break;

            case EstadoMenu.EnPartida:
                DibujarHUDPartidaOnGUI();
                break;

            case EstadoMenu.Pausa:
                DibujarFondoNegro();
                DibujarMenuPausaOnGUI();
                break;

            case EstadoMenu.Opciones:
                DibujarFondoNegro();
                DibujarMenuOpcionesOnGUI();
                break;
        }
    }

    private void DibujarFondoNegro()
    {
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), texFondoNegro);
    }

    private void DibujarMenuPrincipalOnGUI()
    {
        float ancho = Mathf.Min(480f, Screen.width * 0.9f);
        float alto = 530f;
        Rect rect = new Rect((Screen.width - ancho) / 2f, (Screen.height - alto) / 2f, ancho, alto);

        GUI.Box(rect, GUIContent.none, estiloPanel);
        GUILayout.BeginArea(rect);
        GUILayout.Space(30);

        GUILayout.Label("A  J  E  D  R  E  Z", estiloTitulo);
        GUILayout.Label("D  U  E  L  O     1   V  S   1", estiloSubtitulo);
        GUILayout.Space(15);
        DibujarLineaDivisoriaOnGUI(320);
        GUILayout.Space(35);

        if (DibujarBotonMinimalista("btn_jugar", "J  U  G  A  R     1   V  S   1", 62))
        {
            IniciarPartida1vs1();
        }

        GUILayout.Space(18);

        if (DibujarBotonMinimalista("btn_opciones", "O  P  C  I  O  N  E  S", 56))
        {
            AbrirOpciones();
        }

        GUILayout.Space(18);

        if (DibujarBotonMinimalista("btn_salir", "S  A  L  I  R", 56))
        {
            SalirDelJuego();
        }

        GUILayout.FlexibleSpace();
        GUILayout.Label("P U R O   M I N I M A L I S M O", estiloEtiqueta);
        GUILayout.Space(16);
        GUILayout.EndArea();
    }

    private void DibujarMenuPausaOnGUI()
    {
        float ancho = Mathf.Min(480f, Screen.width * 0.9f);
        float alto = 540f;
        Rect rect = new Rect((Screen.width - ancho) / 2f, (Screen.height - alto) / 2f, ancho, alto);

        GUI.Box(rect, GUIContent.none, estiloPanel);
        GUILayout.BeginArea(rect);
        GUILayout.Space(25);

        GUILayout.Label("P  A  R  T  I  D  A     E  N     P  A  U  S  A", estiloTitulo);
        if (generadorTablero != null)
        {
            string bando = generadorTablero.EsTurnoBlancas ? "B L A N C A S" : "A Z U L E S";
            GUILayout.Label($"T U R N O   A C T U A L :   {bando}", estiloSubtitulo);
        }
        GUILayout.Space(10);
        DibujarLineaDivisoriaOnGUI(320);
        GUILayout.Space(25);

        if (DibujarBotonMinimalista("btn_reanudar", "C  O  N  T  I  N  U  A  R", 52))
        {
            ReanudarPartida();
        }
        GUILayout.Space(14);

        if (DibujarBotonMinimalista("btn_reiniciar", "R  E  I  N  I  C  I  A  R", 50))
        {
            ReiniciarPartida();
        }
        GUILayout.Space(14);

        if (DibujarBotonMinimalista("btn_pausa_opc", "O  P  C  I  O  N  E  S", 50))
        {
            AbrirOpciones();
        }
        GUILayout.Space(14);

        if (DibujarBotonMinimalista("btn_pausa_menu", "M  E  N  Ú     P  R  I  N  C  I  P  A  L", 50))
        {
            VolverAlMenuPrincipal();
        }
        GUILayout.Space(14);

        if (DibujarBotonMinimalista("btn_pausa_salir", "S  A  L  I  R", 46))
        {
            SalirDelJuego();
        }

        GUILayout.Space(18);
        GUILayout.EndArea();
    }

    private void DibujarMenuOpcionesOnGUI()
    {
        float ancho = Mathf.Min(560f, Screen.width * 0.92f);
        float alto = 580f;
        Rect rect = new Rect((Screen.width - ancho) / 2f, (Screen.height - alto) / 2f, ancho, alto);

        GUI.Box(rect, GUIContent.none, estiloPanel);
        GUILayout.BeginArea(rect);
        GUILayout.Space(25);

        GUILayout.Label("C  O  N  F  I  G  U  R  A  C  I  Ó  N", estiloTitulo);
        GUILayout.Label("P  A  L  E  T  A     Y     A  J  U  S  T  E  S", estiloSubtitulo);
        GUILayout.Space(10);
        DibujarLineaDivisoriaOnGUI(400);
        GUILayout.Space(25);

        // Volumen
        GUILayout.BeginHorizontal();
        GUILayout.Label("VOLUMEN GENERAL", estiloEtiqueta, GUILayout.Width(200));
        GUILayout.Label($"{Mathf.RoundToInt(volumenGeneral * 100)}%", estiloEtiqueta, GUILayout.Width(50));

        if (GUILayout.Button("-", estiloBoton, GUILayout.Width(40), GUILayout.Height(30)))
        {
            CambiarVolumen(volumenGeneral - 0.1f);
        }
        float nuevoVol = GUILayout.HorizontalSlider(volumenGeneral, 0f, 1f, GUILayout.Height(30));
        if (Mathf.Abs(nuevoVol - volumenGeneral) > 0.01f)
        {
            CambiarVolumen(nuevoVol);
        }
        if (GUILayout.Button("+", estiloBoton, GUILayout.Width(40), GUILayout.Height(30)))
        {
            CambiarVolumen(volumenGeneral + 0.1f);
        }

        string txtMute = volumenGeneral > 0f ? "MUTE" : "ACTIVAR";
        if (GUILayout.Button(txtMute, estiloBoton, GUILayout.Width(75), GUILayout.Height(30)))
        {
            CambiarVolumen(volumenGeneral > 0f ? 0f : 0.8f);
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(20);

        // Pantalla Completa
        GUILayout.BeginHorizontal();
        GUILayout.Label("PANTALLA", estiloEtiqueta, GUILayout.Width(200));
        string txtPantalla = Screen.fullScreen ? "PANTALLA COMPLETA" : "MODO VENTANA";
        if (GUILayout.Button(txtPantalla, estiloBoton, GUILayout.Height(36)))
        {
            AlternarPantallaCompleta();
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(20);

        // Temas de Tablero
        GUILayout.Label("ESTILO DEL TABLERO", estiloEtiqueta);
        GUILayout.Space(6);
        GUILayout.BeginHorizontal();
        for (int i = 0; i < temasDisponibles.Length; i++)
        {
            bool activo = (temaSeleccionado == i);
            string etiquetaTema = activo ? $"[ {temasDisponibles[i].nombre.ToUpperInvariant()} ]" : temasDisponibles[i].nombre.ToUpperInvariant();
            if (GUILayout.Button(etiquetaTema, estiloBoton, GUILayout.Height(38)))
            {
                SeleccionarTema(i);
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(35);

        if (DibujarBotonMinimalista("btn_opc_volver", "G  U  A  R  D  A  R     Y     V  O  L  V  E  R", 52))
        {
            VolverDeOpciones();
        }

        GUILayout.Space(18);
        GUILayout.EndArea();
    }

    private void DibujarHUDPartidaOnGUI()
    {
        float anchoBoton = 130f;
        float altoBoton = 40f;
        rectBotonHUD = new Rect(Screen.width - anchoBoton - 25, 20, anchoBoton, altoBoton);

        if (GUI.Button(rectBotonHUD, "M  E  N  Ú", estiloBotonHUD))
        {
            AlternarPausa();
        }

        if (generadorTablero != null && !generadorTablero.JuegoTerminado)
        {
            bool blancas = generadorTablero.EsTurnoBlancas;
            string txt = blancas ? "T U R N O :   B L A N C A S" : "T U R N O :   A Z U L E S";
            estiloBadgeTurno.normal.textColor = blancas ? ColorDorado : ColorDoradoMetalico;
            GUI.Box(new Rect(25, 20, 220, 40), txt, estiloBadgeTurno);
        }
    }

    private bool DibujarBotonMinimalista(string id, string texto, float alto)
    {
        Rect r = GUILayoutUtility.GetRect(new GUIContent(texto), estiloBoton, GUILayout.Height(alto));
        bool hover = r.Contains(Event.current.mousePosition);

        // Suavizado hover de 0.25s
        if (!animacionesHover.ContainsKey(id)) animacionesHover[id] = 0f;
        float objetivo = hover ? 1f : 0f;
        animacionesHover[id] = Mathf.MoveTowards(animacionesHover[id], objetivo, Time.unscaledDeltaTime / 0.25f);
        float t = animacionesHover[id];

        // Mezclar fondo entre #141414 y #D4AF37
        Color colorFondoActual = Color.Lerp(ColorBotonNormal, ColorDorado, t);
        Color colorTextoActual = Color.Lerp(ColorDorado, ColorTextoHover, t);

        // Reducción ligera de escala al 95% al hacer click
        bool click = false;
        if (hover && Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            click = true;
            colorFondoActual = ColorDoradoOscuro;
        }

        // Dibujar textura de fondo con color tint
        Color guiColorOrig = GUI.color;
        GUI.color = colorFondoActual;
        GUI.DrawTexture(r, texBotonBase);
        GUI.color = guiColorOrig;

        // Dibujar texto en dorado o negro según hover
        GUIStyle st = new GUIStyle(estiloBoton);
        st.normal.textColor = colorTextoActual;
        GUI.Label(r, texto, st);

        if (hover)
        {
            // Forzar repintado para animar suavemente el hover
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            #endif
        }

        return GUI.Button(r, GUIContent.none, GUIStyle.none) || click;
    }

    private void DibujarLineaDivisoriaOnGUI(float ancho)
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        Rect r = GUILayoutUtility.GetRect(ancho, 1);
        GUI.color = new Color(ColorDorado.r, ColorDorado.g, ColorDorado.b, 0.4f);
        GUI.DrawTexture(r, texLineaDorada);
        GUI.color = Color.white;
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    private void InicializarEstilosOnGUI()
    {
        if (estilosInicializados) return;

        texFondoNegro = CrearTextura(ColorFondoNegro);
        texCardPanel = CrearTexturaConBorde(ColorSuperficieCard, ColorDoradoMetalico, 1);
        texBotonBase = CrearTexturaConBorde(Color.white, ColorDorado, 2);
        texBotonHoverDorado = CrearTextura(ColorDorado);
        texBotonClickOscuro = CrearTextura(ColorDoradoOscuro);
        texLineaDorada = CrearTextura(ColorDorado);

        estiloPanel = new GUIStyle(GUI.skin.box);
        estiloPanel.normal.background = texCardPanel;
        estiloPanel.padding = new RectOffset(25, 25, 25, 25);

        estiloTitulo = new GUIStyle(GUI.skin.label);
        estiloTitulo.fontSize = 28;
        estiloTitulo.fontStyle = FontStyle.Bold;
        estiloTitulo.alignment = TextAnchor.MiddleCenter;
        estiloTitulo.normal.textColor = ColorDorado;

        estiloSubtitulo = new GUIStyle(GUI.skin.label);
        estiloSubtitulo.fontSize = 13;
        estiloSubtitulo.fontStyle = FontStyle.Normal;
        estiloSubtitulo.alignment = TextAnchor.MiddleCenter;
        estiloSubtitulo.normal.textColor = ColorDoradoMetalico;

        estiloBoton = new GUIStyle(GUI.skin.button);
        estiloBoton.fontSize = 14;
        estiloBoton.fontStyle = FontStyle.Bold;
        estiloBoton.alignment = TextAnchor.MiddleCenter;
        estiloBoton.normal.background = texBotonBase;
        estiloBoton.normal.textColor = ColorDorado;

        estiloEtiqueta = new GUIStyle(GUI.skin.label);
        estiloEtiqueta.fontSize = 13;
        estiloEtiqueta.fontStyle = FontStyle.Bold;
        estiloEtiqueta.alignment = TextAnchor.MiddleCenter;
        estiloEtiqueta.normal.textColor = ColorDoradoMetalico;

        estiloBotonHUD = new GUIStyle(estiloBoton);
        estiloBotonHUD.fontSize = 13;

        estiloBadgeTurno = new GUIStyle(GUI.skin.box);
        estiloBadgeTurno.normal.background = texCardPanel;
        estiloBadgeTurno.fontSize = 12;
        estiloBadgeTurno.fontStyle = FontStyle.Bold;
        estiloBadgeTurno.alignment = TextAnchor.MiddleCenter;

        estilosInicializados = true;
    }

    private Texture2D CrearTextura(Color color)
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, color);
        tex.Apply();
        return tex;
    }

    private Texture2D CrearTexturaConBorde(Color fondo, Color borde, int grosor)
    {
        int ancho = 32;
        int alto = 32;
        Texture2D tex = new Texture2D(ancho, alto);
        Color[] pixeles = new Color[ancho * alto];

        for (int y = 0; y < alto; y++)
        {
            for (int x = 0; x < ancho; x++)
            {
                bool enBorde = (x < grosor || x >= ancho - grosor || y < grosor || y >= alto - grosor);
                pixeles[y * ancho + x] = enBorde ? borde : fondo;
            }
        }

        tex.SetPixels(pixeles);
        tex.Apply();
        return tex;
    }

    private void LiberarTexturas()
    {
        Destroy(texFondoNegro);
        Destroy(texCardPanel);
        Destroy(texBotonBase);
        Destroy(texBotonHoverDorado);
        Destroy(texBotonClickOscuro);
        Destroy(texLineaDorada);
    }
}
