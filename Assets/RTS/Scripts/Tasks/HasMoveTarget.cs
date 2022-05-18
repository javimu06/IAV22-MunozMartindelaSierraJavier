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
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Tooltip = BehaviorDesigner.Runtime.Tasks.TooltipAttribute; 

namespace es.ucm.fdi.iav.rts
{
    /* 
     * Averigua si hay objetivo de movimiento para la unidad, ya sea estático (ej. posición fija sobre el terreno) o dinámico (ej. unidad enemiga), estableciéndolo en una variable compartida. 
     * Si el objetivo de movimiento CAMBIA, se reinicia el árbol (durante un fotograma este condicional fracasa, o explícitamente lo reiniciamos)
     * 
     * Posibles mejoras:
     * - Podría consultar si me han enviado directamente a una zona de recursos extraíbles, y FIJAR YA el extraction target desde aquí... aunque lo mismo es enrevesado.
     * - Podría consultar si me han enviado directamente a una instalación de procesamiento, y FIJAR YA el processing target desde aquí... aunque lo mismo es enrevesado.
     * - Podría consultar si me han enviado directamente a un enemigo u objeto al que atacar, y FIJAR YA el attack target desde aquí... aunque lo mismo es enrevesado.
     * - Hacer algún tipo de redondeo por si nos dan una posición que es muy próxima a algún objeto relevante (seleccionar el objeto), o de chequeo para asegurarnos de que es posible llegar hasta allí.
     * - Poder establecer también un objetivo por tags o por GameObject.Find(targetName), siendo targetName el nombre del objeto en la escena de Unity.
     * - Hacer reinicio del árbol entero, o sólo de esta tarea, si se produce un cambio de objetivo de movimiento por parte del controlador.
     */
    [TaskCategory("RTS")]
    [TaskDescription("Averigua si hay objetivo de movimiento para la unidad y lo establece en una variable compartida, ya sea dinámico (ej. unidad enemiga) o estático (ej. posición fija sobre el terreno).")]
    public class HasMoveTarget : Conditional
    {
        // La transformada del objetivo de movimiento.
        [Tooltip("La transformada del objetivo de movimiento.")]
        [SerializeField] private SharedTransform _moveTarget;
        public SharedTransform MoveTarget { get { return _moveTarget; } private set { _moveTarget = value; } }

        /********************************************************************/

        // La propia unidad donde se ejecuta el comportamiento
        private Unit Unit { get; set; }

        // El último objetivo de movimiento que tuvo la unidad
        private Transform LastMoveTarget { get; set; }

        // Despierta la tarea cogiendo una referencia a la propia unidad donde se está ejecutando 
        public override void OnAwake()
        {
            Unit = GetComponent<Unit>();
            if (Unit == null)
                throw new System.Exception("El comportamiento no se ejecuta sobre una unidad.");
        }

        // Devuelve éxito si puede establecer el objetivo de movimiento de la unidad en una variable compartida, y fracaso en caso contrario.
        // Si el objetivo de movimiento se cambia por otro distinto, justo en el momento del cambio en que el objetivo no se considera 'estable', se devuelve fracaso también.
        public override TaskStatus OnUpdate()
        {
            if (Unit == null)
                throw new System.Exception("El comportamiento no se ejecuta sobre una unidad.");

            // El objetivo de movimiento se considera 'estable' si ha sido el mismo en el OnUpdate anterior,
            // en caso contrario hay cambio de objetivo y eso implica que se interrumpirá la ejecución normal del árbol de comportamiento
            if (Unit.MoveTarget != null && Unit.MoveTarget == LastMoveTarget)
            { 
                MoveTarget.Value = Unit.MoveTarget;
                return TaskStatus.Success;
            }

            LastMoveTarget = Unit.MoveTarget;
            return TaskStatus.Failure;             
        }
    }
}