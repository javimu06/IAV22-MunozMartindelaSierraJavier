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
     * Averigua si hay objetivo de procesamiento para la unidad y lo establece en una variable compartida, en principio estático (una instalación de procesamiento).
     * Si no hay una instalación de procesamiento ya seleccionada, siempre se escogerá la más cercana de todas.
     * La ubicación concreta estará situada bajo el 'soportal' que tienen las unidades de procesamiento para que las unidades de extracción descarguen allí.
     * 
     * Posibles mejoras:
     * - Pensar si podría utilizarse algún tipo de distancia máxima de detección de instalaciones de procesamiento, o hacer algo en caso de que no queden de ese tipo de instalaciones 
     *   (o merodear en caso contrario, porque lo que no es válido es quedarse parado justo en la zona de recursos extraíbles después de haberlos extraido)
     * - Asegurarnos de que las variables compartidas (transformada, unidad y acceso limitado) corresponden todas la mismo Game Object.
     * - Hacer algún tipo de chequeo para asegurarnos de que es posible llegar hasta allí, como línea de visión (aunque no tiene mucho sentido) o preguntando a la malla de navegación.
     * - Leer el tiempo de descarga de la unidad (dependiendo de su carga) y/o de la instalación de procesamiento, y pasárselo por variable compartida al Wait.
     * - Poder establecer también un objetivo por tags o por GameObject.Find(targetName), siendo targetName el nombre del objeto en la escena de Unity.
     * - Generalizar SetProcessingTarget y SetExtractionTarget para que encuentren objetos de un cierto tipo a una cierta distancia de la unidad.
     */
    [TaskCategory("RTS")]
    [TaskDescription("Averigua si hay objetivo de procesamiento para la unidad y lo establece en una variable compartida, en principio estático (una instalación de procesamiento).")]
    public class HasProcessingTarget : Conditional 
    {
        // Tanto la transformada, como la instalación de procesamiento, como el acceso limitado, deben corresponder al mismo objeto.

        // La transformada del objetivo de procesamiento.
        [Tooltip("La transformada del objetivo de procesamiento.")]
        [SerializeField] private SharedTransform _processingTarget; 
        public SharedTransform ProcessingTarget { get { return _processingTarget; } private set { _processingTarget = value; } }

        // La instalación de procesamiento con la que vamos a trabajar.
        [Tooltip("La instalación de procesamiento con la que vamos a trabajar.")]
        [SerializeField] private SharedProcessingFacility _processingFacility;
        public SharedProcessingFacility ProcessingFacility { get { return _processingFacility; } private set { _processingFacility = value; } }

        // El acceso limitado con el que vamos a trabajar.
        [Tooltip("El acceso limitado con el que vamos a trabajar.")]
        [SerializeField] private SharedLimitedAccess _processingAccess;
        public SharedLimitedAccess ProcessingAccess { get { return _processingAccess; } private set { _processingAccess = value; } }

        /********************************************************************/

        // La propia unidad donde se ejecuta el comportamiento.
        private ExtractionUnit ExtractionUnit { get; set; }

        // Despierta la tarea cogiendo una referencia a la propia unidad.
        public override void OnAwake()
        {
            ExtractionUnit = GetComponent<ExtractionUnit>();
            if (ExtractionUnit == null)
                throw new System.Exception("El comportamiento no se ejecuta sobre una unidad de extracción.");
        }

        // Devuelve éxito si puede establecer el objetivo de procesamiento de la unidad en una variable compartida, y fracaso en caso contrario.
        // Se recoge todo, se filtra por instalaciones de procesamiento que tengan mi índice y nos quedamos con la más cercana (sin importar su ocupación).
        public override TaskStatus OnUpdate()
        {
            if (ExtractionUnit == null)
                throw new System.Exception("El comportamiento no se ejecuta sobre una unidad de extracción.");

            // Sólo trato de establecer una nueva instalación de procesamiento si no tenía otra bien escogida antes
            // Antes tenía (ProcessingTarget.Value == null || ProcessingFacility.Value == null || ProcessingAccess.Value == null)
            if (ProcessingFacility.Value == null)
            {
                // Si no quedan instalaciones de procesamiento, no se puede establecer nada
                var facilities = RTSGameManager.Instance.GetProcessingFacilities(ExtractionUnit.GetControllerIndex());          
                if (facilities.Count == 0)
                    return TaskStatus.Failure; 

                ProcessingFacility bestFacility = null;
                float bestDistance = float.MaxValue; // Empezamos con el valor peor posible
                // Me quedo con la instalación de procesamiento más cercana, si la hay
                foreach (ProcessingFacility facility in facilities)
                {
                    // Todas las instalaciones de procesamiento deben tener punto de descarga y ser a la vez accesos limitados
                    if (facility.UnloadingTransform == null)
                        throw new System.Exception("La instalación de procesamiento no tiene punto de descarga.");
                    if (facility.GetComponent<LimitedAccess>() == null)
                        throw new System.Exception("La instalación que se quiere establecer no es un acceso limitado.");

                    // Al mirar la distancia, se tiene en cuenta el UnloadingTransform
                    float distance = Vector3.Distance(ExtractionUnit.transform.position, facility.UnloadingTransform.position);
                    if (distance < bestDistance)
                    {
                        bestFacility = facility;
                        bestDistance = distance;
                    } 
                }

                // Obligatoriamente habrá salido una instalación de procesamiento candidata, y la establecemos como objetivo de procesamiento
                ProcessingFacility.Value = bestFacility;
            }
            // Si teníamos ya establecida una instalación de procesamiento, seguimos tomándola como objetivo de procesamiento
            ProcessingTarget.Value = ProcessingFacility.Value.UnloadingTransform; // Aún así, luego SetTargetOffset también es consciente de que hay que usar el UnloadTransform
            ProcessingAccess.Value = ProcessingFacility.Value.GetComponent<LimitedAccess>();
            return TaskStatus.Success;
        }
    }
}