using UnityEngine;

public class GeneradorTablero : MonoBehaviour
{
    // Esta variable nos permitirá conectar nuestro Prefab desde Unity
    public GameObject casillaPrefab;

    // Colores para las casillas (tonos tierra para tu temática de leones)
    public Color colorClaro = new Color(0.9f, 0.8f, 0.6f); 
    public Color colorOscuro = new Color(0.5f, 0.3f, 0.1f); 

    void Start()
    {
        GenerarCuadricula();
    }

    void GenerarCuadricula()
    {
        // Un doble bucle for para crear una matriz de 8 columnas (X) y 8 filas (Y)
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                // 1. Calculamos la posición exacta
                Vector2 posicion = new Vector2(x, y);
                
                // 2. Instanciamos (clonamos) la casilla en esa posición
                GameObject nuevaCasilla = Instantiate(casillaPrefab, posicion, Quaternion.identity);

                // 3. Le ponemos nombre (ej: "Casilla 0,0") y la agrupamos dentro de TableroManager
                nuevaCasilla.name = $"Casilla {x},{y}";
                nuevaCasilla.transform.parent = this.transform;

                // 4. Cambiamos su color para hacer el patrón de ajedrez
                SpriteRenderer renderizador = nuevaCasilla.GetComponent<SpriteRenderer>();
                
                // Si la suma de X e Y es impar, es clara; si es par, es oscura
                bool esClara = (x + y) % 2 != 0;
                renderizador.color = esClara ? colorClaro : colorOscuro;
            }
        }

        // Centramos la cámara para que enfoque el tablero (de 0 a 7, el centro matemático es 3.5)
        Camera.main.transform.position = new Vector3(3.5f, 3.5f, -10f);
    }
}