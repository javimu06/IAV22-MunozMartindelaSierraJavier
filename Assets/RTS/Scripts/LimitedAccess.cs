/*    
   Copyright (C) 2020 Federico Peinado
   http://www.federicopeinado.com

   Este fichero forma parte del material de la asignatura Inteligencia Artificial para Videojuegos.
   Esta asignatura se imparte en la Facultad de Inform�tica de la Universidad Complutense de Madrid (Espa�a).

   Autores originales: Opsive (Behavior Designer Samples)
   Revisi�n: Federico Peinado 
   Contacto: email@federicopeinado.com
*/
using System.Linq;
using UnityEngine;

namespace es.ucm.fdi.iav.rts
{
    /* 
     * El acceso limitado es un componente que se asocia a cualquier objeto que s�lo pueda ser usado por una unidad de extracci�n a la vez, como los recursos extra�bles o las intalaciones de procesamiento. 
     * Se requiere que la unidad tenga un Rigidbody (adem�s con la gravedad activada) para poder detectar cuando entra y sale del trigger del acceso limitado.
     * 
     * Posibles mejoras:
     * - Prueba a volver al OnTriggerEnter y OnTriggerExit como m�todo para detectar cuando se ocupan/desocupan los accesos limitados.
     * - Se podr�an optimizar las colisiones, colocando todo el escenario en una capa distinta de los controladores y sus unidades (e instalaciones), por ejemplo.
     * - Estudiar si es posible usar OnTriggerEnter y OnTriggerExit (con NavMeshAgent no hacen m�s que dispararse todo el rato...)
     * - Tener una forma de reiniciar al valor original de 'vac�o'.
     * - Mostrar en depuraci�n quien est� ocupando cada acceso limitado en cada momento; permitir que sea cualquier tipo de unidad.
     * - Ofrecer tambi�n un m�todo para ocupar el objeto con una unidad.
     * - Crear un componente para los recursos que permita que tengan un l�mite y se gasten (dejando de renderizarlos o destruy�ndolos para siempre)... por ahora s�lo les pedimos que tenga LimitedAccess
     * - Que los recursos tengan una posici�n de espera cerca, o alguna forma de repartir posiciones en sus alrededores, para las unidades extractoras que est�n esperando a que otra unidad acabe.
     * - Que las instalaciones de procesamiento tengan alguna forma de repartir posiciones en sus alrededores, para las unidades extractoras que est�n esperando a que otra unidad acabe.
     */
    public class LimitedAccess : MonoBehaviour
    {
        // La unidad de extracci�n que acapara actualmente el acceso a este objeto.
        public ExtractionUnit OccupiedBy { get; private set; } = null;

        // M�ximo n�mero de posiciones posibles que pueden intentarse usar mis futuros ocupantes.
        [SerializeField] private int _maxPositions = 10;
        public int MaxPositions { get { return _maxPositions; } }

        // Distancia a la que mis futuros ocupantes deben quedar de mi.
        // Debe ser siempre menor de la distancia de detecci�n de las unidades y del alcance que tengan los proyectiles disparados.
        [SerializeField] private float _radius = 10;
        public float Radius { get { return _radius; } }

        // Siguiente posici�n de los puntos cercanos (con desplazamiento) que tocar�a intentar utilizar.
        // Esto lo utiliza SetTargetOffset y ahora mismo en realidad lo que hace es contar la cantidad de veces que se piden posiciones cercanas...
        [SerializeField] private int _nextPosition = 0;
        public int NextPosition { get { return _nextPosition; } set { _nextPosition = value; } }

        // Escala de detecci�n para ocupar y desocupar este acceso limitado.
        // Ahora mismo estoy ignorando el valor introducido
        [SerializeField] private Vector3 _detectionScale;
        public Vector3 DetectionScale { get { return _detectionScale; } private set { _detectionScale = value; } }

        // Posici�n de detecci�n para ocupar y desocupar este acceso limitado.
        private Vector3 DetectionPosition { get; set; }

        // Rotaci�n de detecci�n para ocupar y desocupar este acceso limitado.
        private Quaternion DetectionRotation { get; set; }

        // Instalaci�n de procesamiento (por si lo es)
        private ProcessingFacility ProcessingFacility { get; set; }

        // Despierta la unidad y se asegura de que tiene los componentes de salud y comportamiento preparados
        // Mejoras posibles: Que cada tipo de unidad particular tambi�n despierte e inicialice atributos con valores espec�ficos suyos
        protected void Awake()
        {
            ProcessingFacility = GetComponent<ProcessingFacility>();

            // S�lo lo calculo una vez porque asumo que los accesos limitados son est�ticos
            if (ProcessingFacility == null)
                DetectionPosition = transform.position + new Vector3(0f, 2.5f, 0f);
            else
                DetectionPosition = ProcessingFacility.UnloadingTransform.position + new Vector3(0f, 2f, 0f);
            DetectionScale = transform.localScale + new Vector3(10f, 2f, 10f);
            DetectionRotation = transform.rotation;
        }

