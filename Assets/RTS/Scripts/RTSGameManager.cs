/*    
   Copyright (C) 2020 Federico Peinado
   http://www.federicopeinado.com

   Este fichero forma parte del material de la asignatura Inteligencia Artificial para Videojuegos.
   Esta asignatura se imparte en la Facultad de Inform�tica de la Universidad Complutense de Madrid (Espa�a).

   Autores originales: Opsive (Behavior Designer Samples)
   Revisi�n: Federico Peinado 
   Contacto: email@federicopeinado.com
*/
using System; 
using System.Collections.Generic;
using UnityEngine; 

namespace es.ucm.fdi.iav.rts
{
    /* 
     * El gestor del juego es responsable de poner en marcha el juego, iniciando su estado y llevando un seguimiento de todos sus cambios.
     * Mantiene el registro de todas las instalaciones y unidades creadas y activas, de manera que los bots t�cticos puedan percibir y actuar en respuesta a situaciones t�cticas como el ser atacados o pillar desprevenida la base enemiga.
     * 
     * Posibles mejoras:
     * - Mostra una GUI como la que tiene ScenarioManager pero con informaci�n del juego, como nombres y autores de controladores, cantidad total de instalaciones, unidades y salud total de uno y otro jugador.
     * -Quiz� la tecla M para mostrar mapas de influencia (de ambas IAs si las hay) deber�a estar centralizada aqu�, en el GameManager.
     */
    public class RTSGameManager : MonoBehaviour
    {
        // Enumerado con los tipos de instalaciones admitidas 
        public enum FacilityType {BASE, PROCESSING};
        // Enumerado con los tipos de unidades admitidas 
        public enum UnitType {EXTRACTION, EXPLORATION, DESTRUCTION};

        // Lista con todos los controladores. Lo esperado es que sean 2 (a cada uno com�nmente le llamaremos por el �ndice en esta lista + 1; ej. Jugador 1 vs Jugador 2).
        [SerializeField] private List<RTSController> _controllers = null;
        private List<RTSController> Controllers { get { return _controllers; } }

        // Cantidad inicial de dinero con la que empiezan los controladores
        [SerializeField] private int _initialMoney = 50000;
        public int InitialMoney { get { return _initialMoney; } }

        // Informaci�n sobre la unidad extractora
        [SerializeField] private int _extractionUnitCost = 10000;
        public int ExtractionUnitCost { get { return _extractionUnitCost; } }
        [SerializeField] private int _extractionUnitsMax = 5;
        public int ExtractionUnitsMax { get { return _extractionUnitsMax; } }
        // Los prefabs de las distintas unidades. Hay dos variantes, una para los jugadores con �ndice par (como el 0) y otro para los jugadores con �ndice impar
        [SerializeField] private GameObject _extractionUnitEvenPrefab = null;
        private GameObject ExtractionUnitEvenPrefab { get { return _extractionUnitEvenPrefab; } }
        [SerializeField] private GameObject _extractionUnitOddPrefab = null;
        private GameObject ExtractionUnitOddPrefab { get { return _extractionUnitOddPrefab; } }

        // Informaci�n sobre la unidad exploradora
        [SerializeField] private int _explorationUnitCost = 15000;
        public int ExplorationUnitCost { get { return _explorationUnitCost; } }
        [SerializeField] private int _explorationUnitsMax = 20;
        public int ExplorationUnitsMax { get { return _explorationUnitsMax; } }
        // Los prefabs de las distintas unidades. Hay dos variantes, una para los jugadores con �ndice par (como el 0) y otro para los jugadores con �ndice impar
        [SerializeField] private GameObject _explorationUnitEvenPrefab = null;
        private GameObject ExplorationUnitEvenPrefab { get { return _explorationUnitEvenPrefab; } }
        [SerializeField] private GameObject _explorationUnitOddPrefab = null;
        private GameObject ExplorationUnitOddPrefab { get { return _explorationUnitOddPrefab; } }

