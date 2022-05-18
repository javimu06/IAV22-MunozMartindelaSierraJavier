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
    // Variable compartida en pizarra del tipo LimitedAccess
    // Posibles mejoras: Que haya un tipo espec�fico tambi�n para los recursos extraibles
    [System.Serializable]
    public class SharedLimitedAccess : SharedVariable<LimitedAccess>
    {
        public static implicit operator SharedLimitedAccess(LimitedAccess value) 
        { 
            return new SharedLimitedAccess { mValue = value }; 
        }
    }
}