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
using System;

namespace es.ucm.fdi.iav.rts
{
    /* 
     * Las torretas son edificios supuestamente neutrales, que atacarán si se sienten invadidos por tu proximidad.
     * No se mueven, pero disparan proyectiles y pueden sufrir daños e incluso ser destruidas por el enemigo o las amenazas del escenario. 
     * Si se añade este componente a un objeto, será necesario añadir también un componente de salud.
    * 
    * Posibles mejoras:
    * - Que la distancia de ataque no sea sólo el momento en que la unidad se percata del enemigo, sino que también el ataque físicamente no llegue más allá. 
    * - Que las torretas una vez destruidas puedan regenerarse y volver a aparecer (las habría de dos tipos, unas que se regeneran y otras que no)
    */
    [RequireComponent(typeof(Health))]
    public class Tower : MonoBehaviour
    {
        // Daño que realiza al atacar
        [SerializeField] private int _attackDamage = 1;
        public int AttackDamage { get { return _attackDamage; } protected set { _attackDamage = value; } }

        // Tiempo que tarda en atacar
        [SerializeField] private float _attackTime = 0.5f;
        public float AttackTime { get { return _attackTime; } protected set { _attackTime = value; } }

        // Distancia a la que puede comenzar a atacar        
        [SerializeField] private int _attackDistance = 15;
        public int AttackDistance { get { return _attackDistance; } protected set { _attackDistance = value; } }

        // Máximo número de posiciones posibles que pueden intentarse ocupar mis atacantes.
        [SerializeField] private int _maxPositions = 20;
        public int MaxPositions { get { return _maxPositions; } }

        // Distancia a la que mis atancantes deben quedar de mi.
        // Debe ser siempre menor de la distancia de detección de las unidades y del alcance que tengan los proyectiles disparados.
        [SerializeField] private float _radius = 10;
        public float Radius { get { return _radius; } }

        // Siguiente posición de los puntos cercanos (con desplazamiento) que tocaría intentar utilizar.
        // Esto lo utiliza SetTargetOffset y ahora mismo en realidad lo que hace es contar la cantidad de veces que se piden posiciones cercanas...
        [SerializeField] private int _nextPosition = 0;
        public int NextPosition { get { return _nextPosition; } set { _nextPosition = value; } }

        // Comportamiento externo a cargar
        // Posibles mejoras:  
        // - Aclarar si es mejor utilizar la clase ExternalBehaviorTree, que parece ser que sí 
        [SerializeField] private ExternalBehavior _externalBehavior = null;
        public ExternalBehavior ExternalBehavior { get { return _externalBehavior; } }

        // Árbol de comportamiento que afecta a la torreta
        // Posibles mejoras:  
        // - Aclarar si es mejor utilizar la clase Behavior 
        // - Aclarar si permitimos que las subclases lo cambien
        // - A los proyectiles que dispara una torreta se les podría dar un índice distintivo, como lo tienen los controladores, para reconocer de qué torreta vino
        protected BehaviorTree BehaviorTree { get; set; }

        // Salud de la torreta
        protected Health Health { get; set; }

        /**********************************************************/

        /* NO SÉ SI ME HARÁ FALTA ESTO, COMO EN LAS UNIDADES...
        // Objetos auxiliares que se usan como objetivos de ataque cuando hay que ir a un punto específico del escenario
        // Son dos y se van intercambiando para que el árbol de comportamiento detecte que se trata de una NUEVA transformada a la que ir, si damos una orden cuando otra está en marcha
        private Transform _auxAttackTarget1; // Se usa cuando _auxAttackTarget es verdadero
        private Transform _auxAttackTarget2; // Se usa cuando _auxAttackTarget es falso
        private bool _auxAttackTarget = true;
        */

        // Cuantos me están atacando ahora
        private int AttackerUnits { get; set; } = 0;

