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
using BehaviorDesigner.Runtime.Tasks;
using Tooltip = BehaviorDesigner.Runtime.Tasks.TooltipAttribute;

namespace es.ucm.fdi.iav.rts
{
    [TaskCategory("RTS")]
    [TaskDescription("Ejecuta la animación especificada")]
    public class PlaySpecifiedAnimation : Action
    {
        [Tooltip("El nombre de la animación que debería empezar a ejecutarse")]
        public string animationName = "";

        private Animation _animation;

        public override void OnAwake()
        {
            _animation = GetComponent<Animation>();
        }

        // Detiene la animación actualmente en ejecución, si la hubiera, y ejecuta la animación especificada, devolviendo éxito inmediatamente
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