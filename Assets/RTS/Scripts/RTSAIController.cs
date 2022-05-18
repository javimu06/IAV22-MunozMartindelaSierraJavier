/*    
   Copyright (C) 2020 Federico Peinado
   http://www.federicopeinado.com

   Este fichero forma parte del material de la asignatura Inteligencia Artificial para Videojuegos.
   Esta asignatura se imparte en la Facultad de Inform�tica de la Universidad Complutense de Madrid (Espa�a).

   Autores originales: Opsive (Behavior Designer Samples)
   Revisi�n: Federico Peinado 
   Contacto: email@federicopeinado.com
*/
using System.Collections;
using UnityEngine;

namespace es.ucm.fdi.iav.rts
{
    /* 
     * El controlador de IA permite implementar bots t�cticos para jugar autom�ticamente al juego, usando una corrutina para gestionar su ejecuci�n en el tiempo.
     * Se trata de una clase abstracta para todos los controladores de bots espec�ficos que programemos.
     * 
     * Posibles mejoras:
     * - Tener tiempos separados para percibir/sentir/observar y para actuar/dar �rdenes, tal vez con un margen de diferencia en ms que se mantenga, en vez de s�lo una de pensamiento
     * - Pensar si es interesante que deban implementarse m�todos concretos para que llame el gestor del juego, o suscribirse autom�ticamente a eventos del mismo
     * - Permitir cambiar el thinkTime desde la clase espec�fica que implemente esto
     */
    public abstract class RTSAIController : RTSController
    {
        // Tiempo en segundos que tarda la corrutina de pensamiento (percibir/actuar) en ejecutarse.
        [SerializeField] private float thinkTime = 0.5f;
        public float ThinkTime { set { thinkTime = value; } get { return thinkTime; } }

        // Indica si es la primera vez que se va a ejecutar el Update
        private bool FirstTime { get; set; } = true;

        // Indica si debe detenerse el ciclo de pensamiento del controlador
        protected bool Stop { get; set; } = false;



        // El m�todo para actualizar no lo deber�amos usar si ya se est� utilizando la corrutina responsable del pensamiento (percibir/actuar), por eso est� privado.
        // En este caso arranca la corrutina de pensamiento (percibir/actuar) del controlador, s�lo una vez (la primera llamada al Update) 
        private void Update()
        {
            if (FirstTime)
            {
               
                StartCoroutine(Thinking());
                FirstTime = false;
            }
        }

        // Corrutina que llama a 'pensar' regularmente, cada cierto tiempo de pensamiento. Se repite indefinidamente
        private IEnumerator Thinking()
        {
            while (!Stop)
            {
                Think();
                // Espera un cierto tiempo antes del siguiente pensamiento
                yield return new WaitForSeconds(ThinkTime);
            }
        }

        // El m�todo de pensar que tiene que sobreescribir e implementar el bot, para percibir (hacer mapas de influencia, etc.) y luego actuar.
        protected abstract void Think();
    }
}