        // Despierta la torreta y se asegura de que tiene los componentes de salud y comportamiento preparados
        // Mejoras posibles: Que cada tipo de unidad particular también despierte e inicialice atributos con valores específicos suyos
        protected void Awake()
        {
            // Obtenemos el componente de salud y suscribimos nuestra autodestrucción al evento 'en caso de muerte'
            Health = GetComponent<Health>();
            if (Health == null)
                throw new Exception("No hay componente de salud.");
            Health.OnDeath += DestroySelf;

            // Añado un árbol de comportamiento, establezco como comportamiento externo el que tengo fijado y me aseguro de que está desactivado pero iniciará al activarse
            if (ExternalBehavior == null)
                throw new Exception("No hay comportamiento externo asignado.");
            BehaviorTree = gameObject.AddComponent<BehaviorTree>();
            BehaviorTree.ExternalBehavior = ExternalBehavior;

            // Las propiedades con que quiero que arranque el comportamiento
            BehaviorTree.StartWhenEnabled = true;
            BehaviorTree.AsynchronousLoad = false;
            BehaviorTree.PauseWhenDisabled = true;
            BehaviorTree.RestartWhenComplete = false; // No se van a completar nunca porque tienen un Repeater en la raíz
            BehaviorTree.ResetValuesOnRestart = false;
            BehaviorTree.LogTaskChanges = false;

            // Me aseguro de que el comportamiento está activo (y por lo tanto iniciará su actividad)
            BehaviorTree.EnableBehavior();
        }

        /*
        // Devuelve la siguiente posición alrededor suyo, para ayudar a orientarse a las unidades que la vayan a atacar. 
        // La idea es que estas posiciones se alineen en círculo alrededor del edificio.
        // Mejoras posibles: 
        // - Podría devolver una transformada (Transform), aunque la rotación tampoco sea muy relevante, es cosa de mirar hacia el objetivo, nada más.
        // - Se debería ir comprobando los huecos (como las instalaciones base al generar unidades) para descontar el número de atacantes según van apareciendo esos huecos
        public Vector3 NextAttackTransform()
        {
            // Usa la ecuación paramétrica del círculo para determinar la siguiente posición de ataque
            var position = transform.position;
            position.x += AttackerRadius * Mathf.Sin((AttackerSpread * AttackerUnits + AttackerSpreadOffset) * Mathf.Deg2Rad);
            position.z += AttackerRadius * Mathf.Cos((AttackerSpread * AttackerUnits + AttackerSpreadOffset) * Mathf.Deg2Rad);
            AttackerUnits++; // Para ir contando cuantos atacantes tiene acumulados a su alrededor...
            return position;
        } */

        // Al colisionar se comprueba si se trata de un proyectil (sin importar el controlador), en cuyo caso se recibe daño 
        // Tanto el proyectil como el objetivo son rigidbodys con un collider en modo trigger, por eso funciona este método privado.
        // Mejoras posibles: Contabilizar quien ha abatido más torretas, para cada bando.
        private void OnTriggerEnter(UnityEngine.Collider trigger)
        {
            Projectile projectile = trigger.gameObject.GetComponent<Projectile>();
            if (projectile != null) // Todos los proyectiles lo dañan
            {
                // Recibir daño. La salud lanzará un evento si el daño (que puede seguir recibiendo) es igual a 0
                Health.TakeDamage(projectile.DamageAmount);
                // Destruye el proyectil
                projectile.DestroySelf();
            }
        } 

        // La torreta ha sido destruida (por un proyectil enemigo o por lo que sea)
        // Posible mejora: Usar behavior.DisableBehavior(); por si da problemas el destruir un objeto que tiene un árbol de comportamiento funcionando
        public void DestroySelf()
        {
            // Desactivo el comportamiento (y por tanto detengo su actividad)
            BehaviorTree.DisableBehavior();

            // Notifica al gestor del juego
            RTSScenarioManager.Instance.TowerDestroyed(this);

            // Y finalmente se autodestruye
            Destroy(gameObject);
        }
    }
}