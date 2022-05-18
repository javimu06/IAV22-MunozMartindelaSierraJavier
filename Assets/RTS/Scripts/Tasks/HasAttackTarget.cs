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
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using Tooltip = BehaviorDesigner.Runtime.Tasks.TooltipAttribute;

namespace es.ucm.fdi.iav.rts
{
    /* 
     * Averigua si hay objetivo de ataque para la unidad o torreta, ya sea est�tico (instalaci�n, torreta o poblado) o din�mico (unidad), estableci�ndolo en una variable compartida. 
     * El objetivo tiene que ser estar dentro de la distancia de ataque y, si se marca LineOfSight, la l�nea de visi�n tambi�n debe estar despejada.
     * 
     * Posibles mejoras: 
     * - Evitar que las zonas de recursos extra�bles oculten la l�nea de visi�n para atacar otros objetivos, cosa que ahora mismo ocurre.
     * - Poder establecer tambi�n un objetivo por un array de posibles objetivos, por tags o por GameObject.Find(targetName), siendo targetName el nombre del objeto en la escena de Unity. 
     * - Poder trabajar por tanto con una cola de objetivos, para ir pasando de uno a otro si son destruidos.
     * - Si trabajamos con lista de objetivos, hay que mirar la distancia y conviene tenerla al cuadrado, que es una optimizaci�n para no tener que hacer ra�ces cuadradas, que son computacionalmente costosas.
     */
    [TaskCategory("RTS")]
    [TaskDescription("Averigua si hay objetivo de ataque para la unidad o torreta, ya sea est�tico (instalaci�n, torreta o poblado) o din�mico (unidad), estableci�ndolo en una variable compartida.")]
    public class HasAttackTarget : Conditional
    {
        // El controlador de la unidad que ejecuta este comportamiento, si no lo ejecuta una torreta.
        [Tooltip("El controlador de la unidad que ejecuta este comportamiento, si no lo ejecuta una torreta.")]
        [SerializeField] private SharedRTSController _rtsController;
        public SharedRTSController RTSController { get { return _rtsController; } private set { _rtsController = value; } }

        // La transformada del objetivo de ataque que establecemos.
        [Tooltip("La transformada del objetivo de ataque.")]
        [SerializeField] private SharedTransform _attackTarget;
        public SharedTransform AttackTarget { get { return _attackTarget; } private set { _attackTarget = value; } }

        // El tiempo que se tarda en realizar cada ataque (depende del atacante, unidad de combate o torreta).
        [Tooltip("El tiempo que se tarda en realizar cada ataque.")]
        [SerializeField] private SharedFloat _attackTime;
        public SharedFloat AttackTime { get { return _attackTime; } private set { _attackTime = value; } }

        // Indica si debe haber una l�nea de visi�n despejada hasta el objetivo.
        [Tooltip("Indica si debe haber una l�nea de visi�n despejada hasta el objetivo.")]
        [SerializeField] private bool _lineOfSight = false;
        private bool LineOfSight { get { return _lineOfSight; } }

        // Distancia m�nima a partir de la cual ya tiene sentido comprobar la l�nea de visi�n y no asumir que vemos por proximidad.
        [Tooltip("Distancia m�nima a partir de la cual ya tiene sentido comprobar la l�nea de visi�n y no asumir que vemos por proximidad.")]
        [SerializeField] private float _minSightDistance = 5.0f;
        public float MinSightDistance { get { return _minSightDistance; } }

        // Altura que se a�ade a la unidad extractora y su objetivo para poder comprobar la l�nea de visi�n sin chocar con el suelo.
        [Tooltip("Altura que se a�ade a la unidad extractora y su objetivo para poder comprobar la l�nea de visi�n sin chocar con el suelo.")]
        [SerializeField] private float _sightHeight = 1.0f;
        public float SightHeight { get { return _sightHeight; } }

        // Distancia m�nima a partir de la cual ya tiene sentido cambiar de objetivo porque est� suficientemente diferenciado.
        [Tooltip("Distancia m�nima a partir de la cual ya tiene sentido cambiar de objetivo porque est� suficientemente diferenciado.")]
        [SerializeField] private float _minRetargetDistance = 0.5f;
        public float MinRetargetDistance { get { return _minRetargetDistance; } }

