/*    
   Copyright (C) 2020 Federico Peinado
   http://www.federicopeinado.com

   Este fichero forma parte del material de la asignatura Inteligencia Artificial para Videojuegos.
   Esta asignatura se imparte en la Facultad de Informática de la Universidad Complutense de Madrid (España).

   Autores originales: Opsive (Behavior Designer Samples)
   Revisión: Federico Peinado 
   Contacto: email@federicopeinado.com
*/
using BehaviorDesigner.Runtime.Tasks;
using System;

namespace es.ucm.fdi.iav.rts
{
    /* Determina si la unidad de extracción transporta o no recursos ahora mismo. */
    [TaskCategory("RTS")]
    [TaskDescription("Determina si la unidad de extracción transporta o no recursos ahora mismo.")]
    public class HasResources : Conditional
    {
        // La propia unidad de extracción donde se ejecuta el comportamiento
        private ExtractionUnit ExtractionUnit { get; set; }

        // Despierta la tarea cogiendo una referencia a la propia unidad de extracción donde se está ejecutando
        public override void OnAwake()
        {
            ExtractionUnit = GetComponent<ExtractionUnit>();
            if (ExtractionUnit == null)
                throw new Exception("El comportamiento no se ejecuta sobre una unidad de extracción.");
        }

        // Devuelve inmediatamente éxito si la unidad transporta recursos, y fracaso en caso contrario. 
        public override TaskStatus OnUpdate()
        {
            if (ExtractionUnit == null)
                throw new Exception("El comportamiento no se ejecuta sobre una unidad de extracción.");

            // El objetivo de movimiento se considera 'estable' si ha sido el mismo en el OnUpdate anterior,
            // en caso contrario hay cambio de objetivo y eso implica que se interrumpirá la ejecución normal del árbol de comportamiento
            if (ExtractionUnit.Resources > 0)
            {
                return TaskStatus.Success;
            }
            return TaskStatus.Failure;
        }
    }
}