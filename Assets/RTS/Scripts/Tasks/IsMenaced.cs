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
using System;

namespace es.ucm.fdi.iav.rts
{
    /* Determina si la unidad de combate está amenazada ahora mismo. Concretamente si se tiene menos de la mitad de salud inicial. 
     * Esto puede usarse para que sólo se ataque (o se ataque con mayor furia) si has sufrido daños (seas unidad, torre o lo que sea).
     * Posibles mejoras:
     * - Usar algún otro criterio a parte de tener pérdida de salud, como si tengo o no activado el Follow (incluirlo aquí) o si me ha lanzado hace poco un misil ESE ENEMIGO al que sigo, etc.
     */ 
    [TaskCategory("RTS")]
    [TaskDescription("Determina si la unidad de combate está amenazada ahora mismo.")]
    public class IsMenaced : Conditional
    {
        // La propia unidad donde se ejecuta el comportamiento, si se trata de una unidad.
        // Posibles mejoras: Que sea una CombatUnit, una clase común a la unidad de destrucción y a la de exploración
        private Unit Unit { get; set; }
        private DestructionUnit DestructionUnit { get; set; }
        private ExplorationUnit ExplorationUnit { get; set; }

        // Salud de la unidad  
        private Health Health { get; set;  }

        // Despierta la tarea cogiendo una referencia a la propia unidad de combate donde se está ejecutando
        public override void OnAwake()
        {
            Unit = GetComponent<Unit>(); 
            if (Unit == null)
                throw new System.Exception("El comportamiento no se ejecuta sobre una unidad.");

            DestructionUnit = GetComponent<DestructionUnit>();
            ExplorationUnit = GetComponent<ExplorationUnit>();
            if (DestructionUnit == null && ExplorationUnit == null)
                throw new System.Exception("El comportamiento no se ejecuta sobre una unidad de combate (destructora o exploradora).");

            Health = GetComponent<Health>();
            if (Health == null)
                throw new Exception("No hay componente de salud.");
        }

        // Devuelve inmediatamente éxito si la unidad ha perdido salud, y fracaso en caso contrario. 
        public override TaskStatus OnUpdate()
        {
            if (Unit == null)
                throw new System.Exception("El comportamiento no se ejecuta sobre una unidad.");
            if (DestructionUnit == null && ExplorationUnit == null)
                throw new System.Exception("El comportamiento no se ejecuta sobre una unidad de combate (destructora o exploradora).");
            if (Health == null)
                throw new Exception("No hay componente de salud.");

            if (ExplorationUnit != null && ExplorationUnit.IsMenaced())
                return TaskStatus.Success;
            if (DestructionUnit != null && DestructionUnit.IsMenaced())
                return TaskStatus.Success;

            return TaskStatus.Failure;
        }
    }
}