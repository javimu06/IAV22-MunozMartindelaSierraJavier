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
     * El controlador es el 'capit�n general' del ej�rcito. 
     * Se trata de una clase abstracta para todos los controladores, sea�n jugadores humanos o bots.
     */
    public abstract class RTSController : MonoBehaviour
    {
        // El nombre del controlador 
        public string Name { get; protected set; }

        // Devuelve el nombre del autor del controlador (Ej. n�mero de grupo + nombres de alumnos) 
        public string Author { get; protected set; }
    }
}