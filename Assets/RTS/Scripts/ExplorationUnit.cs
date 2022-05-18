/*    
   Copyright (C) 2020 Federico Peinado
   http://www.federicopeinado.com

   Este fichero forma parte del material de la asignatura Inteligencia Artificial para Videojuegos.
   Esta asignatura se imparte en la Facultad de Informática de la Universidad Complutense de Madrid (España).

   Autores originales: Opsive (Behavior Designer Samples)
   Revisión: Federico Peinado 
   Contacto: email@federicopeinado.com
*/
using System;
using UnityEngine;

namespace es.ucm.fdi.iav.rts
{
    /* 
     * Las unidades de exploración son las más comunes y versátiles, ágiles y proactivas (persiguen, responden ataques...), aunque menos poderosas que las destructoras. 
     * 
     * Posibles mejoras:
     * - Que la distancia de ataque no sea sólo el momento en que la unidad se percata del enemigo, sino que también el ataque físicamente no llegue más allá.
     * - Incluir tal vez varias distancias de ataque, dependiendo del estado y la situación en que se encuentre la unidad exploradora.
     * - Tener una clase padre común para la exploradora y la destructora, tipo CombatUnit.
     * - Diferenciar el código del de la unidad destructora, con estados (isAttacking), targetname de preferencia, guardando unidades enemigas como objetivos a perseguir, etc.
     */
    public class ExplorationUnit : Unit
    {
        // Daño que realiza al atacar
        [SerializeField] private int _attackDamage = 1;  
        public int AttackDamage { get { return _attackDamage; } protected set { _attackDamage = value; } }

        // Tiempo que tarda en atacar
        [SerializeField] private float _attackTime = 0.5f;  
        public float AttackTime { get { return _attackTime; } protected set { _attackTime = value; } }

        // Distancia a la que puede comenzar a atacar        
        [SerializeField] private int _attackDistance = 5;
        public int AttackDistance { get { return _attackDistance; } protected set { _attackDistance = value; } }

        // Indica si esta unidad se siente amenazada o no
        // La unidad de exploración se siente amenazada en cuanto ha perdido algo de vida.
        public bool IsMenaced()
        {
            if (Health.Amount < Health.InitialAmmount)
                return true;
            return false;
        }

        // Se solicita el movimiento de esta unidad a una posición del escenario de batalla. 
        // La unidad exploradora tratará de atacar al enmigo más cercano a la posición dada, siguiéndole si huye, y también contestando a otros enemigos que puedan atacarle por el camino.
        // Si no hubiese enemigos, pues poblados o torretas.
        // Posibles mejoras:
        // - Podría tener dos opciones: apuntar a un objeto concreto al que puedes seguir (ej. su transformada) o simplemente una posición fija.
        // - Procurar que el árbol de comportamiento sea bastante autónomo... sólo cambiar algunas variables compartidas y automáticamente el árbol sabrá por donde ir.
        // - Se podría devolver un booleano para saber si el movimiento se pudo realizar sin problemas.
        public override void Move(RTSController controller, Transform transform)
        {
            if (controller == null)
                throw new ArgumentNullException("No se ha pasado un controlador.");
            if (transform == null)
                throw new ArgumentNullException("No se ha pasado una transformada para el movimiento.");

            int index = RTSGameManager.Instance.GetIndex(controller);
            if (index != GetControllerIndex())
                throw new ArgumentException("El controlador " + index + " no es quien controla a esta unidad");

            // Llamada primero a la clase Unit en general
            base.Move(controller, transform);

            // Luego se pueden consultar algunas variables compartidas del árbol de comportamiento, si hiciera falta...
            //var enemy = behaviors[i].GetVariable("Enemy") as SharedTransform;

            // Se podría modificar una variable compartida del árbol de comportamiento para indicar el objetivo...
            //BehaviorTree.SetVariableValue("Target", GameObject.Find("Village"));  o mejor otra opción que no se usando el nombre
        }
    }
}