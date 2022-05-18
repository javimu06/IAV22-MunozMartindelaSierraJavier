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
using BehaviorDesigner.Runtime;

namespace es.ucm.fdi.iav.rts
{
    /* 
     * Reinicia el comportamiento de ataque, olvidando el objetivo (parando), y demás otros elementos del ataque.
     * Posibles mejoras:
     * - Considerar si sería interesante reiniciar alguna variables a nulo antes incluso de que Navigate haya concluido con éxito, sólo al recibir la orden del controlador.
     */
    [TaskCategory("RTS")]
    [TaskDescription("Reinicia el comportamiento de ataque, olvidando el objetivo (parando), y demás otros elementos del ataque.")]
    public class ResetCombat : Action
    {
        // El controlador de la unidad que ejecuta este comportamiento.
        [Tooltip("El controlador de la unidad que ejecuta este comportamiento.")]
        [SerializeField] private SharedRTSController _rtsController;
        public SharedRTSController RTSController { get { return _rtsController; } private set { _rtsController = value; } }

        // La transformada del objetivo de ataque.
        [Tooltip("La transformada del objetivo de ataque.")]
        [SerializeField] private SharedTransform _attackTarget;
        public SharedTransform AttackTarget { get { return _attackTarget; } private set { _attackTarget = value; } }

        // El desplazamiento aplicable sobre el objetivo de navegación.
        [Tooltip("El desplazamiento aplicable sobre el objetivo de navegación.")]
        [SerializeField] private SharedVector3 _targetOffset = null;
        public SharedVector3 TargetOffset { get { return _targetOffset; } }

        /**************************************************************/

        // La propia unidad donde se ejecuta el comportamiento, si se trata de una unidad.
        // Posibles mejoras: Que sea una CombatUnit, una clase común a la unidad de destrucción y a la de exploración
        private Unit Unit { get; set; }
        private DestructionUnit DestructionUnit { get; set; }
        private ExplorationUnit ExplorationUnit { get; set; }

        // La propia torreta donde se ejecuta el comportamiento, si se trata de una torreta.
        // Posibles mejoras: Que sea un Attacker, una clase común a la CombatUnit o una torreta.
        private Tower Tower { get; set; }

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
        }

        // Da orden de parar la unidad, olvida el objetivo de movimiento, y devuelve inmediatamente éxito.
        public override TaskStatus OnUpdate()
        {
            if (Unit == null && Tower == null)
                throw new System.Exception("El comportamiento no se ejecuta sobre un atacante (unidad o torreta).");
            if (Tower == null && DestructionUnit == null && ExplorationUnit == null)
                throw new System.Exception("El comportamiento no se ejecuta sobre una unidad de combate (destructora o exploradora).");

            // Debe haber controlador asociado, salvo que se trate de una torreta
            if (Tower == null && RTSController.Value == null)
                throw new System.Exception("No hay controlador asociado.");

            // A una torreta no hace falta mandarle parar
            if (Unit != null) 
                Unit.Stop(RTSController.Value);
            AttackTarget.Value = null;
            TargetOffset.Value = new Vector3(0.0f, 0.0f, 0.0f);
            // El AttackTime no se reinicia porque sólo depende de la unidad que soy, y se fija en el Awake 

            return TaskStatus.Success; 
        }
    }
}