        // Informaci�n sobre la unidad destructura
        [SerializeField] private int _destructionUnitCost = 30000;
        public int DestructionUnitCost { get { return _destructionUnitCost; } }
        [SerializeField] private int _destructionUnitsMax = 10;
        public int DestructionUnitsMax { get { return _destructionUnitsMax; } }
        // Los prefabs de las distintas unidades. Hay dos variantes, una para los jugadores con �ndice par (como el 0) y otro para los jugadores con �ndice impar
        [SerializeField] private GameObject _destructionUnitEvenPrefab = null;
        private GameObject DestructionUnitEvenPrefab { get { return _destructionUnitEvenPrefab; } }
        [SerializeField] private GameObject _destructionUnitOddPrefab = null;
        private GameObject DestructionUnitOddPrefab { get { return _destructionUnitOddPrefab; } }

        // Utiliza un sencillo patr�n Singleton para dar acceso global y eliminar duplicados, aunque no crea un objeto si no estamos en una escena ni se mantiene si cambiamos de escena
        private static RTSGameManager _instance;
        public static RTSGameManager Instance { get { return _instance; } }

        /******************************************************************************/

        // El dinero de cada controlador (en el �ndice correspondiente del controlador en la lista de controladores)
        // Se ha optado por no usar n�meros decimales para el dinero
        private List<int> ControllersMoney { get; set; } = new List<int>();

        // Las instalaciones de cada controlador, por tipos (en el �ndice correspondiente del controlador en la lista de controladores)
        // Lo normal es que s�lo haya una de cada
        private List<List<BaseFacility>> ControllersBaseFacilities { get; set; } = new List<List<BaseFacility>>();
        private List<List<ProcessingFacility>> ControllersProcessingFacilities { get; set; } = new List<List<ProcessingFacility>>();

        // Las unidades de cada controlador, por tipos (en el �ndice correspondiente del controlador en la lista de controladores)
        private List<List<ExtractionUnit>> ControllersExtractionUnits { get; set; } = new List<List<ExtractionUnit>>();
        private List<List<ExplorationUnit>> ControllersExplorationUnits { get; set; } = new List<List<ExplorationUnit>>();
        private List<List<DestructionUnit>> ControllersDestructionUnits { get; set; } = new List<List<DestructionUnit>>();

        //Mapa influencia
        public GameObject debugPrefab;


        // Despierta el Singleton (lo crea) y elimina duplicados de la misma clase que pueda haber en la escena.
        // Inicializa las estructuras internas del estado del juego, como el dinero que se establece inicialmente en 0.
        // Posibles mejoras: 
        // - Por seguridad podr�an tambi�n destruir los controladores, sus instalaciones y unidades, o incluso el escenario al completo...  y recrearlo todo de alguna manera, por ejemplo desde fichero en el Start. 
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                _instance = this;

                // Se asigna la cantidad inicial de dinero a cada jugador 
                foreach (var controller in Controllers)
                    ControllersMoney.Add(InitialMoney);

