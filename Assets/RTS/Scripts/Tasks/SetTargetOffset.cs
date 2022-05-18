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
using BehaviorDesigner.Runtime.Tasks;
using Tooltip = BehaviorDesigner.Runtime.Tasks.TooltipAttribute;
using UnityEngine.UI;

namespace es.ucm.fdi.iav.rts
{
    /* 
     * Establece el desplazamiento relativo a la posición del objetivo que debemos usar para navegar mejor hasta él, en una variable compartida.
     * Sirve para poner a esperar unidades de extracción en las cercanías de las zonas de recursos extraíbles o de las instalaciones de procesamiento que están ocupadas,
     * también para distribuir mejor las unidades de combate alrededor de un objetivo al que están atacando, y en general para no tener que moverse necesariamente hasta quedar pegado al objetivo (sea unidad, instalación, poblado o torreta).
     * Posibles mejoras:
     * - Podría distinguir la manera en que se posiciona una unidad exploradora y una destructora, por ejemplo.
     * - Añadir también una rotación deseada, para por ejemplo quedarse apuntando correctamente al objetivo al que vas a disparar, y no tener que hacer una rotación brusca.
     * - Hacer que por encima de los valores por defecto primen los valores que recomiende el objetivo para MaxPositions y Radius.
     * - Comprobar que efectivamente si estoy esperando lejos del objetivo y se libera algún punto cercano, este comportamiento me manda allí, para aprovechar el hueco. Pasar de un punto de espera a otro no es recomendable, ya que son todos equidistantes del objetivo.
     */
    [TaskCategory("RTS")]
    [TaskDescription("Establece el desplazamiento relativo a la posición del objetivo que debemos usar para navegar mejor hasta él, en una variable compartida.")]
    public class SetTargetOffset : Action 
    {
        // El controlador de la unidad que ejecuta este comportamiento.
        [Tooltip("El controlador de la unidad que ejecuta este comportamiento.")]
        [SerializeField] private SharedRTSController _rtsController;
        public SharedRTSController RTSController { get { return _rtsController; } private set { _rtsController = value; } }

        // La transformada del objetivo de navegación.
        [Tooltip("La transformada del objetivo de navegación.")]
        [SerializeField] private SharedTransform _navigationTarget;
        public SharedTransform NavigationTarget { get { return _navigationTarget; } private set { _navigationTarget = value; } }

        // El desplazamiento aplicable sobre el objetivo de navegación.
        [Tooltip("El desplazamiento aplicable sobre el objetivo de navegación.")]
        [SerializeField] private SharedVector3 _targetOffset = null;
        public SharedVector3 TargetOffset { get { return _targetOffset; } }

        // Máximo número de posiciones posibles que llegan a intentarse ocupar (valor por defecto).
        [SerializeField] private int _maxPositions = 10;
        public int MaxPositions { get { return _maxPositions; } private set { _maxPositions = value; } }

        // Distancia del objetivo a la que debe quedar la unidad (valor por defecto).
        // Debe ser siempre menor de la distancia de detección de las unidades y del alcance que tengan los proyectiles disparados.
        [SerializeField] private float _radius = 15;
        public float Radius { get { return _radius; } private set { _radius = value; } }

        /**************************************************************/

        // La propia unidad donde se ejecuta el comportamiento, si se trata de una unidad de extracción.
        private ExtractionUnit ExtractionUnit { get; set; }

