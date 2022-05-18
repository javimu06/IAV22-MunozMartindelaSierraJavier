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
using BehaviorDesigner.Runtime.Tasks;
using Tooltip = BehaviorDesigner.Runtime.Tasks.TooltipAttribute;

namespace es.ucm.fdi.iav.rts
{
    [TaskCategory("RTS")]
    [TaskDescription("Ejecuta la animaci�n especificada")]
    public class PlaySpecifiedAnimation : Action
    {
        [Tooltip("El nombre de la animaci�n que deber�a empezar a ejecutarse")]
        public string animationName = "";

        private Animation _animation;

        public override void OnAwake()
        {
            _animation = GetComponent<Animation>();
        }

        // Detiene la animaci�n actualmente en ejecuci�n, si la hubiera, y ejecuta la animaci�n especificada, devolviendo �xito inmediatamente
        public override TaskStatus OnUpdate()
        {
            if (_animation == null)
                throw new System.Exception("No hay animaciones en este personaje.");

            _animation.Stop();
            _animation.Play(animationName);
            return TaskStatus.Success;
        }
    }
}