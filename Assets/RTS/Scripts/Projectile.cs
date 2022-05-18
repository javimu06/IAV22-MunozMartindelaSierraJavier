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
     * El proyectil puede ser disparado por torretas y algunas unidades, se mueve hacia su objetivo y si le impacta, le resta salud.
     * 
     * Posibles mejoras: 
     * - Ten prefabs distintos para cada tipo de proyectil (de colores para cada bando, e incluso más grande para el que lanza la unidad de destrucción)
     * - Importante confirmar que los misiles no colisionan ni interfieren entre sí.
     * - Establecer un tiempo máximo de duración de vuelo o una distancia máxima, para que no pueda recorrer todo el escenario.
     * - Hacer distintos tamaños o colores según lo dispare una unidad exploradora o una destructora, por ejemplo.
     * - Si se añade este componente a un objeto, no necesita Rigidbody con IsKinematic activado, sólo collision y que no sea trigger... para poder detectar si entra o abandona el trigger con su objetivo.
     */
    //[RequireComponent(typeof(Rigidbody))]
    public class Projectile : MonoBehaviour
    {
        // La cantidad de daño que causará el proyectil al objeto con el que colisione, que se establece al activarlo
        // En realidad ahora mismo es modificado por el método Enable
        [SerializeField] private int _damageAmount = 1; // Valor por defecto que será sobreescrito
        public int DamageAmount { get { return _damageAmount; } private set { _damageAmount = value; } }

        // La velocidad del proyectil (diferente de lo que será luego la cadencia de fuego)
        [SerializeField] private float _speed = 1; // Valor por defecto que será sobreescrito
        public float Speed { get { return _speed; } }

        // El rango (alcance) que es capaz de recorrer el proyectil antes de desaparecer.
        // Hay que asegurarse que siempre es algo mayor de la distancia de detección del atacante, para que tenga sentido disparar.
        // Hacerlo privado
        [SerializeField] private float _range = 10; // Valor por defecto que será sobreescrito
        public float Range { get { return _range; } private set { _range = value; } }
         
        // El objetivo del proyectil, que se establece al activarlo, no me lo guardo porque no me hace falta
        //[SerializeField] private Transform _targetTransform = null;
        //public Transform TargetTransform { get { return _targetTransform; } private set { _targetTransform = value; } }

        /****************************************************************/

        // El controlador de este proyectil
        // OJO: Puede ser nulo si se trata de un proyectil enviado por una torreta
        private RTSController Controller { get; set; }

        // El indicador de activación, para evitar que el proyectil se active más de una vez
        private bool Enabled { get; set; }

        // La cantidad de alcace ya recorrido.
        private float TravelledRange { get; set; } = 0;

        // El vector velocidad normalizado. El proyectil se mueve por tanto siempre en línea recta.
        private Vector3 NormalizedDirection { get; set; }

        // Despierta el proyectil y lo orienta hacia su objetivo
        // Se usa la transformada del objeto al que pertenece este componente directamente
        private void Awake()
        {
            if (transform == null)
                throw new Exception("El componente no está aplicado sobre un Game Object."); // Entiendo que es raro que ocurra         
        }

        // El proyectil permanece moviéndose indefinidamente hacia su objetivo... hasta que deje de haberlo
        // Posible mejora: Que cuando desaparezca su objetivo continue su camino, pudiendo impactar con otro enemigo
        private void Update()
        {
            // No compruebo si hay RTSController porque cuando disparan las torretas, no los llevan
            if (transform == null)
                throw new Exception("El componente no está aplicado sobre un Game Object."); // Entiendo que es raro que ocurra
            // Quejarme si no hay distancia, no hay cantidad de daño...

            // Puede pasar perfectamente que no tengas objetivo establecido porque este haya desaparecido 

            // A lo mejor sería suficiente buscar una fórmula que haga avanzar al proyectil una cierta distancia pero sin tener de referencia a su objetivo (ya le hemos orientado antes)
            // transform.position += transform.forward * Speed * Time.deltaTime;
            //transform.position = Vector3.MoveTowards(transform.position, TargetTransform.position, Speed * Time.deltaTime);
            transform.position+= NormalizedDirection * Speed * Time.deltaTime;
           
            TravelledRange += Speed * Time.deltaTime;

            // Si ya he recorrido todo lo que da mi alcance, me autodestruyo 
            if (TravelledRange > Range)
                DestroySelf();
        }

        // Devuelve el índice de su controlador correspondiente. 
        // OJO: Si el proyectil lo ha disparado una torreta, no habrá controlador.
        public int GetControllerIndex()
        {
            if (Controller == null)
                return -1; // Un valor que creo que no corresponde a ningún índice, porque no tengo controlador
            return RTSGameManager.Instance.GetIndex(Controller);
        }

        // Activa la unidad al comienzo de la ejecución, asignándole su controlador, objetivo y daño correspondiente.
        // Saltará una excepción hasta de si hacemos un daño cero o negativo.
        // Posibles mejoras:
        // - Podría pedirse el rango como parámetro, aunque ahora mismo usamos el del propio proyectil.
        // - Si es una torreta, estaría bien tener un método que no requiere controlador (o pasar sólo el índice).
        public void Enable(RTSController controller, Transform targetTransform, int damageAmount)
        {
            // No compruebo si hay RTSController porque cuando disparan las torretas, no los llevan
            if (targetTransform == null)
                throw new ArgumentNullException("Se ha recibido un objetivo nulo.");
            if (damageAmount <= 0)
                throw new ArgumentNullException("Se ha recibido un daño negativo o nulo."); 
            if (Enabled)
                throw new Exception("Este proyectil ya estaba activado.");

            Controller = controller; // Puede ser nulo, si ha disparado una torreta 

            NormalizedDirection = (targetTransform.position - transform.position).normalized;

            DamageAmount = damageAmount; 
            Enabled = true;
        }

        // Autodestruye por completo el proyectil. Este método debe ser llamado por el objetivo con el que ha colisionado el proyectil (u otro enemigo que se ha interpuesto)
        // También se lo llama el proyectil a sí mismo cuando lleva demasiado tiempo volando.
        // Posibles mejoras: 
        // - No veo necesario desactivar el objeto ni nada así
        // - Si es necesario, tener un OnDisable donde se haga targetHealth.OnDeath -= destroySelf (si es que recibimos notificaciones del objetivo)
        public void DestroySelf()
        { 
            Destroy(gameObject);
        } 
    }
}