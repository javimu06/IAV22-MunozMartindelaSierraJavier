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

namespace es.ucm.fdi.iav.rts
{
    /* 
     * Extrae recursos a la unidad extractora, provenientes de una zona de recursos extraíbles.
     * Posibles mejoras:
     * - Que se utilice la zona de recursos extraíbles para saber cuanta cantidad de recursos se obtiene.
     */
    [TaskCategory("RTS")]
    [TaskDescription("Extrae recursos a la unidad extractora, provenientes de una zona de recursos extraíbles")]
    public class ExtractResources : Action
    {
        // Cantidad de recursos que se obtienen con la extracción.
        [Tooltip("Cantidad de recursos que se obtienen con la extracción.")]
        [SerializeField] private int _amount = 1000;
        public int Amount { get { return _amount; } }

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

        // Devuelve éxito inmediatamente al extraer los recursos de la zona de recursos extraíbles a la unidad extractora.
        public override TaskStatus OnUpdate()
        {
            if (ExtractionUnit == null)
                throw new System.Exception("El comportamiento no se ejecuta sobre una unidad de extracción.");

            ExtractionUnit.Resources += Amount; 

            return TaskStatus.Success;
        }
    }
}