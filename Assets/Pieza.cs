using UnityEngine;

public enum TipoPieza { Peon, Torre, Caballo, Alfil, Reina, Rey }

public class Pieza : MonoBehaviour
{
    public TipoPieza tipo;
    public bool esBlanca;
    public int xActual;
    public int yActual;
    public bool seHaMovido = false;

    // Variables para la animación suave
    private Vector3 posicionObjetivo;
    private float velocidadMovimiento = 12f;

    void Update()
    {
        // Desliza la pieza suavemente hacia su casilla destino en cada frame
        transform.position = Vector3.Lerp(transform.position, posicionObjetivo, Time.deltaTime * velocidadMovimiento);
    }

    public void Configurar(TipoPieza nuevoTipo, bool blanca, int x, int y)
    {
        tipo = nuevoTipo;
        esBlanca = blanca;
        xActual = x;
        yActual = y;
        
        // Al iniciar el juego aparece directo en su sitio
        posicionObjetivo = new Vector3(x, y, -1f);
        transform.position = posicionObjetivo;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingOrder = 1;
        }
    }

    public void Posicionar(int x, int y, bool registrarMovimiento = true)
    {
        if (registrarMovimiento) seHaMovido = true;
        xActual = x;
        yActual = y;
        
        // Cambiamos el objetivo para que el Update() la deslice suavemente hasta allá
        posicionObjetivo = new Vector3(x, y, -1f);
    }

    public bool EsMovimientoValido(int destinoX, int destinoY, Pieza[,] tablero)
    {
        if (destinoX == xActual && destinoY == yActual) return false;

        Pieza piezaDestino = tablero[destinoX, destinoY];
        if (piezaDestino != null && piezaDestino.esBlanca == this.esBlanca) return false;

        int dx = destinoX - xActual;
        int dy = destinoY - yActual;
        int absDx = Mathf.Abs(dx);
        int absDy = Mathf.Abs(dy);

        switch (tipo)
        {
            case TipoPieza.Peon:
                int direccion = esBlanca ? 1 : -1;
                if (dx == 0 && dy == direccion && piezaDestino == null)
                    return true;
                if (dx == 0 && dy == 2 * direccion && !seHaMovido && piezaDestino == null && tablero[xActual, yActual + direccion] == null)
                    return true;
                if (absDx == 1 && dy == direccion && piezaDestino != null && piezaDestino.esBlanca != this.esBlanca)
                    return true;
                return false;

            case TipoPieza.Torre:
                if (dx == 0 || dy == 0)
                    return CaminoDespejado(destinoX, destinoY, tablero);
                return false;

            case TipoPieza.Caballo:
                return (absDx == 1 && absDy == 2) || (absDx == 2 && absDy == 1);

            case TipoPieza.Alfil:
                if (absDx == absDy)
                    return CaminoDespejado(destinoX, destinoY, tablero);
                return false;

            case TipoPieza.Reina:
                if (dx == 0 || dy == 0 || absDx == absDy)
                    return CaminoDespejado(destinoX, destinoY, tablero);
                return false;

            case TipoPieza.Rey:
                return absDx <= 1 && absDy <= 1;
        }

        return false;
    }

    private bool CaminoDespejado(int destinoX, int destinoY, Pieza[,] tablero)
    {
        int pasoX = destinoX == xActual ? 0 : (destinoX > xActual ? 1 : -1);
        int pasoY = destinoY == yActual ? 0 : (destinoY > yActual ? 1 : -1);

        int x = xActual + pasoX;
        int y = yActual + pasoY;

        while (x != destinoX || y != destinoY)
        {
            if (tablero[x, y] != null) return false;
            x += pasoX;
            y += pasoY;
        }
        return true;
    }
}