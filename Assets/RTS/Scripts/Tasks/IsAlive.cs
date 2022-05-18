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
    /* Averigua si un cierto objetivo existe, sigue vivo (bien de salud) y está a tiro. */
    [TaskCategory("RTS")]
    [TaskDescription("Averigua si un cierto objetivo existe y sigue vivo (bien de salud). ")]
    public class IsAlive : Conditional
    {
        // La transformada del objetivo de ataque que establecemos.
        [Tooltip("La transformada del objetivo de ataque.")]
        [SerializeField] private SharedTransform _attackTarget;
        public SharedTransform AttackTarget { get { return _attackTarget; } private set { _attackTarget = value; } }

        /************************************************************/

        // La propia unidad donde se ejecuta el comportamiento, si se trata de una unidad.
        // Posibles mejoras: Que sea una CombatUnit, una clase común a la unidad de destrucción y a la de exploración
        private Unit Unit { get; set; }
        private DestructionUnit DestructionUnit { get; set; }
        private ExplorationUnit ExplorationUnit { get; set; }

        // La propia torreta donde se ejecuta el comportamiento, si se trata de una torreta.
        // Posibles mejoras: Que sea un Attacker, una clase común a la CombatUnit o una torreta.
        private Tower Tower { get; set; }

        // Distancia a la que puede comenzar a atacar.
        private float AttackDistance { get; set; }

        // Salud del objetivo de ataque
        private Health Health { get; set; }

        // Despierta la tarea cogiendo una referencia a la propia unidad o torreta.
        public override void OnAwake()
        {
            Unit = GetComponent<Unit>();
            Tower = GetComponent<Tower>();
            if (Unit == null && Tower == null)
                throw new System.Exception("El comportamiento no se ejecuta sobre un atacante (unidad o torreta).");

            DestructionUnit = GetComponent<DestructionUnit>();
            ExplorationUnit = GetComponent<ExplorationUnit>();
            if (Tower == null && DestructionUnit == null && ExplorationUnit == null)
                throw new System.Exception("El comportamiento no se ejecuta sobre una unidad de combate (destructora o exploradora).");

            // Inicializo variables que voy a necesitar (asumo que no van a darse varios componentes a la vez)
            if (Tower != null)
            { 
                AttackDistance = Tower.AttackDistance;
            }
            if (ExplorationUnit != null)
            { 
                AttackDistance = ExplorationUnit.AttackDistance;
            }
            if (DestructionUnit != null)
            { 
                AttackDistance = DestructionUnit.AttackDistance;
            }
        }

        // Devuelve inmediatamente éxito si el objeto existe y sigue con vida, y fracaso si no
        public override TaskStatus OnUpdate()
        {
            if (Unit == null && Tower == null)
                throw new System.Exception("El comportamiento no se ejecuta sobre un atacante (unidad o torreta).");
            if (Tower == null && DestructionUnit == null && ExplorationUnit == null)
                throw new System.Exception("El comportamiento no se ejecuta sobre una unidad de combate (destructora o exploradora).");

            if (AttackTarget.Value != null)
            { 
                Health = AttackTarget.Value.GetComponent<Health>();            
                if (Health != null && Health.Amount > 0 && Vector3.Distance(transform.position, AttackTarget.Value.position) <= AttackDistance) 
                    return TaskStatus.Success;
            }
            return TaskStatus.Failure;
        }
    }
}