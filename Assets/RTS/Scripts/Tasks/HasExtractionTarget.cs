/*    
   Copyright (C) 2020 Federico Peinado
   http://www.federicopeinado.com

   Este fichero forma parte del material de la asignatura Inteligencia Artificial para Videojuegos.
   Esta asignatura se imparte en la Facultad de Informática de la Universidad Complutense de Madrid (España).

   Autores originales: Opsive (Behavior Designer Samples)
   Revisión: Federico Peinado 
   Contacto: email@federicopeinado.com
*/
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using Tooltip = BehaviorDesigner.Runtime.Tasks.TooltipAttribute;

namespace es.ucm.fdi.iav.rts
{
    /* 
     * Averigua si hay objetivo de extracción para la unidad y lo establece en una variable compartida, en principio estático (zona de recursos extraíbles).
     * Si no hay una zona de recursos extraíbles ya seleccionada, se escogerá la más cercana pero SIEMPRE dentro de una distancia máxima de detección.
     * Además, si se marca LineOfSight, la línea de visión también debe estar despejada.
     * La ubicación concreta estará situada en el centro de la zona de recursos extraibles para que las unidades de extracción trabajen allí.
     * 
     * Posibles mejoras:
     * - Hacer algún tipo de chequeo para asegurarnos de que es posible llegar hasta allí, como línea de visión o preguntando a la malla de navegación.
     * - Leer el tiempo de extracción de la unidad (dependiendo de su carga previa) y/o de la dificultad de los recursos extraíbles, y pasárselo por variable compartida al Wait.
     * - Poder establecer también un objetivo por tags o por GameObject.Find(targetName), siendo targetName el nombre del objeto en la escena de Unity.
     * - Generalizar SetProcessingTarget y SetExtractionTarget para que encuentren objetos de un cierto tipo a una cierta distancia de la unidad.
     */
    [TaskCategory("RTS")]
    [TaskDescription("Averigua si hay objetivo de extracción para la unidad y lo establece en una variable compartida, en principio estático (recursos extraíbles).")]
    public class HasExtractionTarget : Conditional
    {
        // Tanto la transformada, como el acceso limitado, deben corresponder al mismo objeto.

        // La transformada del objetivo de extracción.
        [Tooltip("La transformada del objetivo de extracción.")]
        [SerializeField] private SharedTransform _extractionTarget;
        public SharedTransform ExtractionTarget { get { return _extractionTarget; } private set { _extractionTarget = value; } }

        // La zona de recursos extraíbles con la que vamos a trabajar.
        // Posibles mejoras: Tener un tipo específico para esto que sea ExtractableResources
        [Tooltip("La zona de recursos extraíbles con la que vamos a trabajar.")]
        [SerializeField] private SharedLimitedAccess _extractableResources;
        public SharedLimitedAccess ExtractableResources { get { return _extractableResources; } private set { _extractableResources = value; } }

        // Distancia máxima a la que la unidad es capaz de detectar zonas de recursos extraíbles cercanas.
        [Tooltip("Distancia máxima a la que la unidad es capaz de detectar zonas de recursos extraíbles cercanas.")]
        [SerializeField] private float _maxExtractionDistance = 35.0f;
        public float MaxExtractionDistance { get { return _maxExtractionDistance; } }

        // Indica si debe haber una línea de visión despejada hasta el objetivo.
        [Tooltip("Indica si debe haber una línea de visión despejada hasta el objetivo.")]
        [SerializeField] private bool _lineOfSight = false;
        private bool LineOfSight { get { return _lineOfSight; } }

        // Distancia mínima a partir de la cual ya tiene sentido comprobar la línea de visión y no asumir que vemos por proximidad.
        [Tooltip("Distancia mínima a partir de la cual ya tiene sentido comprobar la línea de visión y no asumir que vemos por proximidad.")]
        [SerializeField] private float _minSightDistance = 5.0f;
        public float MinSightDistance { get { return _minSightDistance; } }

        // Altura que se añade a la unidad extractora y su objetivo para poder comprobar la línea de visión sin chocar con el suelo.
        [Tooltip("Altura que se añade a la unidad extractora y su objetivo para poder comprobar la línea de visión sin chocar con el suelo.")]
        [SerializeField] private float _sightHeight = 0.2f;
        public float SightHeight { get { return _sightHeight; } }

        /********************************************************************/

        // La propia unidad donde se ejecuta el comportamiento.
        private ExtractionUnit ExtractionUnit { get; set; }

