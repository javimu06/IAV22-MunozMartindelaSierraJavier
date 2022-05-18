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
using BehaviorDesigner.Runtime.Tasks;
using Tooltip = BehaviorDesigner.Runtime.Tasks.TooltipAttribute;
using UnityEngine.UI;

namespace es.ucm.fdi.iav.rts
{
    /* 
     * Establece el desplazamiento relativo a la posici�n del objetivo que debemos usar para navegar mejor hasta �l, en una variable compartida.
     * Sirve para poner a esperar unidades de extracci�n en las cercan�as de las zonas de recursos extra�bles o de las instalaciones de procesamiento que est�n ocupadas,
     * tambi�n para distribuir mejor las unidades de combate alrededor de un objetivo al que est�n atacando, y en general para no tener que moverse necesariamente hasta quedar pegado al objetivo (sea unidad, instalaci�n, poblado o torreta).
     * Posibles mejoras:
     * - Podr�a distinguir la manera en que se posiciona una unidad exploradora y una destructora, por ejemplo.
     * - A�adir tambi�n una rotaci�n deseada, para por ejemplo quedarse apuntando correctamente al objetivo al que vas a disparar, y no tener que hacer una rotaci�n brusca.
     * - Hacer que por encima de los valores por defecto primen los valores que recomiende el objetivo para MaxPositions y Radius.
     * - Comprobar que efectivamente si estoy esperando lejos del objetivo y se libera alg�n punto cercano, este comportamiento me manda all�, para aprovechar el hueco. Pasar de un punto de espera a otro no es recomendable, ya que son todos equidistantes del objetivo.
     */
    [TaskCategory("RTS")]
    [TaskDescription("Establece el desplazamiento relativo a la posici�n del objetivo que debemos usar para navegar mejor hasta �l, en una variable compartida.")]
    public class SetTargetOffset : Action 
    {
        // El controlador de la unidad que ejecuta este comportamiento.
        [Tooltip("El controlador de la unidad que ejecuta este comportamiento.")]
        [SerializeField] private SharedRTSController _rtsController;
        public SharedRTSController RTSController { get { return _rtsController; } private set { _rtsController = value; } }

        // La transformada del objetivo de navegaci�n.
        [Tooltip("La transformada del objetivo de navegaci�n.")]
        [SerializeField] private SharedTransform _navigationTarget;
        public SharedTransform NavigationTarget { get { return _navigationTarget; } private set { _navigationTarget = value; } }

        // El desplazamiento aplicable sobre el objetivo de navegaci�n.
        [Tooltip("El desplazamiento aplicable sobre el objetivo de navegaci�n.")]
        [SerializeField] private SharedVector3 _targetOffset = null;
        public SharedVector3 TargetOffset { get { return _targetOffset; } }

        // M�ximo n�mero de posiciones posibles que llegan a intentarse ocupar (valor por defecto).
        [SerializeField] private int _maxPositions = 10;
        public int MaxPositions { get { return _maxPositions; } private set { _maxPositions = value; } }

        // Distancia del objetivo a la que debe quedar la unidad (valor por defecto).
        // Debe ser siempre menor de la distancia de detecci�n de las unidades y del alcance que tengan los proyectiles disparados.
        [SerializeField] private float _radius = 15;
        public float Radius { get { return _radius; } private set { _radius = value; } }

        /**************************************************************/

        // La propia unidad donde se ejecuta el comportamiento, si se trata de una unidad de extracci�n.
        private ExtractionUnit ExtractionUnit { get; set; }

