/*    
   Copyright (C) 2020 Federico Peinado
   http://www.federicopeinado.com

   Este fichero forma parte del material de la asignatura Inteligencia Artificial para Videojuegos.
   Esta asignatura se imparte en la Facultad de Inform�tica de la Universidad Complutense de Madrid (Espa�a).

   Autores originales: Opsive (Behavior Designer Samples)
   Revisi�n: Federico Peinado 
   Contacto: email@federicopeinado.com
*/
using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;
using Tooltip = BehaviorDesigner.Runtime.Tasks.TooltipAttribute;

namespace es.ucm.fdi.iav.rts
{
    /* Evita que haya m�s de una unidad ocupando un mismo acceso limitado. */
    [TaskCategory("RTS")]
    [TaskDescription("Evita que haya m�s de una unidad ocupando un mismo acceso limitado. ")]
    public class LimitedAccessGuard : Decorator
    {
        // El acceso limitado.
        [Tooltip("El acceso limitado.")] 
        [SerializeField] private SharedLimitedAccess _limitedAccess; 
        public SharedLimitedAccess LimitedAccess { get { return _limitedAccess; } private set { _limitedAccess = value; } }

        /**************************************************/

        private bool Executing { get; set; } = false;

        // La propia unidad donde se ejecuta el comportamiento.
        private Unit Unit { get; set; }

        // Despierta la tarea cogiendo una referencia a la propia unidad donde se est� ejecutando.
        public override void OnAwake()
        {
            Unit = GetComponent<ExtractionUnit>();
            if (Unit == null)
                throw new System.Exception("El comportamiento no se ejecuta sobre una unidad.");
        }

        // S�lo se permiten ejecutar la tarea hija del �rbol de comportamiento si el acceso limitado est� disponible 
        // (libre u ocupado por mi mismo) y la tarea hija no est� ya en ejecuci�n. 
        public override bool CanExecute()
        {
            if (Unit == null)
                throw new System.Exception("El comportamiento no se ejecuta sobre una unidad.");
            // Devolvemos falso (no se puede ejecutar), porque ha desaparecido el acceso limitado.
            if (LimitedAccess.Value == null)
                return false;

            return (LimitedAccess.Value.OccupiedBy == null || LimitedAccess.Value.OccupiedBy == Unit) && !Executing; 
        }

        // Cuando arranca la tarea hija es porque comienza a ejecutarse y el acceso limitado pasa a estar ocupado por la unidad.
        public override void OnChildStarted()
        {
            if (Unit == null)
                throw new System.Exception("El comportamiento no se ejecuta sobre una unidad.");
            // Aqu� no comprobamos si ha desaparecido el acceso limitado. 

            /*
            // No deber�a ocurrir que se intente ocupar algo ya ocupado por ti mismo
            if (LimitedAccess.Value.OccupiedBy == Unit)
                throw new System.Exception("Se intenta ocupar un acceso limitado ya ocupado por esta misma unidad.");

            Debug.Log("Ocupando " + LimitedAccess.Value.name + " por la unidad " + Unit.name);
            LimitedAccess.Value.OccupiedBy = Unit;
            */

            Executing = true;
            // Se desocupar� el acceso cuando la unidad entre en su espacio de colisi�n.
        }

        // Cuando termina la tarea hija es porque deja de ejecutarse.
        public override void OnEnd()
        {
            if (Unit == null)
                throw new System.Exception("El comportamiento no se ejecuta sobre una unidad.");
            // Aqu� no comprobamos si ha desaparecido el acceso limitado. 

            Executing = false;
            // Se desocupar� el acceso cuando la unidad abandone su espacio de colisi�n.
        }

        // Si la tarea hija est� ejecut�ndose, entonces esta tarea tambi�n se considera que est� ejecut�ndose.
        public override TaskStatus OverrideStatus(TaskStatus status)
        {
            if (Unit == null)
                throw new System.Exception("El comportamiento no se ejecuta sobre una unidad.");
            // Devolvemos fracaso, para interrumpirlo todo, porque ha desaparecido el acceso limitado.
            if (LimitedAccess.Value == null)
                return TaskStatus.Failure;

            return !Executing ? TaskStatus.Running : status;
        }
    }
}