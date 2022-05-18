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
     * Averigua si hay objetivo de extracci�n para la unidad y lo establece en una variable compartida, en principio est�tico (zona de recursos extra�bles).
     * Si no hay una zona de recursos extra�bles ya seleccionada, se escoger� la m�s cercana pero SIEMPRE dentro de una distancia m�xima de detecci�n.
     * Adem�s, si se marca LineOfSight, la l�nea de visi�n tambi�n debe estar despejada.
     * La ubicaci�n concreta estar� situada en el centro de la zona de recursos extraibles para que las unidades de extracci�n trabajen all�.
     * 
     * Posibles mejoras:
     * - Hacer alg�n tipo de chequeo para asegurarnos de que es posible llegar hasta all�, como l�nea de visi�n o preguntando a la malla de navegaci�n.
     * - Leer el tiempo de extracci�n de la unidad (dependiendo de su carga previa) y/o de la dificultad de los recursos extra�bles, y pas�rselo por variable compartida al Wait.
     * - Poder establecer tambi�n un objetivo por tags o por GameObject.Find(targetName), siendo targetName el nombre del objeto en la escena de Unity.
     * - Generalizar SetProcessingTarget y SetExtractionTarget para que encuentren objetos de un cierto tipo a una cierta distancia de la unidad.
     */
    [TaskCategory("RTS")]
    [TaskDescription("Averigua si hay objetivo de extracci�n para la unidad y lo establece en una variable compartida, en principio est�tico (recursos extra�bles).")]
    public class HasExtractionTarget : Conditional
    {
        // Tanto la transformada, como el acceso limitado, deben corresponder al mismo objeto.

        // La transformada del objetivo de extracci�n.
        [Tooltip("La transformada del objetivo de extracci�n.")]
        [SerializeField] private SharedTransform _extractionTarget;
        public SharedTransform ExtractionTarget { get { return _extractionTarget; } private set { _extractionTarget = value; } }

        // La zona de recursos extra�bles con la que vamos a trabajar.
        // Posibles mejoras: Tener un tipo espec�fico para esto que sea ExtractableResources
        [Tooltip("La zona de recursos extra�bles con la que vamos a trabajar.")]
        [SerializeField] private SharedLimitedAccess _extractableResources;
        public SharedLimitedAccess ExtractableResources { get { return _extractableResources; } private set { _extractableResources = value; } }

        // Distancia m�xima a la que la unidad es capaz de detectar zonas de recursos extra�bles cercanas.
        [Tooltip("Distancia m�xima a la que la unidad es capaz de detectar zonas de recursos extra�bles cercanas.")]
        [SerializeField] private float _maxExtractionDistance = 35.0f;
        public float MaxExtractionDistance { get { return _maxExtractionDistance; } }

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
        [SerializeField] private float _sightHeight = 0.2f;
        public float SightHeight { get { return _sightHeight; } }

        /********************************************************************/

        // La propia unidad donde se ejecuta el comportamiento.
        private ExtractionUnit ExtractionUnit { get; set; }

        // Despierta la tarea cogiendo una referencia a la propia unidad, a su controlador y al propio agente de malla de navegaci�n donde se est� ejecutando.
        public override void OnAwake()
        {
            ExtractionUnit = GetComponent<ExtractionUnit>();
            if (ExtractionUnit == null)
                throw new System.Exception("El comportamiento no se ejecuta sobre una unidad de extracci�n.");
        }

        // C�digo que podr�a ser est�tico para comprobar si se tiene l�nea de visi�n con el objetivo de extracci�n.
        // Se lanza un rayo y si colisiona con la transformada del objetivo es que no hay otros objetos obstruyendo la l�nea de visi�n.
        // Si hay mucha cercan�a, tambi�n asumimos que hay visi�n. El problema es que cuando la unidad de extracci�n est� justo encima de una zona de recursos extra�bles y tiene activa LineOfSight, no podr�a verla (porque los rayos no colisionan con colliders que YA EST�N AH� en el inicio).
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

        // Devuelve �xito si puede establecer el objetivo de extracci�n de la unidad en una variable compartida, y fracaso en caso contrario.
        // Se recoge, del escenario, todo lo que est� a CIERTA DISTANCIA, se filtra por accesos limitados (que est� en el escenario, es decir, que no sean instalaciones) y nos quedamos con la m�s cercana (sin importar su ocupaci�n).
        public override TaskStatus OnUpdate()
        {
            if (ExtractionUnit == null)
                throw new System.Exception("El comportamiento no se ejecuta sobre una unidad de extracci�n.");

            // S�lo trato de establecer una nueva zona de extracci�n de recursos si no ten�a escogida otra antes
            // Antes ten�a: (ExtractionTarget.Value == null || ExtractableResources.Value == null)
            if (ExtractableResources.Value == null)
            {
                // C�digo que pod�a ser est�tico para comprobar si hay zonas de recursos extra�bles cercanas.
                // Si no hay colliders, no se puede establecer nada
                var hitColliders = Physics.OverlapSphere(ExtractionUnit.transform.position, MaxExtractionDistance);
                if (hitColliders.Length == 0)  
                    return TaskStatus.Failure;

                LimitedAccess bestResources = null;
                float bestDistance = float.MaxValue; // Empezamos con el valor peor posible
                // Me quedo con la zona de recursos extra�bles m�s cercana, si la hay
                foreach (Collider hitCollider in hitColliders)
                {
                    if (hitCollider.gameObject != null) // Podr�amos asumir que esto ocurre siempre, supongo...
                    {
                        LimitedAccess nearbyResources = hitCollider.gameObject.GetComponent<LimitedAccess>();
                        // Se comprueba si el acceso limitado es hijo, nieto, bisnieto, etc. del escenario (para que no sea una instalaci�n)
                        if (nearbyResources != null && nearbyResources.transform.IsChildOf(RTSScenarioManager.Instance.Scenario.transform))
                        {
                            // Si no hace falta comprobar la l�nea de visi�n, o si hace falta y la hay, consideramos el objetivo
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

                // Si no encuentro zonas de recursos extra�bles que me valgan, no se puede establecer nada
                if (bestResources == null)
                    return TaskStatus.Failure;

                // Establecemos la zona de recursos extra�bles candidata como objetivo de extracci�n
                ExtractableResources.Value = bestResources;
            }
            // Si ten�amos ya establecida una zona de recursos extra�bles, seguimos tom�ndola como objetivo de extracci�n
            ExtractionTarget.Value = ExtractableResources.Value.transform;
            return TaskStatus.Success;
        }
    }
}