        // Devuelve �xito inmediatamente tras establecer el desplazamiento relativo a la posici�n del objetivo que debemos usar para navegar mejor hasta �l; salvo que no haya sitio libre, en cuyo caso devuelve fracaso.
        // Ahora mismo se generan posiciones alineadas en torno a la longitud de un c�rculo con el objetivo como centro.
        // Mejoras posibles: 
        // - Podr�a devolverse una transformada, por aquello de a�adir la rotaci�n (ej. poder mirar hacia el objetivo), en cuyo caso ser�a una transformada que SUMAR a la normal del objetivo. 
        // - Revisar c�mo evitar conflictos derivados del hecho de que aunque una posici�n ahora est� libre, lo mismo ya ha sido asignada y otra unidad est� viniendo para all�
        //   (se podr�a generar un SpreadOffset aleatorio cada vez o algo as�, para reducir la cantidad de posiciones que se asignan a VARIAS unidades)
        public override TaskStatus OnUpdate()
        {
            if (RTSController.Value == null)
                throw new System.Exception("No hay controlador asociado.");
            // Devolvemos fracaso, porque ha desaparecido el objetivo de navegaci�n.
            if (NavigationTarget.Value == null)
                return TaskStatus.Failure;

            int MyIndex = RTSGameManager.Instance.GetIndex(RTSController.Value);

            // Si el objetivo es un objeto reconocible, se le podr�a pedir su MaxPositions, Radius... y hasta el n�mero de posici�n cercana por el que toca seguir repartiendo 
            // (incluso aunque sea una duna/roca ser�a posible ponerse alrededor) 
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
            // Lo de NextPosition en realidad es una optimizaci�n muy poco importante... lo importante es saber si tiene sentido o no calcular el offset

            // Inicialmente el desplazamiento es 0
            var offsetPosition = new Vector3(0.0f, 0.0f, 0.0f);

            // Por si soy una unidad de extracci�n
            ExtractionUnit = GetComponent<ExtractionUnit>();

            // Si el objetivo es una UNIDAD, da igual que sea m�a o de mi enemigo, me dirijo a su alrededor (con la idea de parar pronto, pues son m�viles)
            if (unit != null && unit.GetControllerIndex() != MyIndex)
            {
                MaxPositions = unit.MaxPositions;
                Radius = unit.Radius;
                positionCount = unit.NextPosition;
                unit.NextPosition++;
                calculateOffset = true;
            }
            // Si el objetivo es una instalaci�n, da igual que sea m�a o de mi enemigo, me dirijo a su alrededor para no colapsar
            if (facility != null)  
            {
                // Si adem�s soy unidad de extracci�n con recursos, y la instalaci�n es de procesamiento y de las m�as, ajusto el desplazamiento hacia SU punto de descarga
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
            // Si el objetivo es un poblado, y no soy unidad de extracci�n, me dirijo a su alrededor
            if (village != null && ExtractionUnit == null)
            {
                MaxPositions = village.MaxPositions;
                Radius = village.Radius;
                positionCount = village.NextPosition;
                village.NextPosition++;
                calculateOffset = true;
            }
            // Si el objetivo es una torre, da igual quien sea yo, que me dirijo a su alrededor (no me acerco m�s)
            if (tower != null)
            {
                MaxPositions = tower.MaxPositions;
                Radius = tower.Radius;
                positionCount = tower.NextPosition;
                tower.NextPosition++;
                calculateOffset = true;
            }
            // Si el objetivo es un acceso limitado (entendemos, una zona de recursos extra�bles) y soy una unidad de extracci�n, y voy vac�a, me dirijo a su alrededor
            if (access != null && ExtractionUnit != null && ExtractionUnit.Resources == 0)
            {
                MaxPositions = access.MaxPositions;
                Radius = access.Radius;
                // Podr�a mover un poco el offsetPosition, no har�a falta que fuese justo en el centro
                positionCount = access.NextPosition;
                access.NextPosition++;
                calculateOffset = true;
            }
            // En caso contrario, no vamos a calcular ning�n Offset, dejamos el que ya est�
            if (!calculateOffset) {
                // TargetOffset.Value = offsetPosition; �No ser�a lo correcto mandar este de todo ceros?
                return TaskStatus.Success;
            }

            var Spread = 360 / MaxPositions;

            bool validPosition = false;
            float bestDistance = float.MaxValue; // Empezamos con el valor peor posible 
            // Realizamos todos los intentos posibles, para intentar dar con una posici�n v�lida
            // (empezamos desde la recomendada... pero nos quedamos con la que est� m�s cerca de la unidad que busca objetivo)
            for (int count = 0; count < MaxPositions; count++ )
            {
                int position = (positionCount + count) % MaxPositions;

                Vector3 newOffsetPosition;
                // Usa la ecuaci�n param�trica del c�rculo para determinar la siguiente posici�n.
                // Parte de la transformada original del objetivo (bueno, un poco m�s alta)
                newOffsetPosition.x = offsetPosition.x + Radius * Mathf.Sin((Spread * position) * Mathf.Deg2Rad);
                newOffsetPosition.y = offsetPosition.y + 1.5f;
                newOffsetPosition.z = offsetPosition.z + Radius * Mathf.Cos((Spread * position) * Mathf.Deg2Rad);

                // C�digo que pod�a ser est�tico para comprobar si est� libre la posici�n anterior.
                var hitColliders = Physics.OverlapSphere(NavigationTarget.Value.position + newOffsetPosition, 1f); // Podr�a usar otros valores algo m�s grandes en vez de subir 1.5 y hacer esfera de radio 1

                // No hay ning�n collider all�, est� libre... o como mucho estoy yo
                // Posible mejora: Ignorar tambi�n todos los colliders de proyectiles
                if (hitColliders.Length == 0 || (hitColliders.Length == 1 && hitColliders[0].transform == transform)) 
                {
                    float nearbyDistance = Vector3.Distance(transform.position, NavigationTarget.Value.position + newOffsetPosition);
                    if (nearbyDistance < bestDistance)
                    {
                        // Machaco el valor que hubiera podido haber antes
                        TargetOffset.Value = newOffsetPosition;
                        // Me guardo la mejor distancia y apunto que he encontrado una posici�n valida
                        bestDistance = nearbyDistance;
                        validPosition = true;
                    }
                }
            }
            if (validPosition)
                return TaskStatus.Success;

            // Si no ha encontrado ninguna posici�n libre, se fracasa (tal vez se podr�a devolver offset cero y listo...)
            return TaskStatus.Failure;
        }
    }
}