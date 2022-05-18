/*    
   Copyright (C) 2020 Federico Peinado
   http://www.federicopeinado.com

   Este fichero forma parte del material de la asignatura Inteligencia Artificial para Videojuegos.
   Esta asignatura se imparte en la Facultad de Inform�tica de la Universidad Complutense de Madrid (Espa�a).

   Autores originales: Opsive (Behavior Designer Samples)
   Revisi�n: Federico Peinado 
   Contacto: email@federicopeinado.com
*/
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using Tooltip = BehaviorDesigner.Runtime.Tasks.TooltipAttribute;

namespace es.ucm.fdi.iav.rts
{
    /* 
     * Descarga todos los recursos que la unidad extractora est� transportando actualmente.
     */
    [TaskCategory("RTS")]
    [TaskDescription("Descarga todos los recursos que la unidad extractora est� transportando actualmente.")]
    public class UnloadResources : Action
    { 
        // La instalaci�n de procesamiento con la que vamos a trabajar.
        [Tooltip("La instalaci�n de procesamiento con la que vamos a trabajar.")]
        [SerializeField] private SharedProcessingFacility _processingFacility;
        public SharedProcessingFacility ProcessingFacility { get { return _processingFacility; } private set { _processingFacility = value; } }

        /********************************************************************/

        // La propia unidad de extracci�n donde se ejecuta el comportamiento.
        private ExtractionUnit ExtractionUnit { get; set; }

        // Despierta la tarea cogiendo una referencia a la propia unidad de extracci�n donde se est� ejecutando.
        public override void OnAwake()
        {
            ExtractionUnit = GetComponent<ExtractionUnit>();
            if (ExtractionUnit == null)
                throw new System.Exception("El comportamiento no se ejecuta sobre una unidad de extracci�n.");
        }

        // Devuelve �xito inmediatamente al descargar los recursos de la unidad extractora a la instalaci�n de procesamiento.
        public override TaskStatus OnUpdate()
        {
            if (ExtractionUnit == null)
                throw new System.Exception("El comportamiento no se ejecuta sobre una unidad de extracci�n.");
            // Devolvemos fracaso, porque ha desaparecido la instalaci�n de procesamiento.
            if (ProcessingFacility.Value == null)
                return TaskStatus.Failure;

            // Se quitan todos los recursos de la unidad de extracci�n.
            int resources = ExtractionUnit.Resources;
            ExtractionUnit.Resources = 0;

            // Se a�aden esos mismos recursos a la instalaci�n de procesamiento.
            ProcessingFacility.Value.Resources += resources;

            // Luego se solicita al gestor del juego que procese la instalaci�n de procesamiento.
            RTSGameManager.Instance.Process(ProcessingFacility.Value);

            return TaskStatus.Success;
        }
    }
}