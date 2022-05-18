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
using System;

namespace es.ucm.fdi.iav.rts
{ 
    /* 
     * Las instalaciones son los edificios encargados de la log�stica del ej�rcito.
     * Tienen ciertas responsabilidades, como crear nuevas unidades o procesar recursos, y si reciben la colisi�n de un proyectil enemigo modifican su estado de salud.
     * Se trata de una clase abstracta para todas las instalaciones, sean base o de procesamiento.
     * Si se a�ade este componente a un objeto, ser� necesario a�adir tambi�n un componente de salud.
     * 
     * Mejoras posibles: 
     * - A�adir variables con valores enumerados que permitan conocer de manera orientativa el 'estado general' de la instalaci�n.
     * - A�adir unas constantes que proporcionen valores por defecto para atributos de las instalaciones como su coste, velocidad, etc.
     * - A�adir un m�todo de Reset que permita volver a los valores iniciales de la instalaci�n.
     */
    [RequireComponent(typeof(Health))]
    public abstract class Facility : MonoBehaviour
    {
        // Salud de la instalaci�n (por si se desea consultar)
        [SerializeField] public Health Health { get; protected set; }

        // M�ximo n�mero de posiciones posibles que pueden intentarse ocupar mis atacantes.
        [SerializeField] private int _maxPositions = 10;
        public int MaxPositions { get { return _maxPositions; } }

        // Distancia a la que mis atancantes deben quedar de mi.
        // Debe ser siempre menor de la distancia de detecci�n de las unidades y del alcance que tengan los proyectiles disparados.
        [SerializeField] private float _radius = 18;
        public float Radius { get { return _radius; } }

        // Siguiente posici�n de los puntos cercanos (con desplazamiento) que tocar�a intentar utilizar.
        // Esto lo utiliza SetTargetOffset y ahora mismo en realidad lo que hace es contar la cantidad de veces que se piden posiciones cercanas...
        [SerializeField] private int _nextPosition = 0;
        public int NextPosition { get { return _nextPosition; } set { _nextPosition = value; } }

        /*********************************************************/

        // El controlador de esta instalaci�n
        protected RTSController Controller { get; set;  }

        // El indicador de activaci�n, para evitar que la instalaci�n se active m�s de una vez
        private bool Enabled { get; set; }

        // Despierta la instalaci�n y se asegura de que tiene el componente de salud preparado.
        // Es un m�todo virtual (y por tanto no privado) para que pueda ser sobreescrito por las clases hijas.
        // Mejoras posibles: Que cada tipo de unidad particular tambi�n despierte e inicialice atributos con valores espec�ficos suyos.
        protected virtual void Awake()
        {
            // Obtenemos el componente de salud y suscribimos nuestra autodestrucci�n al evento 'en caso de muerte'
            Health = GetComponent<Health>();
            if (Health == null)
                throw new Exception("No hay componente de salud.");
            Health.OnDeath += DestroySelf;
        }

        // Al colisionar se comprueba si se trata de un proyectil que no pertenece al controlador de esta instalaci�n, en cuyo caso se recibe da�o.
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

        // Devuelve el �ndice correspondiente a su controlador
        public int GetControllerIndex()
        {
            if (Controller == null)
                throw new ArgumentException("Esta instalaci�n no tiene controlador");

            return RTSGameManager.Instance.GetIndex(Controller);
        }

        // Activa la instalaci�n al comienzo de la ejecuci�n, asign�ndole su controlador correspondiente 
        public void Enable(RTSController controller)
        {
            if (controller == null)
                throw new ArgumentNullException("Se ha recibido un controlador nulo.");
            if (Enabled)
                throw new Exception("Esta instalaci�n ya estaba activada.");

            Controller = controller; 
            Enabled = true;
        }

        // La instalaci�n ha sido destruida (por un proyectil enemigo o por lo que sea)
        public void DestroySelf()
        {
            // Notifica al gestor del juego
            RTSGameManager.Instance.FacilityDestroyed(this);

            // Y finalmente se autodestruye
            Destroy(gameObject);
        }
    }
}