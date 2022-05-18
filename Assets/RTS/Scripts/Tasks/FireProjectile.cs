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
    /* Proyectiles que son disparados por las unidades de combate y las torretas.
     * Posibles mejoras:
     * - No hacer la rotación tan brusca, sino que al buscar un TargetOffset, no sólo se incluya desplazamiento de posición sino también desplazamiento de rotación, y que el Navigate, si debe rotar, aplique dicho desplazamiento.
     * - Que no vayan identificados por controlador y hagan daño a cualquier cosa con la que choquen (siempre que esto no suponga un caos de fuego amigo!), tal vez conservando la información de que unidad o torreta envió este proyectil.
     * - Podría haber cierto desplazamiento pero para disparar el proyectil un poco por delante del enemigo, usando predicción o similar.
     * - Ahora mismo el tiempo de ataque no se tiene en cuenta porque el Wait se realiza antes de disparar... pero podría gestionarse desde aquí.
     * - Podríamos alterar desde aquí el rango (alcance) que tiene el proyectil.
     */
    [TaskCategory("RTS")]
    [TaskDescription("Dispara un proyectil al objetivo")]
    public class FireProjectile : Action
    {
        // El controlador de la unidad que ejecuta este comportamiento.
        // Ojo, si lo ha disparado una torreta, estará sin asignar.
        [Tooltip("El controlador de la unidad que ejecuta este comportamiento.")]
        [SerializeField] private SharedRTSController _rtsController;
        public SharedRTSController RTSController { get { return _rtsController; } private set { _rtsController = value; } }

        // La transformada del objetivo de ataque.
        [Tooltip("La transformada del objetivo de ataque.")]
        [SerializeField] private SharedTransform _attackTarget;
        public SharedTransform AttackTarget { get { return _attackTarget; } private set { _attackTarget = value; } }

        // El prefab del proyectil a ser disparado.
        [Tooltip("El prefab del proyectil a ser disparado.")]
        public GameObject projectilePrefab;

        // Altura que se añade al proyectil y su objetivo para poder disparar el proyectil sin tocar el suelo.
        [Tooltip("Altura que se añade al proyectil y su objetivo para poder disparar el proyectil sin tocar el suelo.")]
        [SerializeField] private float _fireHeight = 1f;
        public float FireHeight { get { return _fireHeight; } }

        /**************************************************************/

        // La propia unidad donde se ejecuta el comportamiento, si se trata de una unidad.
        // Posibles mejoras: Que sea una CombatUnit, una clase común a la unidad de destrucción y a la de exploración
        private Unit Unit { get; set; }
        private DestructionUnit DestructionUnit { get; set; }
        private ExplorationUnit ExplorationUnit { get; set; }

        // La propia torreta donde se ejecuta el comportamiento, si se trata de una torreta.
        // Posibles mejoras: Que sea un Attacker, una clase común a la CombatUnit o una torreta.
        private Tower Tower { get; set; }

        // El daño que puede causar el ataque. 
        private int AttackDamage { get; set; }

        // El propio collider de la unidad o torreta donde se ejecuta el comportamiento.
        // La vamos a usar para evitar que el proyectil choque con nosotros mismos al lanzarlo.
        private Collider Collider { get; set; }

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
                AttackDamage = Tower.AttackDamage; 
            }
            if (ExplorationUnit != null)
            {
                AttackDamage = ExplorationUnit.AttackDamage; 
            }
            if (DestructionUnit != null)
            {
                AttackDamage = DestructionUnit.AttackDamage; 
            }

            Collider = GetComponent<Collider>();
            if (Collider == null)
                throw new System.Exception("El comportamiento no se ejecuta sobre una unidad o torreta con colisión.");
        }

        // Devuelve éxito sólo un fotograma después de que se haya creado el proyectil, poniéndolo en marcha.
        public override TaskStatus OnUpdate()
        {
            if (Unit == null && Tower == null)
                throw new System.Exception("El comportamiento no se ejecuta sobre un atacante (unidad o torreta).");
            if (Tower == null && DestructionUnit == null && ExplorationUnit == null)
                throw new System.Exception("El comportamiento no se ejecuta sobre una unidad de combate (destructora o exploradora).");

            // Fracasamos, porque ha desaparecido el objetivo al que disparar.
            if (AttackTarget.Value == null)
                return TaskStatus.Failure;

            // Si somos una unidad, rotamos bruscamente hacia donde vamos a disparar (esto sería mejor hacerlo con el Navigate, con un RotationOffset)
            if (Unit != null)
                Unit.transform.LookAt(AttackTarget.Value.position);

            // Crea un nuevo proyectil cuyo padre es el objeto actual  
            var spawnedProjectile = GameObject.Instantiate(projectilePrefab) as GameObject;
            var elevatedPosition = transform.position;
            elevatedPosition.y += FireHeight;
            var elevatedTargetPosition = AttackTarget.Value.position;
            elevatedTargetPosition.y += FireHeight;
            spawnedProjectile.transform.position = elevatedPosition;
            spawnedProjectile.transform.LookAt(elevatedTargetPosition);

            // Con esto se puede mostrar el rayo de trayectoria del proyectil como Gizmo
            //Debug.DrawLine(elevatedPosition, elevatedTargetPosition, Color.yellow, 5.0f);

            // Podría ubicar los proyectiles debajo del controlador correspondiente, por agruparlo mejor... ¡pero nunca bajo la unidad que los dispara, porque entonces también se moverían con ella!

            // Debe haber controlador asociado, salvo que se trate de una torreta
            if (Tower == null && RTSController.Value == null)
                throw new System.Exception("No hay controlador asociado.");

            //Debug.Log("He disparado un proyectil!");

            var projectile = spawnedProjectile.GetComponent<Projectile>();
            // OJO: Si es una torreta, estaría bien tener un método que no requiere controlador (o pasar sólo el índice)
            projectile.Enable(RTSController.Value, AttackTarget.Value, AttackDamage); 

                // Añade un evento de manera que el proyectil es destruido cuando el objetivo es destruido
                // Esto no lo hago, porque es más realista controlarlo en función de la colisión o del tiempo, exclusivamente
                //AttackTarget.Value.gameObject.GetComponent<Health>().OnDeath += projectile.DestroySelf;

            // Ignora las colisiones entre el proyectil y el objeto que dispara el proyectil para evitar que el proyectil cause daño al propio atacante. 
            Physics.IgnoreCollision(Collider, projectile.GetComponent<Collider>());
            return TaskStatus.Success;
        }
    }
}