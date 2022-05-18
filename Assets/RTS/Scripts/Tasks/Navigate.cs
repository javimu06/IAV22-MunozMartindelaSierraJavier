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
using UnityEngine.AI;

namespace es.ucm.fdi.iav.rts
{
    /* 
     * Ordena al agente de malla de navegación que vaya hacia el objetivo de navegación con los comportamientos de dirección establecidos.
     * 
     * Posibles mejoras:
     * - Como sigue habiendo bloqueos de vez en cuando, para intentar mejorar la situación, podría mover y rotar un poco la unidad, para introducir aleatoriedad... o introducir algún impulso físico en la colisión.
     * - Considerar si incluir una variable en Unit que permita tener en cuenta cierta Unit.DetectionDistance para conformarnos con quedarnos cerca de ciertos objetivos (unidades enemigas, etc.)
     * - Confirmar que DetectionDistance es suficiente para que la unidad se "conforme" con parar cerca de su objetivo, si es una instalación o algo grande y no caminable.
     * - Mejorar la evitación de obstáculos para tener en cuenta el movimiento de las unidades, especialmente las de mi mismo bando.
     * - Mejorar la resolución de bloqueos, con temporizadores durante la navegación, reintentos por otras vías o cada cierto tiempo... u otras técnicas. 
     */
    [TaskCategory("Common")]
    [TaskDescription("Ordena al agente de malla de navegación que vaya hacia el objetivo de navegación con los comportamientos de dirección establecidos.")]
    public class Navigate: Action
    {
        // El controlador de la unidad que ejecuta este comportamiento.
        [Tooltip("El controlador de la unidad que ejecuta este comportamiento.")]
        [SerializeField] private SharedRTSController _rtsController; 
        public SharedRTSController RTSController { get { return _rtsController; } private set { _rtsController = value; } }

        // La transformada del objetivo de navegación.
        [Tooltip("La transformada del objetivo de navegación.")]
        [SerializeField] private SharedTransform _navigationTarget;
        public SharedTransform NavigationTarget { get { return _navigationTarget; } private set { _navigationTarget = value; } }

        // El desplazamiento aplicable sobre el objetivo de navegación.
        [Tooltip("El desplazamiento aplicable sobre el objetivo de navegación.")]
        [SerializeField] private SharedVector3 _targetOffset = null;
        public SharedVector3 TargetOffset { get { return _targetOffset; } }

        // Velocidad mínima del agente que es aceptable antes de asumir que se ha bloqueado.
        [Tooltip("Velocidad mínima del agente que es aceptable antes de asumir que se ha bloqueado.")]
        [SerializeField] private float _acceptableMinVelocity = 1f;
        public float AcceptableMinVelocity { get { return _acceptableMinVelocity; } }

        // Tiempo máximo que el agente puede utilizar para arrancar la navegación y alcanzar la velocidad mínima.
        [Tooltip("Tiempo máximo que el agente puede utilizar para arrancar la navegación y alcanzar la velocidad mínima.")]
        [SerializeField] private float _acceptableMaxBootTime = 2f;
        public float AcceptableMaxBootTime { get { return _acceptableMaxBootTime; } }

        // Indica si el agente debe acabar con la misma rotación que el objetivo de navegación o no.
        [Tooltip("Indica si el agente debe acabar con la misma rotación que el objetivo de navegación o no.")]
        [SerializeField] private bool _rotateToTarget = false;
        public bool RotateToTarget { get { return _rotateToTarget; } }

        // Diferencia máxima que es aceptable como margen de error entre la rotación del agente y la del objetivo de navegación.
        [Tooltip("Diferencia máxima que es aceptable como margen de error entre la rotación del agente y la del objetivo de navegación.")]
        [SerializeField] private float _acceptableMaxRotationError = 5.0f;
        public float AcceptableMaxRotationError { get { return _acceptableMaxRotationError; } }

        /********************************************************************/

        // La propia unidad donde se ejecuta el comportamiento.
        private Unit Unit { get; set; }

        // El propio agente de malla de navegación donde se ejecuta el comportamiento.
        private NavMeshAgent NavMeshAgent { get; set; }

        // Tiempo transcurrido desde el arranque de la navegación.
        private float BootTime { get; set; }

        // Última posición del objetivo de navegación (se asume que aunque el objetivo pueda cambiar, el desplazamiento no cambia).
        private Vector3 LastNavigationTargetPosition;

        // Despierta la tarea cogiendo una referencia a la propia unidad y al propio agente de malla de navegación donde se está ejecutando.
        public override void OnAwake()
        { 
            Unit = GetComponent<Unit>();
            if (Unit == null)
                throw new System.Exception("El comportamiento no se ejecuta sobre una unidad.");

            NavMeshAgent = GetComponent<NavMeshAgent>();
            if (NavMeshAgent == null)
                throw new System.Exception("El comportamiento no se ejecuta sobre un agente de malla de navegación.");
        }

