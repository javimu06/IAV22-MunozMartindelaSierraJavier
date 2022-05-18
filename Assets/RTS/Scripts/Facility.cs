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
using System;

namespace es.ucm.fdi.iav.rts
{ 
    /* 
     * Las instalaciones son los edificios encargados de la logística del ejército.
     * Tienen ciertas responsabilidades, como crear nuevas unidades o procesar recursos, y si reciben la colisión de un proyectil enemigo modifican su estado de salud.
     * Se trata de una clase abstracta para todas las instalaciones, sean base o de procesamiento.
     * Si se añade este componente a un objeto, será necesario añadir también un componente de salud.
     * 
     * Mejoras posibles: 
     * - Añadir variables con valores enumerados que permitan conocer de manera orientativa el 'estado general' de la instalación.
     * - Añadir unas constantes que proporcionen valores por defecto para atributos de las instalaciones como su coste, velocidad, etc.
     * - Añadir un método de Reset que permita volver a los valores iniciales de la instalación.
     */
    [RequireComponent(typeof(Health))]
    public abstract class Facility : MonoBehaviour
    {
        // Salud de la instalación (por si se desea consultar)
        [SerializeField] public Health Health { get; protected set; }

        // Máximo número de posiciones posibles que pueden intentarse ocupar mis atacantes.
        [SerializeField] private int _maxPositions = 10;
        public int MaxPositions { get { return _maxPositions; } }

        // Distancia a la que mis atancantes deben quedar de mi.
        // Debe ser siempre menor de la distancia de detección de las unidades y del alcance que tengan los proyectiles disparados.
        [SerializeField] private float _radius = 18;
        public float Radius { get { return _radius; } }

        // Siguiente posición de los puntos cercanos (con desplazamiento) que tocaría intentar utilizar.
        // Esto lo utiliza SetTargetOffset y ahora mismo en realidad lo que hace es contar la cantidad de veces que se piden posiciones cercanas...
        [SerializeField] private int _nextPosition = 0;
        public int NextPosition { get { return _nextPosition; } set { _nextPosition = value; } }

        /*********************************************************/

        // El controlador de esta instalación
        protected RTSController Controller { get; set;  }

        // El indicador de activación, para evitar que la instalación se active más de una vez
        private bool Enabled { get; set; }

        // Despierta la instalación y se asegura de que tiene el componente de salud preparado.
        // Es un método virtual (y por tanto no privado) para que pueda ser sobreescrito por las clases hijas.
        // Mejoras posibles: Que cada tipo de unidad particular también despierte e inicialice atributos con valores específicos suyos.
        protected virtual void Awake()
        {
            // Obtenemos el componente de salud y suscribimos nuestra autodestrucción al evento 'en caso de muerte'
            Health = GetComponent<Health>();
            if (Health == null)
                throw new Exception("No hay componente de salud.");
            Health.OnDeath += DestroySelf;
        }

        // Al colisionar se comprueba si se trata de un proyectil que no pertenece al controlador de esta instalación, en cuyo caso se recibe daño.
        // Tanto el proyectil como el objetivo son rigidbodys con un collider en modo trigger, por eso funciona este método privado.
        private void OnTriggerEnter(UnityEngine.Collider trigger)
        {
            Projectile projectile = trigger.gameObject.GetComponent<Projectile>();
            if (projectile != null && projectile.GetControllerIndex() != GetControllerIndex()) // Si el índice es diferente, entonces es enemigo
            {
                // Recibir daño. La salud lanzará un evento si el daño (que puede seguir recibiendo) es igual a 0
                Health.TakeDamage(projectile.DamageAmount);
                // Destruye el proyectil
                projectile.DestroySelf();
            }
        }

        // Devuelve el índice correspondiente a su controlador
        public int GetControllerIndex()
        {
            if (Controller == null)
                throw new ArgumentException("Esta instalación no tiene controlador");

            return RTSGameManager.Instance.GetIndex(Controller);
        }

        // Activa la instalación al comienzo de la ejecución, asignándole su controlador correspondiente 
        public void Enable(RTSController controller)
        {
            if (controller == null)
                throw new ArgumentNullException("Se ha recibido un controlador nulo.");
            if (Enabled)
                throw new Exception("Esta instalación ya estaba activada.");

            Controller = controller; 
            Enabled = true;
        }

        // La instalación ha sido destruida (por un proyectil enemigo o por lo que sea)
        public void DestroySelf()
        {
            // Notifica al gestor del juego
            RTSGameManager.Instance.FacilityDestroyed(this);

            // Y finalmente se autodestruye
            Destroy(gameObject);
        }
    }
}