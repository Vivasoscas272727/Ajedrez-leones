using UnityEngine;

public class GeneradorTablero : MonoBehaviour
{
    public GameObject casillaPrefab;

    // Colores oscuros y elegantes para que tus leones destaquen
    public Color colorClaro = new Color(0.85f, 0.85f, 0.85f); 
    public Color colorOscuro = new Color(0.3f, 0.4f, 0.5f); 

    void Start()
    {
        GenerarCuadricula();
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
                
                bool esClara = (x + y) % 2 != 0;
                renderizador.color = esClara ? colorClaro : colorOscuro;
            }
        }
        Camera.main.transform.position = new Vector3(3.5f, 3.5f, -10f);
    }
}