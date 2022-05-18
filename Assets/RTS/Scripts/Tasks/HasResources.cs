/*    
   Copyright (C) 2020 Federico Peinado
   http://www.federicopeinado.com

   Este fichero forma parte del material de la asignatura Inteligencia Artificial para Videojuegos.
   Esta asignatura se imparte en la Facultad de Inform�tica de la Universidad Complutense de Madrid (Espa�a).

   Autores originales: Opsive (Behavior Designer Samples)
   Revisi�n: Federico Peinado 
   Contacto: email@federicopeinado.com
*/
using BehaviorDesigner.Runtime.Tasks;
using System;

namespace es.ucm.fdi.iav.rts
{
    /* Determina si la unidad de extracci�n transporta o no recursos ahora mismo. */
    [TaskCategory("RTS")]
    [TaskDescription("Determina si la unidad de extracci�n transporta o no recursos ahora mismo.")]
    public class HasResources : Conditional
    {
        // La propia unidad de extracci�n donde se ejecuta el comportamiento
        private ExtractionUnit ExtractionUnit { get; set; }

        // Despierta la tarea cogiendo una referencia a la propia unidad de extracci�n donde se est� ejecutando
        public override void OnAwake()
        {
            ExtractionUnit = GetComponent<ExtractionUnit>();
            if (ExtractionUnit == null)
                throw new Exception("El comportamiento no se ejecuta sobre una unidad de extracci�n.");
        }

        // Devuelve inmediatamente �xito si la unidad transporta recursos, y fracaso en caso contrario. 
        public override TaskStatus OnUpdate()
        {
            if (ExtractionUnit == null)
                throw new Exception("El comportamiento no se ejecuta sobre una unidad de extracci�n.");

            // El objetivo de movimiento se considera 'estable' si ha sido el mismo en el OnUpdate anterior,
            // en caso contrario hay cambio de objetivo y eso implica que se interrumpir� la ejecuci�n normal del �rbol de comportamiento
            if (ExtractionUnit.Resources > 0)
            {
                return TaskStatus.Success;
            }
            return TaskStatus.Failure;
        }
    }
}