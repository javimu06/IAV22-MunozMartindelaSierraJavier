/*    
   Copyright (C) 2020 Federico Peinado
   http://www.federicopeinado.com

   Este fichero forma parte del material de la asignatura Inteligencia Artificial para Videojuegos.
   Esta asignatura se imparte en la Facultad de Inform�tica de la Universidad Complutense de Madrid (Espa�a).

   Autores originales: Opsive (Behavior Designer Samples)
   Revisi�n: Federico Peinado 
   Contacto: email@federicopeinado.com
*/
using System;
using UnityEngine;

namespace es.ucm.fdi.iav.rts
{
    /* 
     * Las unidades de exploraci�n son las m�s comunes y vers�tiles, �giles y proactivas (persiguen, responden ataques...), aunque menos poderosas que las destructoras. 
     * 
     * Posibles mejoras:
     * - Que la distancia de ataque no sea s�lo el momento en que la unidad se percata del enemigo, sino que tambi�n el ataque f�sicamente no llegue m�s all�.
     * - Incluir tal vez varias distancias de ataque, dependiendo del estado y la situaci�n en que se encuentre la unidad exploradora.
     * - Tener una clase padre com�n para la exploradora y la destructora, tipo CombatUnit.
     * - Diferenciar el c�digo del de la unidad destructora, con estados (isAttacking), targetname de preferencia, guardando unidades enemigas como objetivos a perseguir, etc.
     */
    public class ExplorationUnit : Unit
    {
        // Da�o que realiza al atacar
        [SerializeField] private int _attackDamage = 1;  
        public int AttackDamage { get { return _attackDamage; } protected set { _attackDamage = value; } }

        // Tiempo que tarda en atacar
        [SerializeField] private float _attackTime = 0.5f;  
        public float AttackTime { get { return _attackTime; } protected set { _attackTime = value; } }

        // Distancia a la que puede comenzar a atacar        
        [SerializeField] private int _attackDistance = 5;
        public int AttackDistance { get { return _attackDistance; } protected set { _attackDistance = value; } }

        // Indica si esta unidad se siente amenazada o no
        // La unidad de exploraci�n se siente amenazada en cuanto ha perdido algo de vida.
        public bool IsMenaced()
        {
            if (Health.Amount < Health.InitialAmmount)
                return true;
            return false;
        }

        // Se solicita el movimiento de esta unidad a una posici�n del escenario de batalla. 
        // La unidad exploradora tratar� de atacar al enmigo m�s cercano a la posici�n dada, sigui�ndole si huye, y tambi�n contestando a otros enemigos que puedan atacarle por el camino.
        // Si no hubiese enemigos, pues poblados o torretas.
        // Posibles mejoras:
        // - Podr�a tener dos opciones: apuntar a un objeto concreto al que puedes seguir (ej. su transformada) o simplemente una posici�n fija.
        // - Procurar que el �rbol de comportamiento sea bastante aut�nomo... s�lo cambiar algunas variables compartidas y autom�ticamente el �rbol sabr� por donde ir.
        // - Se podr�a devolver un booleano para saber si el movimiento se pudo realizar sin problemas.
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

            // Luego se pueden consultar algunas variables compartidas del �rbol de comportamiento, si hiciera falta...
            //var enemy = behaviors[i].GetVariable("Enemy") as SharedTransform;

            // Se podr�a modificar una variable compartida del �rbol de comportamiento para indicar el objetivo...
            //BehaviorTree.SetVariableValue("Target", GameObject.Find("Village"));  o mejor otra opci�n que no se usando el nombre
        }
    }
}