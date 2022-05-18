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
     * Los poblados son edificios supuestamente neutrales, que los controladores podrán destruir si lo desean (para tener más espacio de navegación, por ejemplo).
     * No se mueven ni disparan proyectiles, pero pueden sufrir daños e incluso ser destruidas por el enemigo o las amenazas del escenario. 
     * Si se añade este componente a un objeto, será necesario añadir también un componente de salud.
    * 
    * Posibles mejoras:
    * - Controlar que no ocurra que una torreta ataca sin querer a un poblado y lo hace daño 
    * - Que los poblados una vez destruidos puedan regenerarse y volver a aparecer (los habría de dos tipos, unos que se regeneran y otros que no)
    */
    [RequireComponent(typeof(Health))]
    public class Village : MonoBehaviour
    {
        // Máximo número de posiciones posibles que pueden intentarse ocupar mis atacantes.
        [SerializeField] private int _maxPositions = 20;
        public int MaxPositions { get { return _maxPositions; } }

        // Distancia a la que mis atancantes deben quedar de mi.
        // Debe ser siempre menor de la distancia de detección de las unidades y del alcance que tengan los proyectiles disparados.
        [SerializeField] private float _radius = 12;
        public float Radius { get { return _radius; } }

        // Siguiente posición de los puntos cercanos (con desplazamiento) que tocaría intentar utilizar.
        // Esto lo utiliza SetTargetOffset y ahora mismo en realidad lo que hace es contar la cantidad de veces que se piden posiciones cercanas...
        [SerializeField] private int _nextPosition = 0;
        public int NextPosition { get { return _nextPosition; } set { _nextPosition = value; } }

        // Salud del poblado
        protected Health Health { get; set; }

        /**********************************************************/

        // Cuantos me están atacando ahora
        private int AttackerUnits { get; set; } = 0;

        // Despierta al poblado y se asegura de que tiene los componentes de salud y comportamiento preparados
        // Mejoras posibles: Que cada tipo de unidad particular también despierte e inicialice atributos con valores específicos suyos
        protected void Awake()
        {
            // Obtenemos el componente de salud y suscribimos nuestra autodestrucción al evento 'en caso de muerte'
            Health = GetComponent<Health>();
            if (Health == null)
                throw new Exception("No hay componente de salud.");
            Health.OnDeath += DestroySelf;
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
        }*/

        // Al colisionar se comprueba si se trata de un proyectil (sin importar el controlador), en cuyo caso se recibe daño 
        // Tanto el proyectil como el objetivo son rigidbodys con un collider en modo trigger, por eso funciona este método privado.
        // Mejoras posibles: Contabilizar quien ha abatido más poblados, para cada bando.
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

        // El poblado ha sido destruida (por un proyectil enemigo o por lo que sea)
        // Posible mejora: Usar behavior.DisableBehavior(); por si da problemas el destruir un objeto que tiene un árbol de comportamiento funcionando
        public void DestroySelf()
        {
            // Notifica al gestor del juego
            RTSScenarioManager.Instance.VillageDestroyed(this);

            // Y finalmente se autodestruye
            Destroy(gameObject);
        }
    }
}