        /* Este comportamiento ahora se hace en el �rbol, cuando una unidad se siente amenazada
        // Indica si debemos perseguir al mismo objetivo, sin cambiarlo por otro (s�lo funciona si somos una unidad m�vil).
        [Tooltip("Indica si debemos perseguir al mismo objetivo, sin cambiarlo por otro (s�lo funciona si somos una unidad m�vil).")]
        [SerializeField] private bool _follow = false;
        private bool Follow { get { return _follow; } }
        */

        /********************************************************************/

        // La propia unidad donde se ejecuta el comportamiento, si se trata de una unidad.
        // Posibles mejoras: Que sea una CombatUnit, una clase com�n a la unidad de destrucci�n y a la de exploraci�n
        private Unit Unit { get; set; }
        private DestructionUnit DestructionUnit { get; set; }
        private ExplorationUnit ExplorationUnit { get; set; }

        // La propia torreta donde se ejecuta el comportamiento, si se trata de una torreta.
        // Posibles mejoras: Que sea un Attacker, una clase com�n a la CombatUnit o una torreta.
        private Tower Tower { get; set; }

        // Distancia a la que puede comenzar a atacar.
        private float AttackDistance { get; set; }

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
                AttackTime.Value = Tower.AttackTime;
                AttackDistance = Tower.AttackDistance;
            }
            if (ExplorationUnit != null)
            {
                AttackTime.Value = ExplorationUnit.AttackTime;
                AttackDistance = ExplorationUnit.AttackDistance;
            }
            if (DestructionUnit != null)
            {
                AttackTime.Value = DestructionUnit.AttackTime;
                AttackDistance = DestructionUnit.AttackDistance;
            }
        }

        // C�digo que podr�a ser est�tico para comprobar si se tiene l�nea de visi�n con el objetivo de ataque.
        // Se lanza un rayo y si colisiona con la transformada del objetivo es que no hay otros objetos obstruyendo la l�nea de visi�n.
        // Si hay mucha cercan�a, tambi�n asumimos que hay visi�n. El problema es que cuando el atacante est� justo encima de un objetivo y tiene activa LineOfSight, no podr�a verlo (porque los rayos no colisionan con colliders que YA EST�N AH� en el inicio).
        private bool HasLineOfSight(Transform targetTransform)
        {
            Vector3 direction = targetTransform.position - transform.position;

            if (direction.magnitude < MinSightDistance)
                return true;

            RaycastHit hit;
            var elevatedPosition = transform.position;
            elevatedPosition.y += SightHeight;
            var elevatedTargetPosition = targetTransform.position;
            elevatedTargetPosition.y += SightHeight;

            // Con esto se puede mostrar el rayo de ataque como Gizmo
            Debug.DrawLine(elevatedPosition, elevatedTargetPosition, Color.red, 5.0f);

            // Entiendo que la LayerMask est� a la negaci�n de 0 (~0) y eso permite capturar colisiones en todas las capas (podr�a restringirse)
            // La direcci�n del rayo se podr�a normalizar, porque da igual, luego la distancia es infinita
            if (Physics.Raycast(elevatedPosition, direction, out hit, Mathf.Infinity, ~0, QueryTriggerInteraction.Collide))
            {
                if (hit.transform.Equals(targetTransform))
                {
                    return true;
                }
            }

            return false;
        }

        // Devuelve �xito si se puede establecer el objetivo de ataque en una variable compartida, y fracaso en caso contrario.
        // Se recoge todo lo del escenario que est� a la distancia de ataque, se filtra por tipos posibles de objetivos (enemigos en caso de ser una unidad) y l�nea de visi�n, y nos quedamos con el m�s cercano (sin importar su tipo).
        public override TaskStatus OnUpdate()
        {
            if (Unit == null && Tower == null)
                throw new System.Exception("El comportamiento no se ejecuta sobre un atacante (unidad o torreta).");
            if (Tower == null && DestructionUnit == null && ExplorationUnit == null)
                throw new System.Exception("El comportamiento no se ejecuta sobre una unidad de combate (destructora o exploradora).");

            // Establezco un nuevo objetivo de ataque, si no estoy siguiendo objetivo, o ha desaparecido el objetivo anterior, o se ha marchado demasiado lejos
            // Antes ten�a esta condici�n... !Follow || .. cuidado con no cambiarte de objetivo con facilidad, con el primero que encuentres
            if (AttackTarget.Value == null || (Vector3.Distance(transform.position, AttackTarget.Value.position) > AttackDistance))
            {
                // C�digo que pod�a ser est�tico para comprobar si hay objetivos de combate cercanos.
                // Si no hay colliders, no se puede establecer nada
                var hitColliders = Physics.OverlapSphere(transform.position, AttackDistance);
                if (hitColliders.Length == 0)
                    return TaskStatus.Failure;

                int MyIndex;
                if (Tower == null)
                {
                    // Debe haber controlador asociado, salvo que se trate de una torreta
                    if (RTSController.Value == null) // A VECES FALLA ESTO, Y SI NO ES TORRETA... DEBER�A TENER RTSController
                        throw new System.Exception("No hay controlador asociado.");
                    MyIndex = RTSGameManager.Instance.GetIndex(RTSController.Value);
                }
                else
                {
                    MyIndex = -1; // Asumo que este valor no se usa, para d�rselo a la torre y que todo lo vea como enemigos
                }

                Transform bestEnemy = null;
                float bestDistance = float.MaxValue; // Empezamos con el valor peor posible 
                // Me quedo con el objetivo m�s cercano, si lo hay
                foreach (Collider hitCollider in hitColliders)
                {
                    if (hitCollider.gameObject != null) // Podr�amos asumir que esto ocurre siempre, supongo...
                    {
                        Unit unit = hitCollider.gameObject.GetComponent<Unit>(); 
                        Facility facility = hitCollider.gameObject.GetComponent<Facility>();
                        Tower tower = hitCollider.gameObject.GetComponent<Tower>();
                        Village village = hitCollider.gameObject.GetComponent<Village>();
                        // Si soy una torre, s�lo ataco unidades, del ej�rcito que sea
                        // Si soy una unidad, ataco cualquier cosa que no tenga mi mismo controlador
                        if ((Tower != null && unit != null) ||
                            (Tower == null && ((unit != null && unit.GetControllerIndex() != MyIndex) ||
                                               (facility != null && facility.GetControllerIndex() != MyIndex) ||
                                               (tower != null) ||
                                               (village != null)))) 
                        {
                            // Si no hace falta comprobar la l�nea de visi�n, o si hace falta y la hay, consideramos el objetivo
                            if (!LineOfSight || HasLineOfSight(hitCollider.transform))
                            {
                                // No se elige por tipo de enemigo sino por el m�s cercano, asumiendo que ser� el m�s peligroso (y posiblemente el que nos ataca fijo)
                                // Podr�a comprobar si es hijo, nieto, bisnieto, etc. del controlador enemigo... o del escenario, en el caso de poblados y torretas; para priorizar por ello.
                                Transform nearbyEnemy = hitCollider.gameObject.transform;
                                float nearbyDistance = Vector3.Distance(transform.position, nearbyEnemy.position);
                                if (nearbyDistance < bestDistance)
                                {
                                    bestEnemy = nearbyEnemy;
                                    bestDistance = nearbyDistance;
                                }
                            }
                        }
                    }
                }

                // Si no encuentro candidatos que me valgan, no se puede establecer nada como objetivo de ataque
                if (bestEnemy == null)
                    return TaskStatus.Failure;

                // Si ya ten�a un objetivo de ataque, y el nuevo est� muy cerca del otro... me mantengo como estaba, para no liarme. No hago retargeting.
                //if (AttackTarget.Value != null && Vector3.Distance(AttackTarget.Value.position, bestEnemy.position) < MinRetargetDistance)
                //    return TaskStatus.Success;

                // Establecemos el mejor candidato como objetivo de ataque
                AttackTarget.Value = bestEnemy;
            }


            // Si ten�amos el objetivo de ataque seleccionado y tenemos Follow, seguimos detr�s de �l 
            return TaskStatus.Success;
        }

        /*
        // Dibuja la distancia para que podamos verla en el editor
        public override void OnDrawGizmos()
        {
        #if UNITY_EDITOR
            var oldColor = UnityEditor.Handles.color;
            UnityEditor.Handles.color = Color.yellow;
            UnityEditor.Handles.DrawWireDisc(Owner.transform.position, Owner.transform.up, distance);
            UnityEditor.Handles.color = oldColor;
        #endif
        }
        */
    }
}