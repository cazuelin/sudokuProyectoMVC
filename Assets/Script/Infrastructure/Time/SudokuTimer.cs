using TMPro;
using UnityEngine;
public class SudokuTimer : MonoBehaviour//Este script controla el tiempo de la partida y actualiza el texto visual del timer.
{
    [SerializeField] TMP_Text timerText;//Referencia al texto donde se muestra el tiempo.
    //Por ejemplo, en pantalla puede verse así: 00:35
    //Este campo se asigna desde el Inspector de Unity arrastrando el texto del timer.
    float timeElapsed;//Guarda el tiempo transcurrido en segundos. Es float porque Time.deltaTime usa decimales.
    //Ejemplo: timeElapsed = 65.4f    Eso significa 65.4 segundos.  Visualmente se mostraría como:  01:05
    bool running;//Indica si el timer está corriendo o detenido. Si está en true, el tiempo aumenta. Si está en false, el tiempo no cambia.
    int lastSecond = -1;//Guarda el último segundo que se mostró en pantalla. 
    //Sirve para no actualizar el texto cada frame, sino solo cuando cambia el segundo.
    //Ejemplo: Si timeElapsed va de: 10.1 , 10.2 , 10.3 ..... sigue siendo segundo 10, entonces no necesita redibujar el texto muchas veces.
    void Update()//Update se ejecuta cada frame.
    {
        if (!running) return;//Si el timer no está corriendo, se sale. 
        //!running significa: pregunta si running es false.

        //si no es falso entonces pasa aca
        timeElapsed += Time.deltaTime;//Suma el tiempo que pasó desde el último frame.
        //Ejemplo a 60 FPS: Time.deltaTime ≈ 0.016     Entonces va acumulando: 0.016 , 0.032 , 0.048 .......
        int currentSecond = Mathf.FloorToInt(timeElapsed);//Convierte el tiempo a segundos enteros.
        //Mathf.FloorToInt redondea hacia abajo. ejemplo : timeElapsed = 12.9f entonces currentSecond = 12
        if (currentSecond != lastSecond)//Pregunta si el segundo actual es diferente al último mostrado.
        {
            lastSecond = currentSecond;//Guarda el nuevo segundo.
            UpdateTimerText();//actualiza el texto
            //Así el texto solo se refresca una vez por segundo.
        }
    }
    void UpdateTimerText()//Esta función convierte segundos a formato minutos/segundos.
    {
        int totalSeconds = Mathf.FloorToInt(timeElapsed);//Toma el tiempo total como entero.
        //Ejemplo: timeElapsed = 125.8f.  lo pasa a totalSeconds = 125
        int minutes = totalSeconds / 60;//Calcula los minutos.
        //Ejemplo: 125 / 60 = 2. Entonces son 2 minutos.
        int seconds = totalSeconds % 60;//Calcula los segundos restantes.  % es módulo, devuelve el resto de una división.
        //Ejemplo: 125 % 60 = 5  entonces el total seria 2 minutos y 5 segundos que es lo mismo que 60 * 2 son 120 y de 125 - 120 son 5
        timerText.text = $"{minutes:00}:{seconds:00}";//Actualiza el texto visual. El formato :00 significa que siempre muestra dos dígitos.
        //Ejemplos: 0 minutos, 5 segundos  -> 00:05
        //2 minutos, 5 segundos  -> 02:05
    }
    public void StartTimer() => running = true;//Inicia el timer. Cuando running pasa a true, Update empieza a sumar tiempo.
    public void StopTimer() => running = false;//Detiene el timer. No reinicia el tiempo. Solo pausa el conteo.
    //ejemplo : timeElapsed = 80 y running = false. El tiempo se queda en 80 segundos hasta que vuelvas a llamar StartTimer.
    public float GetTime() => timeElapsed;//Devuelve el tiempo actual.Se usa para guardar la partida:time = timer.GetTime()

    public void SetTime(float time)//Restaura el tiempo.
        //Se usa cuando cargas una partida guardada.
    {
        timeElapsed = time;//Guarda el tiempo cargado.
        UpdateTimerText();//Actualiza el texto para mostrar ese tiempo en pantalla.
    }
    public void ResetTime()//Reinicia el timer.
    {
        timeElapsed = 0f;//Vuelve el tiempo a cero.
        lastSecond = -1;//Reinicia el último segundo mostrado.
        running = false;//Deja el timer detenido.
        UpdateTimerText();//Actualiza el texto a: 00:00
    }
}