        // Actualizar comprobando si alguna unidad de extracci�n colisiona con este trigger (y venga vac�a, si es zona de extracci�n, o llena, si es de descarga), y la ocupa.... o si lo abandona.
        // Esto implica que no necesito ni RigidBodys ni colliders en los accesos limitados, s�lo collider en las unidades.
        private void Update()
        {
            // C�digo que pod�a ser est�tico para comprobar si est� libre el entorno del acceso limitado  
            // Importante indicar que colisiones con los triggers, porque los collider de las unidades est�n marcados como tales
            // Entiendo que la LayerMask est� a la negaci�n de 0 (~0) y eso permite capturar colisiones en todas las capas (podr�a restringirse)
            var hitColliders = Physics.OverlapBox(DetectionPosition, DetectionScale, DetectionRotation, ~0, QueryTriggerInteraction.Collide); // Podr�a usar otros valores algo m�s grandes o dependiente de si esto es una instalaci�n o un recurso extra�ble...
            // Habr�a que intentar evitar que entre los colliders se cuente el de ESTE PROPIO acceso limitado... con alg�n Physics.IgnoreCollision o as� 

            // Si el acceso limitado est� ocupado, se comprueba que el collider del ocupante sigue teniendo sentido 
            // (tiene recursos que descargar, si estamos en una instalaci�n de procesamiento... o no los tiene, si es una zona de recursos extra�bles)
            // y que efectivamente est� f�sicamente ah�; y si no cumple algo de esto, el acceso limitado se libera.
            // Esto se hace as� para minimizar los bloqueos...
            if (OccupiedBy != null)
            {
                var occupiedCollider = OccupiedBy.GetComponent<Collider>();
                if (occupiedCollider == null)
                    throw new System.Exception("El acceso limitado tiene un ocupante sin collider.");

                if ((ProcessingFacility == null && OccupiedBy.Resources > 0) ||
                    (ProcessingFacility != null && OccupiedBy.Resources == 0) || 
                    !hitColliders.Contains(occupiedCollider))
                    OccupiedBy = null;
            }

            // En el mismo fotograma, si se queda desocupado se mira si otro puede ocupar el acceso... no llegando a estar nunca desocupado
             
            // Si el acceso limitado est� libre, se ocupa con la primera unidad v�lida que est� f�sicamente colisionando aqu�, si la hay y tiene sentido
            // (tiene recursos que descargar, si estamos en una instalaci�n de procesamiento... o no los tiene, si es una zona de recursos extra�bles) 
            // Esto se hace as� para minimizar los bloqueos...
            if (OccupiedBy == null)
            {
                foreach (var collider in hitColliders)
                {
                    var extractionUnit = collider.gameObject.GetComponent<ExtractionUnit>();
                    // Si se trata de una unidad de extracci�n, al ser la primera encontrada, ocupa el acceso limitado
                    if (extractionUnit != null &&
                        ((ProcessingFacility == null && extractionUnit.Resources == 0) ||
                        (ProcessingFacility != null && extractionUnit.Resources > 0)))
                    {
                        OccupiedBy = extractionUnit;
                        break;
                    }
                }
            }
        }


        // Para depuraci�n, se dibuja el Box Overlap como un gizmo para mostrar donde se est� mirando la colisi�n.
        // Activa la opci�n de 'Gizmos' en el editor para verlo.
        void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            // Podr�a comprobar si est� siendo ejecuta en Play Mode, para no intentar dibujar esto en Editor Mode
            // Dibuja un cubo justo donde est� la OverlapBox (misma posici�n, misma escala). La rotaci�n no parece usarse
            Gizmos.DrawWireCube(DetectionPosition, DetectionScale);
        }
    

        /* SEGURAMENTE UNA VEZ ACLARADO LO DE IS KINEMATIC YA PODR�A FUNCIONAR EL ONTRIGGERENTER Y ONTRIGGEREXIT CON NORMALIDAD

                // Con que una unidad de extracci�n colisione con este trigger (y venga vac�a, si es zona de extracci�n, o llena, si es de descarga), ya se asume que la ocupa.
                // Para que funcione OnTriggerEnter hace falta tener que uno de los dos tenga marcada la casilla OnTrigger.   
                public void OnTriggerEnter(Collider other)
                {
                    ProcessingFacility facility = this.GetComponent<ProcessingFacility>();
                    ExtractionUnit unit = other.GetComponent<ExtractionUnit>();
                    if (OccupiedBy == null && ((facility == null && unit.Resources == 0) || (facility != null && unit.Resources > 0)))
                    {
                        Debug.Log("Ocupando " + this.name + " con la unidad " + unit.name);
                        OccupiedBy = unit;
                    }
                }

                // Con que la unidad que ocupa este acceso salga fuera del trigger del mismo, ya se asume que vuelve a estar desocupado.
                // Para que funcione OnTriggerEnter hace falta tener que uno de los dos tenga marcada la casilla OnTrigger.
                // Posibles mejoras: A veces se comporta RARO y este m�todo se llama cuando se ENTRA en el trigger, o con el movimiento cinem�tico o algo as�...  
                public void OnTriggerExit(Collider other)
                {
                    ProcessingFacility facility = this.GetComponent<ProcessingFacility>();
                    ExtractionUnit unit = other.GetComponent<ExtractionUnit>();
                    if (OccupiedBy == unit) // && ((facility == null && unit.Resources > 0) || (facility != null && unit.Resources == 0))) 
                    { 
                        Debug.Log("Desocupando " + this.name + " de la unidad " + unit.name);
                        OccupiedBy = null;
                    }
                }
        */

    }
}