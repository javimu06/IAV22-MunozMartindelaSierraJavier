/*    
   Copyright (C) 2020 Federico Peinado
   http://www.federicopeinado.com

   Este fichero forma parte del material de la asignatura Inteligencia Artificial para Videojuegos.
   Esta asignatura se imparte en la Facultad de Inform�tica de la Universidad Complutense de Madrid (Espa�a).

   Autores originales: Opsive (Behavior Designer Samples)
   Revisi�n: Federico Peinado 
   Contacto: email@federicopeinado.com
*/ 
using BehaviorDesigner.Runtime; 

namespace es.ucm.fdi.iav.rts
{
    // Variable compartida en pizarra del tipo ProcessingFacility
    [System.Serializable]
    public class SharedProcessingFacility : SharedVariable<ProcessingFacility>
    {
        public static implicit operator SharedProcessingFacility(ProcessingFacility value) 
        { 
            return new SharedProcessingFacility { mValue = value }; 
        }
    }
}