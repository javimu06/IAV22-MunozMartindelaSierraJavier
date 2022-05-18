/*    
   Copyright (C) 2020 Federico Peinado
   http://www.federicopeinado.com

   Este fichero forma parte del material de la asignatura Inteligencia Artificial para Videojuegos.
   Esta asignatura se imparte en la Facultad de Informática de la Universidad Complutense de Madrid (España).

   Autores originales: Opsive (Behavior Designer Samples)
   Revisión: Federico Peinado 
   Contacto: email@federicopeinado.com
*/
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using Tooltip = BehaviorDesigner.Runtime.Tasks.TooltipAttribute;

namespace es.ucm.fdi.iav.rts
{
    /* 
     * Descarga todos los recursos que la unidad extractora está transportando actualmente.
     */
    [TaskCategory("RTS")]
    [TaskDescription("Descarga todos los recursos que la unidad extractora está transportando actualmente.")]
    public class UnloadResources : Action
    { 
        // La instalación de procesamiento con la que vamos a trabajar.
        [Tooltip("La instalación de procesamiento con la que vamos a trabajar.")]
        [SerializeField] private SharedProcessingFacility _processingFacility;
        public SharedProcessingFacility ProcessingFacility { get { return _processingFacility; } private set { _processingFacility = value; } }

        /********************************************************************/

        // La propia unidad de extracción donde se ejecuta el comportamiento.
        private ExtractionUnit ExtractionUnit { get; set; }

        // Despierta la tarea cogiendo una referencia a la propia unidad de extracción donde se está ejecutando.
        public override void OnAwake()
        {
            ExtractionUnit = GetComponent<ExtractionUnit>();
            if (ExtractionUnit == null)
                throw new System.Exception("El comportamiento no se ejecuta sobre una unidad de extracción.");
        }

        // Devuelve éxito inmediatamente al descargar los recursos de la unidad extractora a la instalación de procesamiento.
        public override TaskStatus OnUpdate()
        {
            if (ExtractionUnit == null)
                throw new System.Exception("El comportamiento no se ejecuta sobre una unidad de extracción.");
            // Devolvemos fracaso, porque ha desaparecido la instalación de procesamiento.
            if (ProcessingFacility.Value == null)
                return TaskStatus.Failure;

            // Se quitan todos los recursos de la unidad de extracción.
            int resources = ExtractionUnit.Resources;
            ExtractionUnit.Resources = 0;

            // Se añaden esos mismos recursos a la instalación de procesamiento.
            ProcessingFacility.Value.Resources += resources;

            // Luego se solicita al gestor del juego que procese la instalación de procesamiento.
            RTSGameManager.Instance.Process(ProcessingFacility.Value);

            return TaskStatus.Success;
        }
    }
}