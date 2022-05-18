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
     * Reinicia el comportamiento de extracción, olvidando el objetivo (parando), zona de recursos extraíbles e instalación de procesamiento asignadas.
     * Posibles mejoras:
     * - Considerar si sería interesante reiniciar alguna variables a nulo antes incluso de que Navigate haya concluido con éxito, sólo al recibir la orden del controlador.
     */
    [TaskCategory("RTS")]
    [TaskDescription("Reinicia el comportamiento de extracción, olvidando el objetivo (parando), zona de recursos extraíbles e instalación de procesamiento asignadas.")]
    public class ResetExtraction : Action
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

        // La zona de recursos extraíbles con la que vamos a trabajar.
        // Posibles mejoras: Tener un tipo específico para esto que sea ExtractableResources
        [Tooltip("La zona de recursos extraíbles con la que vamos a trabajar.")]
        [SerializeField] private SharedLimitedAccess _extractableResources;
        public SharedLimitedAccess ExtractableResources { get { return _extractableResources; } private set { _extractableResources = value; } }

        // La instalación de procesamiento con la que vamos a trabajar.
        [Tooltip("La instalación de procesamiento con la que vamos a trabajar.")]
        [SerializeField] private SharedProcessingFacility _processingFacility;
        public SharedProcessingFacility ProcessingFacility { get { return _processingFacility; } private set { _processingFacility = value; } }

        // El acceso limitado con el que vamos a trabajar.
        [Tooltip("El acceso limitado con el que vamos a trabajar.")]
        [SerializeField] private SharedLimitedAccess _processingAccess;
        public SharedLimitedAccess ProcessingAccess { get { return _processingAccess; } private set { _processingAccess = value; } }

        /**************************************************************/

        // La propia unidad donde se ejecuta el comportamiento.
        private ExtractionUnit Unit { get; set; }

        // Despierta la tarea cogiendo una referencia a la propia unidad.
        public override void OnAwake()
        {
            Unit = GetComponent<ExtractionUnit>();
            if (Unit == null)
                throw new System.Exception("El comportamiento no se ejecuta sobre una unidad.");
        }

        // Da orden de parar la unidad, olvida el objetivo de movimiento, la zona de recursos extraíbles y la instalación de procesamiento asignada, y devuelve inmediatamente éxito.
        public override TaskStatus OnUpdate()
        {
            if (RTSController.Value == null)
                throw new System.Exception("No hay controlador asociado.");
            if (Unit == null)
                throw new System.Exception("El comportamiento no se ejecuta sobre una unidad.");
             
            Unit.Stop(RTSController.Value);
            NavigationTarget.Value = null;
            TargetOffset.Value = new Vector3(0.0f, 0.0f, 0.0f);
            ExtractableResources.Value = null;
            ProcessingFacility.Value = null;
            ProcessingAccess.Value = null; 

            return TaskStatus.Success; 
        }
    }
}