        // Inicia la navegación hacia el objetivo de navegación
        public override void OnStart()
        {
            if (RTSController.Value == null)
                throw new System.Exception("No hay controlador asociado.");
            if (Unit == null)
                throw new System.Exception("El comportamiento no se ejecuta sobre una unidad.");
            if (NavMeshAgent == null)
                throw new System.Exception("El comportamiento no se ejecuta sobre un agente de malla de navegación.");

            // Se activa el agente de malla de navegación y se pone a funcionar
            // Quizá no haga falta activar todo esto en cada Start
            NavMeshAgent.enabled = true;
            NavMeshAgent.isStopped = false;

            // Se asigna el objetivo de navegación al agente de malla de navegación como destino para que empiece la búsqueda del camino óptimo.
            // Si no hay objetivo de navegación no nos preocupa ahora... ya fracasará el OnUpdate
            if (NavigationTarget.Value != null) { 
                NavMeshAgent.destination = NavigationTarget.Value.position + TargetOffset.Value;
                // Guardamos la última posición del objetivo de navegación
                LastNavigationTargetPosition = NavigationTarget.Value.position;
            }

            // Guardamos el momento en que arranca la navegación
            BootTime = Time.time;
        }

        // Gestiona la navegación devolviendo 'Running' mientras se realiza.
        // Se devuelve éxito cuando hayamos llegado lo suficientemente cerca de nuestro destino, y fracaso si no (hay bloqueo o desaparece el objetivo de navegación).
        public override TaskStatus OnUpdate()
        {
            if (RTSController.Value == null)
                throw new System.Exception("No hay controlador asociado.");
            if (Unit == null)
                throw new System.Exception("El comportamiento no se ejecuta sobre una unidad.");
            if (NavMeshAgent == null)
                throw new System.Exception("El comportamiento no se ejecuta sobre un agente de malla de navegación.");

            // Si no hay objetivo de navegación, se para todo (se habrá dado orden de parar) y devolvemos fracaso.
            if (NavigationTarget.Value == null) {
                NavMeshAgent.isStopped = true;
                return TaskStatus.Failure;
            }

            // OJO POR SI ESTO FUERA PARALIZA AL AGENTE!
            // Si el objetivo se mantiene pero ha cambiado su posición (es dinámico), se reestablece el destino del agente de mallas de navegación
            if (NavigationTarget.Value.position != LastNavigationTargetPosition)
                NavMeshAgent.destination = NavigationTarget.Value.position + TargetOffset.Value;

            // Sólo si ya se ha calculado el camino óptimo (puede costar varios fotogramas), comprobamos situaciones especiales
            if (!NavMeshAgent.pathPending) {

                // Si consideramos que el agente de malla de navegación ha terminado su trabajo (generalmente LLEGAR), nos aseguramos de pararlo, rotamos (si hace falta) y finalmente devolvemos éxito.
                // Saber si un NavMeshAgent ha terminado es complejo, se puede tener en cuenta:
                // - Recibir eventos del componente NavMeshAgent como OnComplete()
                // - Que el pathStatus sea NavMeshPathStatus.PathComplete
                // - Que la distancia remanente (remainingDistance) sea 0 o que al menos sea inferior a cierto valor pequeño, tipo 0.1 (o menor que la stoppingDistance, por ejemplo)
                // - Desde luego que la distancia remanente no sea desconocida (que se expresa con un Mathf.Infinite)
                // - Que no tenga hasPath o que la velocidad también sea 0 o que al menos sea inferior a cierto valor pequeño, tipo 0.1
                // - Y otras condiciones como ver si la ruta calculada es inválida o no (pathInvalid), etc.

                if (NavMeshAgent.pathStatus == NavMeshPathStatus.PathComplete && NavMeshAgent.remainingDistance <= NavMeshAgent.stoppingDistance)
                { 
                    // Paramos y devolvemos éxito cuando no necesitamos rotar al objetivo... o si ya se ha rotado lo suficiente
                    if (!RotateToTarget || 1 - Mathf.Abs(Quaternion.Dot(transform.rotation, NavigationTarget.Value.rotation)) < AcceptableMaxRotationError)
                    {
                        NavMeshAgent.isStopped = true;
                        Unit.Stop(RTSController.Value);
                        return TaskStatus.Success;
                    }
                    // Rotamos para ir adoptando la rotación que hace falta para terminar                  
                    // Posible mejora: Podría reiniciar BootTime y así dar algo más de tiempo para terminar la rotación, ya que la velocidad lineal tenderá a cero y pararemos por bloqueo
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, NavigationTarget.Value.rotation, NavMeshAgent.angularSpeed * Time.deltaTime);  
                }

                // Si consideramos que se ha producido un bloqueo, devuelvo fracaso
                // Para reconocer un bloque vamos a comprobar si estoy prácticamente parado, siempre que haya pasado el tiempo de arranque de la navegación 
                if (NavMeshAgent.velocity.magnitude < AcceptableMinVelocity && Time.time > BootTime + AcceptableMaxBootTime)
                {
                    // Damos orden de parar a la unidad, eliminando el objetivo de movimiento que da el controlador
                    // Posibles mejoras: Podríamos no hacer esto, por si es posible retomar la navegación haciendo algún reintento
                    NavMeshAgent.isStopped = true;
                    //Unit.Stop(RTSController.Value);

                    // Como sigue habiendo bloqueos, para intentar mejorar la situación, podría mover y rotar un poco la unidad, para introducir aleatoriedad

                    return TaskStatus.Failure;
                }
            }

            // Guardamos la última posición del objetivo de navegación
            LastNavigationTargetPosition = NavigationTarget.Value.position;

            return TaskStatus.Running;
        }
    }
}
 