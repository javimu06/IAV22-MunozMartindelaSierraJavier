/*    
   Copyright (C) 2020 Federico Peinado
   http://www.federicopeinado.com

   Este fichero forma parte del material de la asignatura Inteligencia Artificial para Videojuegos.
   Esta asignatura se imparte en la Facultad de Informática de la Universidad Complutense de Madrid (España).

   Autores originales: Opsive (Behavior Designer Samples)
   Revisión: Federico Peinado 
   Contacto: email@federicopeinado.com
*/ 
using BehaviorDesigner.Runtime; 

namespace es.ucm.fdi.iav.rts
{
    // Variable compartida en pizarra del tipo RTSControlle 
    [System.Serializable]
    public class SharedRTSController : SharedVariable<RTSController>
    {
        public static implicit operator SharedRTSController(RTSController value) 
        { 
            return new SharedRTSController { mValue = value }; 
        }
    }
}