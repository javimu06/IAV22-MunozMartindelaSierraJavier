/*    
   Copyright (C) 2020 Federico Peinado
   http://www.federicopeinado.com

   Este fichero forma parte del material de la asignatura Inteligencia Artificial para Videojuegos.
   Esta asignatura se imparte en la Facultad de Informática de la Universidad Complutense de Madrid (España).

   Autores originales: Opsive (Behavior Designer Samples)
   Revisión: Federico Peinado 
   Contacto: email@federicopeinado.com
*/
using UnityEngine;

namespace es.ucm.fdi.iav.rts
{
    /* 
     * La instalación de procesamiento hace las veces de refinería del ejército. Sirve para que las unidades de extracción vaya allí a descargar los recursos extraidos, para convertirlos en dinero.
     * La cantidad de recursos se expresa con un número entero.
     * 
     * Posibles mejoras: 
     * - Darle alguna utilidad, para almacenar los recursos o convertirlos en dinero y almacenar ese dinero (de modo que si destruyen la instalación se pierde ese dinero)
     * - Darle alguna utilidad a los puntos de descarga y de espera...
     */ 
    public class ProcessingFacility : Facility
    {  
        // La transformada donde tiene que entrar a descargar la unidad extractora, incluye posición, rotación y vectores básicos para orientarse
        [SerializeField] private Transform _unloadingTransform = null;
        public Transform UnloadingTransform { get { return _unloadingTransform; } }

        // La cantidad de recursos que hay en la instalación de procesamiento aún por procesar 
        public int Resources { get; set; } = 0;
    }
}