/*    
   Copyright (C) 2020 Federico Peinado
   http://www.federicopeinado.com

   Este fichero forma parte del material de la asignatura Inteligencia Artificial para Videojuegos.
   Esta asignatura se imparte en la Facultad de Inform�tica de la Universidad Complutense de Madrid (Espa�a).

   Autores originales: Opsive (Behavior Designer Samples)
   Revisi�n: Federico Peinado 
   Contacto: email@federicopeinado.com
*/
using UnityEngine;

namespace es.ucm.fdi.iav.rts
{
    /* 
     * La instalaci�n de procesamiento hace las veces de refiner�a del ej�rcito. Sirve para que las unidades de extracci�n vaya all� a descargar los recursos extraidos, para convertirlos en dinero.
     * La cantidad de recursos se expresa con un n�mero entero.
     * 
     * Posibles mejoras: 
     * - Darle alguna utilidad, para almacenar los recursos o convertirlos en dinero y almacenar ese dinero (de modo que si destruyen la instalaci�n se pierde ese dinero)
     * - Darle alguna utilidad a los puntos de descarga y de espera...
     */ 
    public class ProcessingFacility : Facility
    {  
        // La transformada donde tiene que entrar a descargar la unidad extractora, incluye posici�n, rotaci�n y vectores b�sicos para orientarse
        [SerializeField] private Transform _unloadingTransform = null;
        public Transform UnloadingTransform { get { return _unloadingTransform; } }

        // La cantidad de recursos que hay en la instalaci�n de procesamiento a�n por procesar 
        public int Resources { get; set; } = 0;
    }
}