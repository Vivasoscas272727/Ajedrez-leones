using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Proporciona micro-interacciones pulidas y minimalistas a los botones:
/// - Transición suave (0.2s - 0.3s) de color de texto (Dorado -> Negro) en Hover.
/// - Reducción de escala al 95% al hacer clic (Pressed).
/// </summary>
public class MicroInteraccionBoton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Referencias de Texto")]
    public Text textoLegado;
    public TextMeshProUGUI textoTMP;

    [Header("Paleta Negro y Dorado")]
    public Color colorTextoNormal = new Color(0.831f, 0.686f, 0.216f, 1f); // #D4AF37 Dorado clásico
    public Color colorTextoHover = new Color(0.039f, 0.039f, 0.039f, 1f);  // #0A0A0A Negro profundo

    [Header("Tiempos y Escalas")]
    [Range(0.1f, 0.5f)] public float duracionTransicion = 0.25f; // Transición de 0.2s a 0.3s
    public float escalaNormal = 1.0f;
    public float escalaHover = 1.02f;
    public float escalaPressed = 0.95f; // Reducción ligera al 95%

    private Coroutine corrutinaColor;
    private Coroutine corrutinaEscala;
    private bool estaHover = false;
    private Vector3 escalaOriginal = Vector3.one;
    private bool inicializado = false;

    void Awake()
    {
        Inicializar();
    }

    void OnEnable()
    {
        Inicializar();
        AplicarColorTexto(colorTextoNormal);
        transform.localScale = escalaOriginal * escalaNormal;
    }

    private void Inicializar()
    {
        if (inicializado) return;
        escalaOriginal = transform.localScale == Vector3.zero ? Vector3.one : transform.localScale;
        BuscarTextos();
        inicializado = true;
    }

    public void BuscarTextos()
    {
        if (textoTMP == null) textoTMP = GetComponentInChildren<TextMeshProUGUI>(true);
        if (textoLegado == null) textoLegado = GetComponentInChildren<Text>(true);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        estaHover = true;
        IniciarTransicionColor(colorTextoHover);
        IniciarTransicionEscala(escalaHover);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        estaHover = false;
        IniciarTransicionColor(colorTextoNormal);
        IniciarTransicionEscala(escalaNormal);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        IniciarTransicionEscala(escalaPressed);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        IniciarTransicionEscala(estaHover ? escalaHover : escalaNormal);
    }

    private void IniciarTransicionColor(Color destino)
    {
        if (corrutinaColor != null) StopCoroutine(corrutinaColor);
        if (gameObject.activeInHierarchy)
        {
            corrutinaColor = StartCoroutine(AnimarColorTexto(destino));
        }
        else
        {
            AplicarColorTexto(destino);
        }
    }

    private void IniciarTransicionEscala(float factorEscala)
    {
        if (corrutinaEscala != null) StopCoroutine(corrutinaEscala);
        if (gameObject.activeInHierarchy)
        {
            corrutinaEscala = StartCoroutine(AnimarEscala(escalaOriginal * factorEscala));
        }
        else
        {
            transform.localScale = escalaOriginal * factorEscala;
        }
    }

    private IEnumerator AnimarColorTexto(Color destino)
    {
        Color inicial = ObtenerColorTextoActual();
        float tiempo = 0f;

        while (tiempo < duracionTransicion)
        {
            tiempo += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(tiempo / duracionTransicion);
            float tSuave = Mathf.SmoothStep(0f, 1f, t);
            Color nuevoColor = Color.Lerp(inicial, destino, tSuave);
            AplicarColorTexto(nuevoColor);
            yield return null;
        }

        AplicarColorTexto(destino);
    }

    private IEnumerator AnimarEscala(Vector3 destino)
    {
        Vector3 inicial = transform.localScale;
        float tiempo = 0f;
        float duracionEscala = 0.12f;

        while (tiempo < duracionEscala)
        {
            tiempo += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(tiempo / duracionEscala);
            transform.localScale = Vector3.Lerp(inicial, destino, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        transform.localScale = destino;
    }

    private Color ObtenerColorTextoActual()
    {
        if (textoTMP != null) return textoTMP.color;
        if (textoLegado != null) return textoLegado.color;
        return colorTextoNormal;
    }

    public void AplicarColorTexto(Color c)
    {
        if (textoTMP != null) textoTMP.color = c;
        if (textoLegado != null) textoLegado.color = c;
    }
}

