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
     * Las unidades de destrucci�n son las m�s poderosas y m�s enfocadas al ataque, aunque menos �giles y m�s inc�modads de manejar que las exploradoras (no persiguen, no responden ataques...). 
     * 
     * Posibles mejoras:
     * - Que la distancia de ataque no sea s�lo el momento en que la unidad se percata del enemigo, sino que tambi�n el ataque f�sicamente no llegue m�s all�.
     * - Diferenciar el c�digo del de la unidad exploradora, con estados (isAttacking), targetname de preferencia, sin guardar unidades enemigas como objetivos a perseguir, etc.
     */
    public class DestructionUnit : Unit
    {
        // Da�o que realiza al atacar
        [SerializeField] private int _attackDamage = 10;  
        public int AttackDamage { get { return _attackDamage; } protected set { _attackDamage = value; } }

        // Tiempo que tarda en atacar
        [SerializeField] private float _attackTime = 2f;  
        public float AttackTime { get { return _attackTime; } protected set { _attackTime = value; } }

        // Distancia a la que puede comenzar a atacar        
        [SerializeField] private int _attackDistance = 20;
        public int AttackDistance { get { return _attackDistance; } protected set { _attackDistance = value; } }

        /********************************************************/

        // Objetos auxiliares que se usan como objetivos de ataque cuando hay que ir a un punto espec�fico del escenario
        // Son dos y se van intercambiando para que el �rbol de comportamiento detecte que se trata de una NUEVA transformada a la que ir, si damos una orden cuando otra est� en marcha
        private Transform _auxAttackTarget1; // Se usa cuando _auxAttackTarget es verdadero
        private Transform _auxAttackTarget2; // Se usa cuando _auxAttackTarget es falso
        private bool _auxAttackTarget = true;

        // Despierta la unidad y se asegura de que tiene los componentes de salud y comportamiento preparados
        // Mejoras posibles: Que cada tipo de unidad particular tambi�n despierte e inicialice atributos con valores espec�ficos suyos
        protected override void Awake()
        {
            _auxAttackTarget1 = (new GameObject("Aux Attack 1")).transform;
            _auxAttackTarget2 = (new GameObject("Aux Attack 2")).transform;

            // Llamada a la clase Unit en general
            base.Awake();
        }

        // Indica si esta unidad se siente amenazada o no
        // La unidad de destrucci�n es muy profesional y no se siente amenazada nunca.
        public bool IsMenaced()
        { 
            return false;
        }

        // Se solicita el movimiento de esta unidad a una posici�n del escenario de batalla. 
        // La unidad destructora se centrar� �nicamente en atacar el enmigo m�s cercano a la posici�n dada (si no hubiera enemigos, pues una torreta). 
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

            // Llamada a la clase Unit en general
            base.Move(controller, transform);

            // Luego se pueden consultar algunas variables compartidas del �rbol de comportamiento, si hiciera falta...
            //var enemy = behaviors[i].GetVariable("Enemy") as SharedTransform;

            // Se podr�a modificar una variable compartida del �rbol de comportamiento para indicar el objetivo...
            //BehaviorTree.SetVariableValue("Target", GameObject.Find("Village"));  o mejor otra opci�n que no se usando el nombre
        }

        // Se solicita el ataque de esta unidad a una transformada enemiga concreta. 
        // Si esta unidad se mueve, se realizar� un seguimiento.
        // Dependiendo de la unidad concreta, luego puede que esta se ponga a atacar alguna unidad o instalaci�n enemiga. 
        // Posibles mejoras: 
        // - Procurar que el �rbol de comportamiento sea bastante aut�nomo... s�lo cambiar algunas variables compartidas y autom�ticamente el �rbol sabr� por donde ir.
        // - Se podr�a devolver un booleano para saber si el ataque se pudo realizar sin problemas (pero eso implicar�a esperar al resultado de lanzar proyectiles, de hecho).
        public void Attack(RTSController controller, Transform transform)
        {
            if (controller == null)
                throw new ArgumentNullException("No se ha pasado un controlador.");
            if (transform == null)
                throw new ArgumentNullException("No se ha pasado una transformada para el ataque.");

            int index = RTSGameManager.Instance.GetIndex(controller);
            if (index != GetControllerIndex())
                throw new ArgumentException("El controlador " + index + " no es quien controla a esta unidad");

            // El objetivo de ataque
            //AttackTarget = transform;
        }

        // Se solicita el movimiento de esta unidad a una posici�n concreta del escenario de batalla. 
        // Dependiendo de la unidad concreta, luego puede que esta se ponga a atacar alguna unidad o instalaci�n enemiga. 
        // Posibles mejoras: 
        // - Procurar que el �rbol de comportamiento sea bastante aut�nomo... s�lo cambiar algunas variables compartidas y autom�ticamente el �rbol sabr� por donde ir.
        // - Se podr�a devolver un booleano para saber si el ataque se pudo realizar sin problemas (pero eso implicar�a esperar al resultado de lanzar proyectiles, de hecho).
        public void Attack(RTSController controller, Vector3 position)
        {
            if (controller == null)
                throw new ArgumentNullException("No se ha pasado un controlador.");
            if (transform == null)
                throw new ArgumentNullException("No se ha pasado una transformada para el ataque.");

            int index = RTSGameManager.Instance.GetIndex(controller);
            if (index != GetControllerIndex())
                throw new ArgumentException("El controlador " + index + " no es quien controla a esta unidad");

            // El objetivo de ataque 
            // Se utiliza el objetivo auxiliar para posicionar esto
            if (_auxAttackTarget)
            {
                _auxAttackTarget1.position = position;
                //AttackTarget = _auxAttackTarget1;
                _auxAttackTarget = false;
            }
            else
            {
                _auxAttackTarget2.position = position;
                //AttackTarget = _auxAttackTarget2;
                _auxAttackTarget = true;
            }
        }
    }
}