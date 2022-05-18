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
    /* Determina si un acceso limitado está o no ocupado por otra unidad diferente. */
    [TaskCategory("RTS")]
    [TaskDescription("Determina si un acceso limitado está o no ocupado por otra unidad diferente.")]
    public class IsOccupiedByOther : Conditional
    {
        // El acceso limitado.
        [Tooltip("El acceso limitado.")]
        [SerializeField] private SharedLimitedAccess _limitedAccess;
        public SharedLimitedAccess LimitedAccess { get { return _limitedAccess; } private set { _limitedAccess = value; } }

        /***************************************************/

        // La propia unidad donde se ejecuta el comportamiento.
        private Unit Unit { get; set; }

        // Despierta la tarea cogiendo una referencia a la propia unidad.
        public override void OnAwake()
        {
            Unit = GetComponent<ExtractionUnit>();
            if (Unit == null)
                throw new System.Exception("El comportamiento no se ejecuta sobre una unidad.");
        }

        // Devuelve inmediatamente éxito si el acceso limitado está ocupado (¡por alguien que no sea yo!), y fracaso en caso contrario. 
        public override TaskStatus OnUpdate()
        { 
            if (Unit == null)
                throw new System.Exception("El comportamiento no se ejecuta sobre una unidad."); 
            // Devolvemos éxito (ocupado por 'otro'), porque ha desaparecido el acceso limitado.
            if (LimitedAccess.Value == null)
                return TaskStatus.Success;

            // Habría que ver en qué casos fracasa esto porque lo estoy ocupando yo mismo
            if (LimitedAccess.Value.OccupiedBy != null && LimitedAccess.Value.OccupiedBy != Unit)
            { 
                return TaskStatus.Success;
            }

            return TaskStatus.Failure;
        }
    }
}