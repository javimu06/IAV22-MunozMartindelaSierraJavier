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
     * El controlador es el 'capitán general' del ejército. 
     * Se trata de una clase abstracta para todos los controladores, seaán jugadores humanos o bots.
     */
    public abstract class RTSController : MonoBehaviour
    {
        // El nombre del controlador 
        public string Name { get; protected set; }

        // Devuelve el nombre del autor del controlador (Ej. número de grupo + nombres de alumnos) 
        public string Author { get; protected set; }
    }
}