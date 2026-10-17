using UnityEngine;

/// <summary>
/// Adaptador del Tablero para compatibilidad con la escena y UI existente (MenuPrincipal).
/// Hereda toda la lógica matricial 8x8, simulación y reglas especiales de BoardManager,
/// y se sincroniza automáticamente con GameManager.
/// </summary>
public class GeneradorTablero : BoardManager
{
    // Propiedades expuestas para MenuPrincipal
    public bool EsTurnoBlancas => (GameManager.Instance != null) ? GameManager.Instance.EsTurnoBlancas : true;
    public bool JuegoTerminado => (GameManager.Instance != null) ? GameManager.Instance.JuegoTerminado : false;
    public string MensajePantalla => (GameManager.Instance != null) ? GameManager.Instance.MensajePantalla : "";

    protected override void Awake()
    {
        base.Awake();

        // Aseguramos que GameManager esté presente en la escena para gobernar turnos y reglas
        if (FindFirstObjectByType<GameManager>() == null)
        {
            gameObject.AddComponent<GameManager>();
        }

        // Aseguramos la existencia de MenuPrincipal
        if (FindFirstObjectByType<MenuPrincipal>() == null)
        {
            gameObject.AddComponent<MenuPrincipal>();
        }
    }

    private void Start()
    {
        GenerarCuadriculaVisual();
        ColocarPiezasIniciales();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ActualizarVisualesTablero();
        }
    }

    /// <summary>
    /// Reinicia el tablero y el estado de la partida a través de GameManager.
    /// </summary>
    public void ReiniciarTablero()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReiniciarPartida();
        }
        else
        {
            ColocarPiezasIniciales();
        }
    }
}