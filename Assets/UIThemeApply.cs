using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Script auxiliar que aplica automáticamente el tema visual Minimalista Negro y Dorado
/// a todos los componentes de Canvas (Buttons, Images, Text y TextMeshProUGUI) en la escena.
/// </summary>
[ExecuteAlways]
public class UIThemeApply : MonoBehaviour
{
    [Header("Paleta Minimalista Negro y Dorado")]
    public static readonly Color ColorFondoPrincipal = new Color(0.039f, 0.039f, 0.039f, 0.96f);    // #0A0A0A Negro profundo
    public static readonly Color ColorPanelSuperficie = new Color(0.067f, 0.067f, 0.067f, 0.98f);   // #111111 Superficie oscura
    public static readonly Color ColorBotonFondo = new Color(0.078f, 0.078f, 0.078f, 0.95f);        // #141414 Negro carbón
    public static readonly Color ColorDoradoElegante = new Color(0.831f, 0.686f, 0.216f, 1f);       // #D4AF37 Dorado clásico
    public static readonly Color ColorDoradoMetalico = new Color(0.773f, 0.627f, 0.349f, 1f);       // #C5A059 Dorado metálico
    public static readonly Color ColorDoradoOscuro = new Color(0.541f, 0.435f, 0.114f, 1f);         // #8A6F1D Dorado click
    public static readonly Color ColorTextoEnHover = new Color(0.039f, 0.039f, 0.039f, 1f);         // #0A0A0A Texto en hover

    [Header("Configuración")]
    public bool aplicarAlIniciar = true;

    private static Sprite cacheSpriteBotonBorde;
    private static Sprite cacheSpritePanelBorde;

    void Start()
    {
        if (aplicarAlIniciar)
        {
            AplicarTemaCompleto();
        }
    }

    [ContextMenu("Aplicar Tema Negro y Dorado")]
    public void AplicarTemaCompleto()
    {
        ActualizarBotones();
        ActualizarTextos();
        ActualizarFondos();
    }

    public static void ConfigurarBotonMinimalista(Button boton)
    {
        if (boton == null) return;

        // Configuración de Color Tint con transición de 0.25 segundos
        boton.transition = Selectable.Transition.ColorTint;

        ColorBlock cb = boton.colors;
        cb.normalColor = ColorBotonFondo;
        cb.highlightedColor = ColorDoradoElegante;
        cb.pressedColor = ColorDoradoOscuro;
        cb.selectedColor = ColorBotonFondo;
        cb.disabledColor = new Color(0.15f, 0.15f, 0.15f, 0.5f);
        cb.fadeDuration = 0.25f; // Transición suave entre 0.2s y 0.3s
        boton.colors = cb;

        // Borde fino dorado de 1px a 2px en la imagen del botón
        Image img = boton.GetComponent<Image>();
        if (img != null)
        {
            if (cacheSpriteBotonBorde == null)
            {
                cacheSpriteBotonBorde = GenerarSpriteConBorde(32, 32, 2, Color.white, ColorDoradoElegante);
            }
            img.sprite = cacheSpriteBotonBorde;
            img.type = Image.Type.Sliced;
        }

        // Micro-interacción: texto negro en hover y escala al 95% al pulsar
        MicroInteraccionBoton micro = boton.GetComponent<MicroInteraccionBoton>();
        if (micro == null)
        {
            micro = boton.gameObject.AddComponent<MicroInteraccionBoton>();
        }
        micro.colorTextoNormal = ColorDoradoElegante;
        micro.colorTextoHover = ColorTextoEnHover;
        micro.duracionTransicion = 0.25f;
        micro.escalaPressed = 0.95f;
        micro.BuscarTextos();
        micro.AplicarColorTexto(ColorDoradoElegante);
    }

    private void ActualizarBotones()
    {
        Button[] botones = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Button b in botones)
        {
            ConfigurarBotonMinimalista(b);
        }
    }

    private void ActualizarTextos()
    {
        // TextMeshProUGUI
        TextMeshProUGUI[] tmps = FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (TextMeshProUGUI tmp in tmps)
        {
            // Si es hijo de un botón
            if (tmp.GetComponentInParent<Button>() != null)
            {
                tmp.color = ColorDoradoElegante;
                tmp.characterSpacing = 8f; // Letter spacing amplio y elegante
            }
            else if (tmp.fontSize >= 32)
            {
                // Título
                tmp.color = ColorDoradoElegante;
                tmp.characterSpacing = 12f;
            }
            else
            {
                // Subtítulos o etiquetas
                tmp.color = ColorDoradoMetalico;
                tmp.characterSpacing = 5f;
            }
        }

        // Text estándar de Unity
        Text[] textos = FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Text t in textos)
        {
            if (t.GetComponentInParent<Button>() != null)
            {
                t.color = ColorDoradoElegante;
            }
            else if (t.fontSize >= 28)
            {
                t.color = ColorDoradoElegante;
            }
            else
            {
                t.color = ColorDoradoMetalico;
            }
        }
    }

    private void ActualizarFondos()
    {
        Image[] imagenes = FindObjectsByType<Image>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Image img in imagenes)
        {
            // Ignorar las imágenes de los botones
            if (img.GetComponent<Button>() != null) continue;

            string nombre = img.gameObject.name.ToLower();
            if (nombre.Contains("fondo") || nombre.Contains("overlay") || nombre.Contains("background"))
            {
                img.color = ColorFondoPrincipal;
            }
            else if (nombre.Contains("panel") || nombre.Contains("card") || nombre.Contains("menu"))
            {
                img.color = ColorPanelSuperficie;
                if (cacheSpritePanelBorde == null)
                {
                    cacheSpritePanelBorde = GenerarSpriteConBorde(32, 32, 1, Color.white, ColorDoradoMetalico);
                }
                img.sprite = cacheSpritePanelBorde;
                img.type = Image.Type.Sliced;
            }
        }
    }

    public static Sprite GenerarSpriteConBorde(int ancho, int alto, int grosor, Color fondo, Color borde)
    {
        Texture2D tex = new Texture2D(ancho, alto, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Color[] pixeles = new Color[ancho * alto];

        for (int y = 0; y < alto; y++)
        {
            for (int x = 0; x < ancho; x++)
            {
                bool esBorde = (x < grosor || x >= ancho - grosor || y < grosor || y >= alto - grosor);
                pixeles[y * ancho + x] = esBorde ? borde : fondo;
            }
        }

        tex.SetPixels(pixeles);
        tex.Apply();

        return Sprite.Create(
            tex,
            new Rect(0, 0, ancho, alto),
            new Vector2(0.5f, 0.5f),
            100f,
            0,
            SpriteMeshType.FullRect,
            new Vector4(grosor + 1, grosor + 1, grosor + 1, grosor + 1) // 9-sliced
        );
    }
}

