using System.Collections;  // Asegúrate de tener esto
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuInicial : MonoBehaviour
{
    public void Jugar()
    {
        // Inicia la carga asíncrona de la escena
        StartCoroutine(CargarEscenaAsync());
    }

    private IEnumerator CargarEscenaAsync()
    {
        // Comienza la carga de la siguiente escena en segundo plano
        AsyncOperation operacionCarga = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);

        // Mientras la escena se carga
        while (!operacionCarga.isDone)
        {
            // Puedes mostrar una barra de progreso o animación aquí si lo deseas
            yield return null;
        }
    }
        public void Controles()
    {
        // Cambiar al siguiente nivel en el índice de la escena
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 2);
    }

public void MenuIni()
{
    SceneManager.LoadScene(0);  // Aquí 0 es el índice de la escena del menú inicial
}


    public void Salir()
    {
        // Mostrar mensaje de salida
        Debug.Log("Salir...");
        
        // Salir de la aplicación
        Application.Quit();
    }
}