                // Se guardan todas las instalaciones y unidades en listas
                for (int index = 0; index < Controllers.Count; index++)
                {
                    var controller = Controllers[index];

                    ControllersBaseFacilities.Add(new List<BaseFacility>(controller.GetComponentsInChildren<BaseFacility>()));
                    ControllersProcessingFacilities.Add(new List<ProcessingFacility>(controller.GetComponentsInChildren<ProcessingFacility>()));
                    ControllersExtractionUnits.Add(new List<ExtractionUnit>(controller.GetComponentsInChildren<ExtractionUnit>()));
                    ControllersExplorationUnits.Add(new List<ExplorationUnit>(controller.GetComponentsInChildren<ExplorationUnit>()));
                    ControllersDestructionUnits.Add(new List<DestructionUnit>(controller.GetComponentsInChildren<DestructionUnit>()));
                }
            }
        }

        // Inicializa el estado del juego en relaci�n a otros objetos, activando todas las instalaciones y unidades del juego
        private void Start()
        {
                // Se activan todas las instalaciones y unidades, asign�ndoles su controlador correspondiente
                for (int index = 0; index < Controllers.Count; index++)
                {
                    var controller = Controllers[index];

                    foreach (var baseFacility in ControllersBaseFacilities[index])
                        baseFacility.Enable(controller);

                    foreach (var processingFacility in ControllersProcessingFacilities[index])
                        processingFacility.Enable(controller);

                    foreach (var extractionUnit in ControllersExtractionUnits[index])
                        extractionUnit.Enable(controller);

                    foreach (var explorationUnit in ControllersExplorationUnits[index])
                        explorationUnit.Enable(controller);

                    foreach (var destructionUnit in ControllersDestructionUnits[index])
                        destructionUnit.Enable(controller);
            }     


        }

        // Devuelve el �ndice correspondiente al controlador que nos pasan.
        // Aunque parezca obvio obtenerlos de la lista de controladores, hay que usar este m�todo porque por seguridad el gestor podr�a asignar �ndices extra�os y no consecutivos a los controladores
        public int GetIndex(RTSController controller)
        {
            if (controller == null)
                throw new ArgumentNullException();

            for (int index = 0; index < Controllers.Count; index++)
            {
                if (Controllers[index] == controller)
                    return index;
            }
            throw new ArgumentException("El gestor del juego no conoce este controlador");
        }

        // Devuelve la lista con todos los �ndices de los controladores que hay en juego.
        // Aunque parezca obvio obtenerlos de la lista de controladores, hay que usar este m�todo porque por seguridad el gestor podr�a asignar �ndices extra�os y no consecutivos a los controladores
        public List<int> GetIndexes()
        {
            List<int> list = new List<int>();

            // Ahora mismo los �ndices son obvios
            for (int index = 0; index < Controllers.Count; index++)
            {
                list.Add(index);
            }

            return list;
        }

        // Devuelve el dinero que tiene el controlador correspondiente al �ndice introducido.
        // Si el �ndice es menor que cero, o igual o mayor que la cantidad de controladores que hay, saltar� una excepci�n.
        public int GetMoney(int index)
        { 
            return ControllersMoney[index];
        }

        // Devuelve las instalaciones base que tiene el controlador correspondiente al �ndice introducido.
        // Si el �ndice es menor que cero, o igual o mayor que la cantidad de controladores que hay, saltar� una excepci�n.
        public List<BaseFacility> GetBaseFacilities(int index)
        {
            return ControllersBaseFacilities[index];
        }

        // Devuelve las instalaciones de procesamiento que tiene el controlador correspondiente al �ndice introducido.
        // Si el �ndice es menor que cero, o igual o mayor que la cantidad de controladores que hay, saltar� una excepci�n.
        public List<ProcessingFacility> GetProcessingFacilities(int index)
        {
            return ControllersProcessingFacilities[index];
        }

        // Devuelve las unidades de extracci�n que tiene el controlador correspondiente al �ndice introducido.
        // Si el �ndice es menor que cero, o igual o mayor que la cantidad de controladores que hay, saltar� una excepci�n.
        public List<ExtractionUnit> GetExtractionUnits(int index)
        {
            return ControllersExtractionUnits[index];
        }

        // Devuelve las unidades de exploraci�n que tiene el controlador correspondiente al �ndice introducido.
        // Si el �ndice es menor que cero, o igual o mayor que la cantidad de controladores que hay, saltar� una excepci�n.
        public List<ExplorationUnit> GetExplorationUnits(int index)
        {
            return ControllersExplorationUnits[index];
        }

        // Devuelve las unidades de destrucci�n que tiene el controlador correspondiente al �ndice introducido.
        // Si el �ndice es menor que cero, o igual o mayor que la cantidad de controladores que hay, saltar� una excepci�n.
        public List<DestructionUnit> GetDestructionUnits(int index)
        {
            return ControllersDestructionUnits[index];
        }

        // Una instalaci�n de procesamiento llena de recursos solicita ser procesada, lo que permitir� obtener dinero al controlador correspondiente.
        // Por seguridad se lanzan excepciones si no hay instalaci�n de procesamiento o si esta no tiene recursos.
        // Devuelve la cantidad de dinero obtenida.
        public int Process(ProcessingFacility facility)
        {
            if (facility == null)
                throw new ArgumentNullException();                       

            if (facility.Resources <= 0)
                throw new ArgumentException("Se est� intentando procesar recursos en una instalaci�n que no los tiene");

            // Se quita la cantidad de recursos de la instalaci�n de procesamiento
            int resources = facility.Resources;
            facility.Resources = 0;

            // Ahora mismo no se utiliza una ratio Recursos/Dinero de ning�n tipo, sino que se utiliza directamente el mismo valor.
            int money = resources;

            // Se a�ade la cantidad de dinero proporcional al controlador correspondiente 
            int index = facility.GetControllerIndex();
            ControllersMoney[index] += money;

            return money;
        }

        // Se solicita la creaci�n de una nueva unidad de cierto tipo en una instalaci�n base, gastando el dinero del controlador correspondiente para crear y pedir situarla en dicha instalaci�n.  
        // Por seguridad se pide la referencia al controlador que da la orden y se lanzan excepciones si no hay instalaci�n base o si el controlador correspondiente no tiene dinero suficiente para crear esa unidad.
        // Devuelve una referencia a la unidad creada.
        public Unit CreateUnit(RTSController controller, BaseFacility facility, UnitType type) 
        {
            if (controller == null || facility == null) 
                throw new ArgumentNullException();

            int index = GetIndex(controller);
            if (index != facility.GetControllerIndex())
                throw new ArgumentException("El controlador " + index + " intenta crear una unidad en una instalaci�n que no es suya");

            // Con el tipo de unidad se comprueba que no se est�n creando unidades por encima del l�mite m�ximo permitido 
            int cost = 0; // Valor inicial que luego se sobreescribe
            GameObject prefab = null; // Valor inicial que luego se sobreescribe 
            switch (type)
            {
                case UnitType.EXTRACTION: 
                    if (ControllersExtractionUnits[index].Count >= ExtractionUnitsMax)
                        throw new Exception("No se pueden crear m�s unidades de extracci�n de las que ya posee este controlador.");
                    cost = ExtractionUnitCost;
                    if (index % 2 == 0) // El jugador de �ndice 0 tiene �ndice par, y el de �ndice 1, impar
                        prefab = ExtractionUnitEvenPrefab;
                    else
                        prefab = ExtractionUnitOddPrefab; 
                    break;
                case UnitType.EXPLORATION: 
                    if (ControllersExplorationUnits[index].Count >= ExplorationUnitsMax)
                        throw new Exception("No se pueden crear m�s unidades de exploraci�n de las que ya posee este controlador.");
                    cost = ExplorationUnitCost;
                    if (index % 2 == 0)  // El jugador de �ndice 0 tiene �ndice par, y el de �ndice 1, impar
                        prefab = ExplorationUnitEvenPrefab;
                    else
                        prefab = ExplorationUnitOddPrefab; 
                    break;
                case UnitType.DESTRUCTION: 
                    if (ControllersDestructionUnits[index].Count >= DestructionUnitsMax)
                        throw new Exception("No se pueden crear m�s unidades de destrucci�n de las que ya posee este controlador.");
                    cost = DestructionUnitCost;
                    if (index % 2 == 0)  // El jugador de �ndice 0 tiene �ndice par, y el de �ndice 1, impar
                        prefab = DestructionUnitEvenPrefab;
                    else
                        prefab = DestructionUnitOddPrefab; 
                    break;
            }

            // Se comprueba que el coste es asumible
            if (cost > ControllersMoney[index]) { 
                throw new ArgumentException("El controlador no tiene dinero para pagar una unidad de coste " + cost + "."); 
            }

            // Se comprueba que es posible situar la unidad en una posici�n correcta de la instalaci�n
            if (!facility.CanPlaceUnit())
            {
                throw new ArgumentException("La instalaci�n base no puede generar una unidad ahora.");
            }

            // Se resta el dinero gastado al controlador correspondiente    
            ControllersMoney[index] -= cost;

            // Provisionalmente se genera el objeto con la misma posici�n y rotaci�n de la instalaci�n base (no de su transformada original)
            var spawnedObject = GameObject.Instantiate(prefab, facility.transform.position, facility.transform.rotation) as GameObject;

            // Se 'activa' la unidad, para asociarla a su controlador y que inicie su actividad 
            var spawnedUnit = spawnedObject.GetComponent<Unit>();
            spawnedUnit.Enable(Controllers[index]);

            // La instalaci�n se ocupa de situar la unidad
            facility.PlaceUnit(controller, spawnedUnit);

            // Como padre en la jerarqu�a, tiene a su controlador (lo asigno despu�s de haberlo reposicionado)
            spawnedObject.transform.parent = Controllers[index].transform;

            // Almacena la unidad generada en su lista correspondiente
            switch (type)
            {
                case UnitType.EXTRACTION:
                    ControllersExtractionUnits[index].Add(spawnedObject.GetComponent<ExtractionUnit>());
                    break;
                case UnitType.EXPLORATION:
                    ControllersExplorationUnits[index].Add(spawnedObject.GetComponent<ExplorationUnit>());
                    break;
                case UnitType.DESTRUCTION:
                    ControllersDestructionUnits[index].Add(spawnedObject.GetComponent<DestructionUnit>());
                    break;
            }

            return spawnedUnit;
        }

        // Se solicita el movimiento de una unidad a un objeto concreto (transformada) del escenario de batalla. 
        // Dependiendo de la unidad, luego puede que esta se ponga a extraer los recursos de un campo cercano, o a atacar alguna unidad o instalaci�n enemiga.
        // Cada tipo de unidad sabr� qu� variables deben cambiar para que su �rbol de comportamiento act�e debidamente.
        // Posibles mejoras: 
        // - Se podr�a devolver un booleano para saber si el movimiento se pudo realizar sin problemas.
        public void MoveUnit(RTSController controller, Unit unit, Transform transform) 
        {
            if (controller == null)
                throw new ArgumentNullException("No se ha pasado un controlador.");
            if (unit == null)
                throw new ArgumentNullException("No se ha pasado una unidad.");
            if (transform == null)
                throw new ArgumentNullException("No se ha pasado una transformada para el movimiento.");

            int index = GetIndex(controller);
            if (index != unit.GetControllerIndex())
                throw new ArgumentException("El controlador " + index + " intenta mover una unidad que no es suya");

            // Si la posici�n de la transformada fuese absurda con respecto al escenario (terreno) de batalla (Ground), tambi�n hay excepci�n
            if (!RTSScenarioManager.Instance.InsideScenario(transform.position))
                throw new ArgumentException("La transformada a la que se intenta mover la unidad est� fuera del escenario.");

            unit.Move(controller, transform);
        }

        // Se solicita el movimiento de una unidad a una posici�n concreta del escenario de batalla. 
        // Dependiendo de la unidad, luego puede que esta se ponga a extraer los recursos de un campo cercano, o a atacar alguna unidad o instalaci�n enemiga.
        // Cada tipo de unidad sabr� qu� variables deben cambiar para que su �rbol de comportamiento act�e debidamente.
        // Posibles mejoras: 
        // - Se podr�a devolver un booleano para saber si el movimiento se pudo realizar sin problemas.
        public void MoveUnit(RTSController controller, Unit unit, Vector3 position)
        {
            if (controller == null)
                throw new ArgumentNullException("No se ha pasado un controlador.");
            if (unit == null)
                throw new ArgumentNullException("No se ha pasado una unidad.");
            if (transform == null)
                throw new ArgumentNullException("No se ha pasado una transformada para el movimiento.");

            int index = GetIndex(controller);
            if (index != unit.GetControllerIndex())
                throw new ArgumentException("El controlador " + index + " intenta mover una unidad que no es suya");

            // Si la posici�n fuese absurda con respecto al escenario (terreno) de batalla (Ground), tambi�n hay excepci�n
            if (!RTSScenarioManager.Instance.InsideScenario(position))
                throw new ArgumentException("La posici�n a la que se intenta mover la unidad cae fuera del escenario.");

            unit.Move(controller, position);
        }

        // Se solicita la detenci�n del movimiento de una unidad (en realidad es azucar sint�ctico para ordenar moverse al mismo sitio donde est�). 
        // Dependiendo de la unidad, luego puede que esta se ponga a extraer los recursos de un campo cercano, o a atacar alguna unidad o instalaci�n enemiga.
        // Cada tipo de unidad sabr� qu� variables deben cambiar para que su �rbol de comportamiento act�e debidamente.
        // Posibles mejoras: 
        // - Se podr�a devolver un booleano para saber si la detenci�n se pudo realizar sin problemas.
        public void StopUnit(RTSController controller, Unit unit)
        {
            if (controller == null)
                throw new ArgumentNullException("No se ha pasado un controlador.");
            if (unit == null)
                throw new ArgumentNullException("No se ha pasado una unidad."); 

            int index = GetIndex(controller);
            if (index != unit.GetControllerIndex())
                throw new ArgumentException("El controlador " + index + " intenta mover una unidad que no es suya");

            unit.Move(controller, unit.transform.position);
        }

        // Cuando una unidad va a ser destruida, avisa antes de autodestruirse para que se la elimine de las listas del gestor del juego.
        // Posibles mejoras: 
        // - Se podr�a ir llevando una puntuaci�n de cuantas unidades ha destruido cada ej�rcito, por ejemplo... junto con otras estad�sticas
        public void UnitDestroyed(Unit unit) {
            if (unit == null)
                throw new ArgumentNullException("No se ha pasado una unidad.");

            int index = unit.GetControllerIndex();

            if (unit is ExtractionUnit)
            {
                ControllersExtractionUnits[index].Remove((ExtractionUnit)unit);
                // Mostramos informaci�n de lo que pasa en el juego al menos en consola
                Debug.Log("Unidad de extracci�n [ C" + index + " ] destruida. Quedan " + ControllersExtractionUnits[index].Count);
            }
            if (unit is ExplorationUnit)
            {
                ControllersExplorationUnits[index].Remove((ExplorationUnit)unit);
                // Mostramos informaci�n de lo que pasa en el juego al menos en consola
                Debug.Log("Unidad de exploraci�n [ C" + index + " ] destruida. Quedan " + ControllersExplorationUnits[index].Count);
            }
            if (unit is DestructionUnit)
            {
                ControllersDestructionUnits[index].Remove((DestructionUnit)unit);
                // Mostramos informaci�n de lo que pasa en el juego al menos en consola
                Debug.Log("Unidad de destrucci�n [ C" + index + " ] destruida. Quedan " + ControllersDestructionUnits[index].Count);
            }

       }

        // Cuando una instalaci�n va a ser destruida, avisa antes de autodestruirse para que se la elimine de las listas del gestor del juego.
        // Posibles mejoras: Si han acabado con todas las unidades base de un ej�rcito, se podr�a mostrar un mensaje en la GUI de que ha perdido el juego.
        public void FacilityDestroyed(Facility facility)
        {
            if (facility == null)
                throw new ArgumentNullException("No se ha pasado una instalaci�n.");

            int index = facility.GetControllerIndex();

            if (facility is BaseFacility)
            {
                ControllersBaseFacilities[index].Remove((BaseFacility)facility);
                // Mostramos informaci�n de lo que pasa en el juego al menos en consola
                Debug.Log("Instalaci�n base [ C" + index + " ] destruida. Quedan " + ControllersBaseFacilities[index].Count);
                if (ControllersBaseFacilities[index].Count == 0) { 
                    Debug.Log("*** PARTIDA FINALIZADA ***");
                    // Se deber�a pausar el juego incluso, tener un booleano gameOver o as� y mostrarlo por pantalla... 
                }
            }
            if (facility is ProcessingFacility)
            {
                ControllersProcessingFacilities[index].Remove((ProcessingFacility)facility);
                // Mostramos informaci�n de lo que pasa en el juego al menos en consola
                Debug.Log("Instalaci�n de procesamiento [ C" + index + " ] destruida. Quedan " + ControllersProcessingFacilities[index].Count);
            }
        }
    }
}