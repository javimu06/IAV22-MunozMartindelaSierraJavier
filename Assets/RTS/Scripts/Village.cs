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
     * Los poblados son edificios supuestamente neutrales, que los controladores podr�n destruir si lo desean (para tener m�s espacio de navegaci�n, por ejemplo).
     * No se mueven ni disparan proyectiles, pero pueden sufrir da�os e incluso ser destruidas por el enemigo o las amenazas del escenario. 
     * Si se a�ade este componente a un objeto, ser� necesario a�adir tambi�n un componente de salud.
    * 
    * Posibles mejoras:
    * - Controlar que no ocurra que una torreta ataca sin querer a un poblado y lo hace da�o 
    * - Que los poblados una vez destruidos puedan regenerarse y volver a aparecer (los habr�a de dos tipos, unos que se regeneran y otros que no)
    */
    [RequireComponent(typeof(Health))]
    public class Village : MonoBehaviour
    {
        // M�ximo n�mero de posiciones posibles que pueden intentarse ocupar mis atacantes.
        [SerializeField] private int _maxPositions = 20;
        public int MaxPositions { get { return _maxPositions; } }

        // Distancia a la que mis atancantes deben quedar de mi.
        // Debe ser siempre menor de la distancia de detecci�n de las unidades y del alcance que tengan los proyectiles disparados.
        [SerializeField] private float _radius = 12;
        public float Radius { get { return _radius; } }

        // Siguiente posici�n de los puntos cercanos (con desplazamiento) que tocar�a intentar utilizar.
        // Esto lo utiliza SetTargetOffset y ahora mismo en realidad lo que hace es contar la cantidad de veces que se piden posiciones cercanas...
        [SerializeField] private int _nextPosition = 0;
        public int NextPosition { get { return _nextPosition; } set { _nextPosition = value; } }

        // Salud del poblado
        protected Health Health { get; set; }

        /**********************************************************/

        // Cuantos me est�n atacando ahora
        private int AttackerUnits { get; set; } = 0;

        // Despierta al poblado y se asegura de que tiene los componentes de salud y comportamiento preparados
        // Mejoras posibles: Que cada tipo de unidad particular tambi�n despierte e inicialice atributos con valores espec�ficos suyos
        protected void Awake()
        {
            // Obtenemos el componente de salud y suscribimos nuestra autodestrucci�n al evento 'en caso de muerte'
            Health = GetComponent<Health>();
            if (Health == null)
                throw new Exception("No hay componente de salud.");
            Health.OnDeath += DestroySelf;
        }

        /*
        // Devuelve la siguiente posici�n alrededor suyo, para ayudar a orientarse a las unidades que la vayan a atacar. 
        // La idea es que estas posiciones se alineen en c�rculo alrededor del edificio.
        // Mejoras posibles: 
        // - Podr�a devolver una transformada (Transform), aunque la rotaci�n tampoco sea muy relevante, es cosa de mirar hacia el objetivo, nada m�s.
        // - Se deber�a ir comprobando los huecos (como las instalaciones base al generar unidades) para descontar el n�mero de atacantes seg�n van apareciendo esos huecos
        public Vector3 NextAttackTransform()
        {
            // Usa la ecuaci�n param�trica del c�rculo para determinar la siguiente posici�n de ataque
            var position = transform.position;
            position.x += AttackerRadius * Mathf.Sin((AttackerSpread * AttackerUnits + AttackerSpreadOffset) * Mathf.Deg2Rad);
            position.z += AttackerRadius * Mathf.Cos((AttackerSpread * AttackerUnits + AttackerSpreadOffset) * Mathf.Deg2Rad);
            AttackerUnits++; // Para ir contando cuantos atacantes tiene acumulados a su alrededor...
            return position;
        }*/

        // Al colisionar se comprueba si se trata de un proyectil (sin importar el controlador), en cuyo caso se recibe da�o 
        // Tanto el proyectil como el objetivo son rigidbodys con un collider en modo trigger, por eso funciona este m�todo privado.
        // Mejoras posibles: Contabilizar quien ha abatido m�s poblados, para cada bando.
        private void OnTriggerEnter(UnityEngine.Collider trigger)
        {
            Projectile projectile = trigger.gameObject.GetComponent<Projectile>();
            if (projectile != null) // Todos los proyectiles lo da�an
            {
                // Recibir da�o. La salud lanzar� un evento si el da�o (que puede seguir recibiendo) es igual a 0
                Health.TakeDamage(projectile.DamageAmount);
                // Destruye el proyectil
                projectile.DestroySelf();
            }
        }

        // El poblado ha sido destruida (por un proyectil enemigo o por lo que sea)
        // Posible mejora: Usar behavior.DisableBehavior(); por si da problemas el destruir un objeto que tiene un �rbol de comportamiento funcionando
        public void DestroySelf()
        {
            // Notifica al gestor del juego
            RTSScenarioManager.Instance.VillageDestroyed(this);

            // Y finalmente se autodestruye
            Destroy(gameObject);
        }
    }
}