        // Devuelve éxito inmediatamente tras establecer el desplazamiento relativo a la posición del objetivo que debemos usar para navegar mejor hasta él; salvo que no haya sitio libre, en cuyo caso devuelve fracaso.
        // Ahora mismo se generan posiciones alineadas en torno a la longitud de un círculo con el objetivo como centro.
        // Mejoras posibles: 
        // - Podría devolverse una transformada, por aquello de añadir la rotación (ej. poder mirar hacia el objetivo), en cuyo caso sería una transformada que SUMAR a la normal del objetivo. 
        // - Revisar cómo evitar conflictos derivados del hecho de que aunque una posición ahora está libre, lo mismo ya ha sido asignada y otra unidad está viniendo para allá
        //   (se podría generar un SpreadOffset aleatorio cada vez o algo así, para reducir la cantidad de posiciones que se asignan a VARIAS unidades)
        public override TaskStatus OnUpdate()
        {
            if (RTSController.Value == null)
                throw new System.Exception("No hay controlador asociado.");
            // Devolvemos fracaso, porque ha desaparecido el objetivo de navegación.
            if (NavigationTarget.Value == null)
                return TaskStatus.Failure;

            int MyIndex = RTSGameManager.Instance.GetIndex(RTSController.Value);

            // Si el objetivo es un objeto reconocible, se le podría pedir su MaxPositions, Radius... y hasta el número de posición cercana por el que toca seguir repartiendo 
            // (incluso aunque sea una duna/roca sería posible ponerse alrededor) 
            Unit unit = NavigationTarget.Value.GetComponent<Unit>();
            Facility facility = NavigationTarget.Value.GetComponent<Facility>();
            ProcessingFacility processingFacility = NavigationTarget.Value.GetComponent<ProcessingFacility>();
            if (NavigationTarget.Value.GetComponentInParent<ProcessingFacility>() != null)
            { // Por si me pasan el UnloadingTransform, que es lo habitual
                facility = NavigationTarget.Value.GetComponentInParent<Facility>();
                processingFacility = NavigationTarget.Value.GetComponentInParent<ProcessingFacility>();
            }
            Tower tower = NavigationTarget.Value.GetComponent<Tower>();
            Village village = NavigationTarget.Value.GetComponent<Village>();
            LimitedAccess access = NavigationTarget.Value.GetComponent<LimitedAccess>();

            int positionCount = 0;
            bool calculateOffset = false;
            // Lo de NextPosition en realidad es una optimización muy poco importante... lo importante es saber si tiene sentido o no calcular el offset

            // Inicialmente el desplazamiento es 0
            var offsetPosition = new Vector3(0.0f, 0.0f, 0.0f);

            // Por si soy una unidad de extracción
            ExtractionUnit = GetComponent<ExtractionUnit>();

            // Si el objetivo es una UNIDAD, da igual que sea mía o de mi enemigo, me dirijo a su alrededor (con la idea de parar pronto, pues son móviles)
            if (unit != null && unit.GetControllerIndex() != MyIndex)
            {
                MaxPositions = unit.MaxPositions;
                Radius = unit.Radius;
                positionCount = unit.NextPosition;
                unit.NextPosition++;
                calculateOffset = true;
            }
            // Si el objetivo es una instalación, da igual que sea mía o de mi enemigo, me dirijo a su alrededor para no colapsar
            if (facility != null)  
            {
                // Si además soy unidad de extracción con recursos, y la instalación es de procesamiento y de las mías, ajusto el desplazamiento hacia SU punto de descarga
                if (ExtractionUnit != null && ExtractionUnit.Resources > 0 &&
                    processingFacility != null && processingFacility.GetControllerIndex() == ExtractionUnit.GetControllerIndex())
                {
                    offsetPosition = processingFacility.UnloadingTransform.position - processingFacility.transform.position;
                }
                MaxPositions = facility.MaxPositions;
                Radius = facility.Radius;
                positionCount = facility.NextPosition;
                facility.NextPosition++;
                calculateOffset = true;
            }
            // Si el objetivo es un poblado, y no soy unidad de extracción, me dirijo a su alrededor
            if (village != null && ExtractionUnit == null)
            {
                MaxPositions = village.MaxPositions;
                Radius = village.Radius;
                positionCount = village.NextPosition;
                village.NextPosition++;
                calculateOffset = true;
            }
            // Si el objetivo es una torre, da igual quien sea yo, que me dirijo a su alrededor (no me acerco más)
            if (tower != null)
            {
                MaxPositions = tower.MaxPositions;
                Radius = tower.Radius;
                positionCount = tower.NextPosition;
                tower.NextPosition++;
                calculateOffset = true;
            }
            // Si el objetivo es un acceso limitado (entendemos, una zona de recursos extraíbles) y soy una unidad de extracción, y voy vacía, me dirijo a su alrededor
            if (access != null && ExtractionUnit != null && ExtractionUnit.Resources == 0)
            {
                MaxPositions = access.MaxPositions;
                Radius = access.Radius;
                // Podría mover un poco el offsetPosition, no haría falta que fuese justo en el centro
                positionCount = access.NextPosition;
                access.NextPosition++;
                calculateOffset = true;
            }
            // En caso contrario, no vamos a calcular ningún Offset, dejamos el que ya esté
            if (!calculateOffset) {
                // TargetOffset.Value = offsetPosition; ¿No sería lo correcto mandar este de todo ceros?
                return TaskStatus.Success;
            }

            var Spread = 360 / MaxPositions;

            bool validPosition = false;
            float bestDistance = float.MaxValue; // Empezamos con el valor peor posible 
            // Realizamos todos los intentos posibles, para intentar dar con una posición válida
            // (empezamos desde la recomendada... pero nos quedamos con la que esté más cerca de la unidad que busca objetivo)
            for (int count = 0; count < MaxPositions; count++ )
            {
                int position = (positionCount + count) % MaxPositions;

                Vector3 newOffsetPosition;
                // Usa la ecuación paramétrica del círculo para determinar la siguiente posición.
                // Parte de la transformada original del objetivo (bueno, un poco más alta)
                newOffsetPosition.x = offsetPosition.x + Radius * Mathf.Sin((Spread * position) * Mathf.Deg2Rad);
                newOffsetPosition.y = offsetPosition.y + 1.5f;
                newOffsetPosition.z = offsetPosition.z + Radius * Mathf.Cos((Spread * position) * Mathf.Deg2Rad);

                // Código que podía ser estático para comprobar si está libre la posición anterior.
                var hitColliders = Physics.OverlapSphere(NavigationTarget.Value.position + newOffsetPosition, 1f); // Podría usar otros valores algo más grandes en vez de subir 1.5 y hacer esfera de radio 1

                // No hay ningún collider allí, está libre... o como mucho estoy yo
                // Posible mejora: Ignorar también todos los colliders de proyectiles
                if (hitColliders.Length == 0 || (hitColliders.Length == 1 && hitColliders[0].transform == transform)) 
                {
                    float nearbyDistance = Vector3.Distance(transform.position, NavigationTarget.Value.position + newOffsetPosition);
                    if (nearbyDistance < bestDistance)
                    {
                        // Machaco el valor que hubiera podido haber antes
                        TargetOffset.Value = newOffsetPosition;
                        // Me guardo la mejor distancia y apunto que he encontrado una posición valida
                        bestDistance = nearbyDistance;
                        validPosition = true;
                    }
                }
            }
            if (validPosition)
                return TaskStatus.Success;

            // Si no ha encontrado ninguna posición libre, se fracasa (tal vez se podría devolver offset cero y listo...)
            return TaskStatus.Failure;
        }
    }
}