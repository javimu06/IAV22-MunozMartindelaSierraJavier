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
using BehaviorDesigner.Runtime;
using System;

namespace es.ucm.fdi.iav.rts
{
    /* 
     * Las unidades son los agentes con movilidad y comportamientos inteligentes del ej�rcito.
     * Tienen la responsabilidad de moverse, ya sea para obtener recursos o atacar, y pueden sufrir da�os e incluso ser destruidas por el enemigo o las amenazas del escenario.
     * Se trata de una clase abstracta para todas las unidades, sean extractoras, exploradoras o destructoras.
     * Si se a�ade este componente a un objeto, ser� necesario a�adir tambi�n un componente de salud;
     * y otro de Rigidbody (adem�s con la gravedad activada y con IsKinematic activado) para poder detectar entra o abandona el trigger del acceso limitado.
     * �Si no, el objeto aparece y desaparece todo el rato, e incluso llega a perder el Collider!
     * 
     * Mejoras posibles:
     * - Cargar distintos �rboles de comportamiento, seg�n se est� en modo de ataque (como las exploradoras cuando persiguen) o m�s tranquilo.
     * - A�adir variables con valores enumerados que permitan conocer de manera orientativa el 'estado general' de la unidad.
     * - A�adir unas constantes que proporcionen valores por defecto para atributos de las unidades como su coste, velocidad, etc.
     * - A�adir un m�todo de Reset que permita volver a los valores de la transformada inicial que ten�a la unidad cuando empez� la ejecuci�n, reactivar los �rboles de comportamiento, etc.
     */
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(Rigidbody))]
    public abstract class Unit : MonoBehaviour
    {
        // La transformada con la posici�n objetivo de movimiento de la unidad (se puede establecer un objetivo de movimiento inicial desde el inspector de Unity)
        // Posibles mejoras: Evitar que el setter tenga que ser p�blico para que se puedan fijar nuevos objetivos desde los �rboles de comportamiento
        [SerializeField] public Transform MoveTarget { get; protected set; }

        // Salud de la unidad (por si se desea consultar)
        [SerializeField] public Health Health { get; protected set; }

        // Siguiente posici�n de los puntos cercanos (con desplazamiento) que tocar�a intentar utilizar.
        // Esto lo utiliza SetTargetOffset y ahora mismo en realidad lo que hace es contar la cantidad de veces que se piden posiciones cercanas...
        [SerializeField] private int _nextPosition = 0;
        public int NextPosition { get { return _nextPosition; } set { _nextPosition = value; } }

        // Comportamiento externo a cargar
        // Posibles mejoras: 
        // - Permitir que pueda cargar varios, con una lista
        // - Aclarar si es mejor utilizar la clase ExternalBehaviorTree, que parece ser que s�  
        [SerializeField] private ExternalBehavior _externalBehavior = null;
        public ExternalBehavior ExternalBehavior { get { return _externalBehavior; } }

        // M�ximo n�mero de posiciones posibles que pueden intentarse ocupar mis atacantes.
        [SerializeField] private int _maxPositions = 20;
        public int MaxPositions { get { return _maxPositions; } }

        // Distancia a la que mis atancantes deben quedar de mi.
        // Debe ser siempre menor de la distancia de detecci�n de las unidades y del alcance que tengan los proyectiles disparados.
        [SerializeField] private float _radius = 10;
        public float Radius { get { return _radius; } }

        /*******************************************************************/

        // �rbol de comportamiento que afecta a la unidad 
        // Posibles mejoras: 
        // - Permitir que tenga varios funcionando al mismo tiempo, con una lista
        // - Aclarar si es mejor utilizar la clase Behavior 
        // - Aclarar si permitimos que las subclases lo cambien
        protected BehaviorTree BehaviorTree { get; set; } 

        // El controlador de esta unidad
        protected RTSController Controller { get; set; }

        // El indicador de activaci�n, para evitar que la unidad se active m�s de una vez
        private bool Enabled { get; set; }

        // Objetos auxiliares que se usan como objetivos de movimiento cuando hay que ir a un punto espec�fico del escenario
        // Son dos y se van intercambiando para que el �rbol de comportamiento detecte que se trata de una NUEVA transformada a la que ir, si damos una orden cuando otra est� en marcha
        private Transform _auxMoveTarget1; // Se usa cuando _auxMoveTarget es verdadero
        private Transform _auxMoveTarget2; // Se usa cuando _auxMoveTarget es falso
        private bool _auxMoveTarget = true;

        // Despierta la unidad y se asegura de que tiene los componentes de salud y comportamiento preparados
        // Mejoras posibles: Que cada tipo de unidad particular tambi�n despierte e inicialice atributos con valores espec�ficos suyos
        protected virtual void Awake()
        {
            // Podr�a ubicarlos debajo del controlador correspondiente, por agruparlo mejor... �pero nunca bajo la unidad m�vil, porque entonces tambi�n se mover�a el objetivo!
            _auxMoveTarget1 = (new GameObject("Aux Move 1")).transform; 
            _auxMoveTarget2 = (new GameObject("Aux Move 2")).transform; 

            // Obtenemos el componente de salud y suscribimos nuestra autodestrucci�n al evento 'en caso de muerte'
            Health = GetComponent<Health>(); 
            if (Health == null)
                throw new Exception("No hay componente de salud.");
            Health.OnDeath += DestroySelf;

            // A�ado un �rbol de comportamiento, establezco como comportamiento externo el que tengo fijado y me aseguro de que est� desactivado pero iniciar� al activarse
            if (ExternalBehavior == null)
                throw new Exception("No hay comportamiento externo asignado.");
            BehaviorTree = gameObject.AddComponent<BehaviorTree>(); 
            BehaviorTree.ExternalBehavior = ExternalBehavior;

            // Las propiedades con que quiero que arranque el comportamiento
            BehaviorTree.StartWhenEnabled = true;
            BehaviorTree.AsynchronousLoad = false;
            BehaviorTree.PauseWhenDisabled = true;
            BehaviorTree.RestartWhenComplete = false; // No se van a completar nunca porque tienen un Repeater en la ra�z
            BehaviorTree.ResetValuesOnRestart = false;
            BehaviorTree.LogTaskChanges = false;

            // Me aseguro de que el comportamiento est� activo (y por lo tanto iniciar� su actividad)
            BehaviorTree.EnableBehavior(); 
        }

        // Al colisionar se comprueba si se trata de un proyectil que no pertenece al controlador de esta unidad, en cuyo caso se recibe da�o 
        // Tanto el proyectil como el objetivo son rigidbodys con un collider en modo trigger, por eso funciona este m�todo privado.
        private void OnTriggerEnter(UnityEngine.Collider trigger)
        {
            Projectile projectile = trigger.gameObject.GetComponent<Projectile>();
            if (projectile != null && projectile.GetControllerIndex() != GetControllerIndex()) // Si el �ndice es diferente, entonces es enemigo
            {
                // Recibir da�o. La salud lanzar� un evento si el da�o (que puede seguir recibiendo) es igual a 0
                Health.TakeDamage(projectile.DamageAmount);
                // Destruye el proyectil
                projectile.DestroySelf();
            }
        }

        // Se solicita el movimiento de esta unidad a una transformada concreta del escenario de batalla. 
        // Si esta unidad se mueve, se realizar� un seguimiento.
        // Dependiendo de la unidad concreta, luego puede que esta se ponga a extraer los recursos de un campo cercano, o a atacar alguna unidad o instalaci�n enemiga.
        // Es un m�todo virtual porque se espera que cada unidad espec�fica lo extienda a su manera.
        // Posibles mejoras: 
        // - Procurar que el �rbol de comportamiento sea bastante aut�nomo... s�lo cambiar algunas variables compartidas y autom�ticamente el �rbol sabr� por donde ir.
        // - Se podr�a devolver un booleano para saber si el movimiento se pudo realizar sin problemas (pero eso implicar�a esperar al pathfinding y al resultado del movimiento, de hecho).
        public virtual void Move(RTSController controller, Transform transform) 
        {
            if (controller == null)
                throw new ArgumentNullException("No se ha pasado un controlador."); 
            if (transform == null)
                throw new ArgumentNullException("No se ha pasado una transformada para el movimiento.");

            int index = RTSGameManager.Instance.GetIndex(controller);
            if (index != GetControllerIndex())
                throw new ArgumentException("El controlador " + index + " no es quien controla a esta unidad");

            // Por ahora el objetivo del movimiento es algo gen�rico, no espec�fico de cada unidad
            MoveTarget = transform;
        }

        // Se solicita el movimiento de esta unidad a una posici�n concreta del escenario de batalla. 
        // Dependiendo de la unidad concreta, luego puede que esta se ponga a extraer los recursos de un campo cercano, o a atacar alguna unidad o instalaci�n enemiga.
        // Es un m�todo virtual porque se espera que cada unidad espec�fica lo extienda a su manera.
        // Posibles mejoras: 
        // - Procurar que el �rbol de comportamiento sea bastante aut�nomo... s�lo cambiar algunas variables compartidas y autom�ticamente el �rbol sabr� por donde ir.
        // - Se podr�a devolver un booleano para saber si el movimiento se pudo realizar sin problemas.
        public virtual void Move(RTSController controller, Vector3 position)
        {
            if (controller == null)
                throw new ArgumentNullException("No se ha pasado un controlador.");
            if (transform == null)
                throw new ArgumentNullException("No se ha pasado una transformada para el movimiento.");

            int index = RTSGameManager.Instance.GetIndex(controller);
            if (index != GetControllerIndex())
                throw new ArgumentException("El controlador " + index + " no es quien controla a esta unidad");

            // Por ahora el objetivo del movimiento es algo gen�rico, no espec�fico de cada unidad
            // Se utiliza el objetivo auxiliar para posicionar esto
            if (_auxMoveTarget)
            {
                _auxMoveTarget1.position = position;
                MoveTarget = _auxMoveTarget1;
                _auxMoveTarget = false;
            } else
            {
                _auxMoveTarget2.position = position;
                MoveTarget = _auxMoveTarget2;
                _auxMoveTarget = true;
            }
        }

        // ES SUTILMENTE DIFERENTE PEDIR A UNA UNIDAD QUE SE QUEDE QUIETA, MANDANDO UN MOVE A SU PROPIA POSICI�N, QUE PEDIR QUE NO TENGA ORDEN DE MOVERSE
        // Solicita la detenci�n de esta unidad, anulando el objetivo de movimiento que pudiera haber establecido
        // Lo puede llamar el controlador, aunque tambi�n se utiliza desde alguna tarea del �rbol de comportamiento
        public virtual void Stop(RTSController controller)
        {
            if (controller == null)
                throw new ArgumentNullException("No se ha pasado un controlador.");

            int index = RTSGameManager.Instance.GetIndex(controller);
            if (index != GetControllerIndex())
                throw new ArgumentException("El controlador " + index + " no es quien controla a esta unidad");

            MoveTarget = null; 
        }

        // Devuelve el �ndice correspondiente a su controlador
        public int GetControllerIndex()
        {
            if (Controller == null)
                throw new ArgumentException("Esta instalaci�n no tiene controlador");

            return RTSGameManager.Instance.GetIndex(Controller);
        }

        // Activa la unidad al comienzo de la ejecuci�n, asign�ndole su controlador correspondiente, incluso al �rbol de comportamiento y activando a este �ltimo tambi�n.
        public void Enable(RTSController controller)
        {
            if (controller == null)
                throw new ArgumentNullException("Se ha recibido un controlador nulo.");
            if (Enabled)
                throw new Exception("Esta unidad ya estaba activada.");

            Controller = controller;
            //BehaviorTree.EnableBehavior(); Si lo hemos activado antes, no hace falta volver a hacer ahora

            /* LO QUE US� PARA COMPROBAR QUE ESTO FUNCIONA
             * var variables = BehaviorTree.GetAllVariables();
             * var variable = BehaviorTree.GetVariable("RTSController");
             * */

            // Esto est� funcionado PERFECTAMENTE, OJO (aunque me est� lanzando una excepci�n... a ver si la excepci�n no salta al tener alg�n valor dentro o algo). Por ahora se pasa el GameObject entero, no el componente RTSController
            BehaviorTree.SetVariableValue("RTSController", controller);  

            Enabled = true;
        }

        // La unidad ha sido destruida (por un proyectil enemigo o por lo que sea)
        // Posible mejora: Usar behavior.DisableBehavior(); por si da problemas el destruir un objeto que tiene un �rbol de comportamiento funcionando
        public void DestroySelf()
        {
            // Desactivo el comportamiento (y por tanto detengo su actividad)
            BehaviorTree.DisableBehavior();

            // Notifica al gestor del juego
            RTSGameManager.Instance.UnitDestroyed(this); 

            // Y finalmente se autodestruye
            Destroy(gameObject);
        }
    }
}