        // Despierta la tarea cogiendo una referencia a la propia unidad, a su controlador y al propio agente de malla de navegación donde se está ejecutando.
        public override void OnAwake()
        {
            ExtractionUnit = GetComponent<ExtractionUnit>();
            if (ExtractionUnit == null)
                throw new System.Exception("El comportamiento no se ejecuta sobre una unidad de extracción.");
        }

        // Código que podría ser estático para comprobar si se tiene línea de visión con el objetivo de extracción.
        // Se lanza un rayo y si colisiona con la transformada del objetivo es que no hay otros objetos obstruyendo la línea de visión.
        // Si hay mucha cercanía, también asumimos que hay visión. El problema es que cuando la unidad de extracción está justo encima de una zona de recursos extraíbles y tiene activa LineOfSight, no podría verla (porque los rayos no colisionan con colliders que YA ESTÁN AHÍ en el inicio).
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

            //Con esto se puede mostrar el rayo como Gizmo
            Debug.DrawLine(elevatedPosition, elevatedTargetPosition, Color.green, 5.0f);

            // Entiendo que la LayerMask está a la negación de 0 (~0) y eso permite capturar colisiones en todas las capas (podría restringirse)
            // La dirección del rayo se podría normalizar, porque da igual, luego la distancia es infinita
            if (Physics.Raycast(elevatedPosition, direction, out hit, Mathf.Infinity, ~0, QueryTriggerInteraction.Collide))
            {
                if (hit.transform.Equals(targetTransform))
                {
                    return true;
                }
            }

            return false;
        }

        // Devuelve éxito si puede establecer el objetivo de extracción de la unidad en una variable compartida, y fracaso en caso contrario.
        // Se recoge, del escenario, todo lo que esté a CIERTA DISTANCIA, se filtra por accesos limitados (que esté en el escenario, es decir, que no sean instalaciones) y nos quedamos con la más cercana (sin importar su ocupación).
        public override TaskStatus OnUpdate()
        {
            if (ExtractionUnit == null)
                throw new System.Exception("El comportamiento no se ejecuta sobre una unidad de extracción.");

            // Sólo trato de establecer una nueva zona de extracción de recursos si no tenía escogida otra antes
            // Antes tenía: (ExtractionTarget.Value == null || ExtractableResources.Value == null)
            if (ExtractableResources.Value == null)
            {
                // Código que podía ser estático para comprobar si hay zonas de recursos extraíbles cercanas.
                // Si no hay colliders, no se puede establecer nada
                var hitColliders = Physics.OverlapSphere(ExtractionUnit.transform.position, MaxExtractionDistance);
                if (hitColliders.Length == 0)  
                    return TaskStatus.Failure;

                LimitedAccess bestResources = null;
                float bestDistance = float.MaxValue; // Empezamos con el valor peor posible
                // Me quedo con la zona de recursos extraíbles más cercana, si la hay
                foreach (Collider hitCollider in hitColliders)
                {
                    if (hitCollider.gameObject != null) // Podríamos asumir que esto ocurre siempre, supongo...
                    {
                        LimitedAccess nearbyResources = hitCollider.gameObject.GetComponent<LimitedAccess>();
                        // Se comprueba si el acceso limitado es hijo, nieto, bisnieto, etc. del escenario (para que no sea una instalación)
                        if (nearbyResources != null && nearbyResources.transform.IsChildOf(RTSScenarioManager.Instance.Scenario.transform))
                        {
                            // Si no hace falta comprobar la línea de visión, o si hace falta y la hay, consideramos el objetivo
                            if (!LineOfSight || HasLineOfSight(hitCollider.transform))
                            {
                                float nearbyDistance = Vector3.Distance(ExtractionUnit.transform.position, nearbyResources.transform.position);
                                if (nearbyDistance < bestDistance)
                                {
                                    bestResources = nearbyResources;
                                    bestDistance = nearbyDistance;
                                }
                            }
                        }
                    }
                }

                // Si no encuentro zonas de recursos extraíbles que me valgan, no se puede establecer nada
                if (bestResources == null)
                    return TaskStatus.Failure;

                // Establecemos la zona de recursos extraíbles candidata como objetivo de extracción
                ExtractableResources.Value = bestResources;
            }
            // Si teníamos ya establecida una zona de recursos extraíbles, seguimos tomándola como objetivo de extracción
            ExtractionTarget.Value = ExtractableResources.Value.transform;
            return TaskStatus.Success;
        }
    }
}