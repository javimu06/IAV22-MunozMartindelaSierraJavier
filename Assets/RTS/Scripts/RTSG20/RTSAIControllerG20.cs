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
using System.Collections.Generic;

namespace es.ucm.fdi.iav.rts.G20
{


    enum Estrategias
    {
        DEFENDER, 
        ATACAR,
        FARMEAR,
        GUERRILA,
        EMERGENCIA,
        NINGUNA
    }


    // El controlador táctico que proporciono de ejemplo... simplemente manda órdenes RANDOM, y no hace ninguna interpretación (localizar puntos de ruta bien, análisis táctico, acción coordinada...) 
    public class RTSAIControllerG20 : RTSAIController
    {
        // No necesita guardar mucha información porque puede consultar la que desee por sondeo, incluida toda la información de instalaciones y unidades, tanto propias como ajenas

        // Mi índice de controlador y un par de instalaciones para referenciar
        private int MyIndex { get; set; }
        private int FirstEnemyIndex { get; set; }
        private BaseFacility MyFirstBaseFacility { get; set; }
        private ProcessingFacility MyFirstProcessingFacility { get; set; }
        private BaseFacility FirstEnemyFirstBaseFacility { get; set; }
        private ProcessingFacility FirstEnemyFirstProcessingFacility { get; set; }

        // Número de paso de pensamiento 
        private int ThinkStepNumber { get; set; } = 0;
        private Estrategias currentStrategy { get; set; } = Estrategias.NINGUNA;

        // Última unidad creada
        private Unit LastUnit { get; set; }

        [SerializeField]
        int potenciaColor = 15;
        private int money;

        //!Mapa de influencia
        public InfluenceMap influencemap_;
        public GameObject debugPrefab;

        public Vector2 bottomLeft;
        public Vector2 topRight;



        // Despierta el controlado y configura toda estructura interna que sea necesaria
        private void Awake()
        {
            Name = "G20";
            Author = "Diego Castillo & Javier Muñoz";

            influencemap_ = new InfluenceMap(bottomLeft, topRight, 40, 40);
            influencemap_.setDebugPrefab(debugPrefab);
        }


        // El método de pensar que sobreescribe e implementa el controlador, para percibir (hacer mapas de influencia, etc.) y luego actuar.
        protected override void Think()
        {

            Debug.Log(currentStrategy);
            // Actualizo el mapa de influencia 
            influencemap_.updateDebug();
            influencemap_.updateCost();
            influencemap_.setPotence(potenciaColor);
            // ...
            if (ThinkStepNumber != 0)
            {
                money = RTSGameManager.Instance.GetMoney(MyIndex);

                List<ExtractionUnit> nExtractor = RTSGameManager.Instance.GetExtractionUnits(MyIndex);
                if (nExtractor.Count > 0)
                {
                    foreach (ExtractionUnit n in nExtractor)
                    {
                        influencemap_.propagateInfluence(n.transform.position, 3, 0);
                    }
                }
                List<ExplorationUnit> nExploration = RTSGameManager.Instance.GetExplorationUnits(MyIndex);
                if (nExploration.Count > 0)
                {
                    foreach (ExplorationUnit n in nExploration)
                    {
                        influencemap_.propagateInfluence(n.transform.position, 5, 0);
                    }
                }
                List<DestructionUnit> nDestruction = RTSGameManager.Instance.GetDestructionUnits(MyIndex);
                if (nDestruction.Count > 0)
                {
                    foreach (DestructionUnit n in nDestruction)
                    {
                        influencemap_.propagateInfluence(n.transform.position, 9, 0);
                    }
                }

                ///////////ENEMY//////////
                nExtractor = RTSGameManager.Instance.GetExtractionUnits(FirstEnemyIndex);
                if (nExtractor.Count > 0)
                {
                    foreach (ExtractionUnit n in nExtractor)
                    {
                        influencemap_.propagateInfluence(n.transform.position, 3, 1);
                    }
                }
                nExploration = RTSGameManager.Instance.GetExplorationUnits(FirstEnemyIndex);
                if (nExploration.Count > 0)
                {
                    foreach (ExplorationUnit n in nExploration)
                    {
                        influencemap_.propagateInfluence(n.transform.position, 5, 1);
                    }
                }
                nDestruction = RTSGameManager.Instance.GetDestructionUnits(FirstEnemyIndex);
                if (nDestruction.Count > 0)
                {
                    foreach (DestructionUnit n in nDestruction)
                    {
                        influencemap_.propagateInfluence(n.transform.position, 9, 1);
                    }
                }

                ///////////GRABEN//////////
                List<Tower> towers = RTSScenarioManager.Instance.Towers;
                if (towers.Count > 0)
                {
                    foreach (Tower n in towers)
                    {
                        influencemap_.propagateInfluence(n.transform.position, 3, 2);
                    }
                }
                List<Village> villages = RTSScenarioManager.Instance.Villages;
                if (villages.Count > 0)
                {
                    foreach (Village n in villages)
                    {
                        influencemap_.propagateInfluence(n.transform.position, 3, 2);
                    }
                }

















                //primero hay que elegir una estrategia
                //muchas las haremos consultando el mapa de influencias


                if (money < RTSGameManager.Instance.ExtractionUnitCost && RTSGameManager.Instance.GetExtractionUnits(MyIndex).Count <= 0) //final blow
                {
                    currentStrategy = Estrategias.ATACAR;
                }
                //emergencia, si las casillas cercanas a la base o o al extractor tienen influencia enemiga se entrara en modo de emergencia
                else if (influencemap_.valueArea(MyFirstBaseFacility.transform.position, 5, 0) < 0 || influencemap_.valueArea(MyFirstProcessingFacility.transform.position, 5, 0) < 0)
                {
                    currentStrategy = Estrategias.EMERGENCIA;
                }
                else if (influencemap_.valueArea(MyFirstBaseFacility.transform.position, 9, 0) < 50 || influencemap_.valueArea(MyFirstProcessingFacility.transform.position, 9, 0) < 50)
                {
                    //la base o factoria necesita refuerzos defensivos
                    currentStrategy = Estrategias.DEFENDER;
                }
                else if (RTSGameManager.Instance.GetDestructionUnits(MyIndex).Count >= (RTSGameManager.Instance.DestructionUnitsMax * 2 / 3))
                {
                    //si tengo suficiente potencia de ataque
                    currentStrategy = Estrategias.ATACAR;
                }
                else if ((currentStrategy == Estrategias.FARMEAR || currentStrategy == Estrategias.GUERRILA) && RTSGameManager.Instance.GetExtractionUnits(MyIndex).Count >= 3 && RTSGameManager.Instance.GetExtractionUnits(FirstEnemyIndex).Count > 0)
                {
                    currentStrategy = Estrategias.GUERRILA;
                }
                //si las tropas del enemigo son menores el controlador ahorra dinero, y producira mas extractores si lo considera una ventaja (para prevenir ataques o que el enemigo contruya una armada grande derrepente)(basicamente la guerra fria americana)
                else if (nDestruction.Count *3 + (nExploration.Count) < RTSGameManager.Instance.GetExplorationUnits(MyIndex).Count + (RTSGameManager.Instance.GetDestructionUnits(MyIndex).Count * 3))
                {
                    currentStrategy = Estrategias.FARMEAR;
                }
               








                //tenemso varias estrategias y varias formas de actuar, reuniner una armada, una guardia, conseguir recursos, acumular dinero, etc etc.



                //detectar ataques del enemigo para dirigir una defensa al punto (entenderemos un ataque como la influencia enemiga se encuetre dentro de la influencia de las abses¿?)



                //la primera de las variables que hay que tener en cuenta seria la influencia del enemigo y la nuestra, si son equiparables
                //si el mapa de influencia genera una frontera entrar a ataque o defensa ¿?

                //creacion de unidades
                //priorizar los extractores de recursos y mandar al explorador ocasional a molestar a otros extracotres o base, lo que se considere mas oportuno 
                //mandar a los exploradores a controlar una zona de baja influencia en un punto de extraccion de esencia.

                //cuando se alcanza el numero maximo de extractores, (o 6 o 7 i dunno)
                //pasar a reunir una armada
                //conseguir 10 exploradores
                //crear destructores

                //al llegar al maximo número de construtores liderar un ataque a un punto de influencia que sea conquistable


                //estrategias principales: //HACERSE UN SWITCH CON LAS ESTRATEGIAS

                //DEFENSA
                //ATAQUE
                //FARMEAR
                //GUERRILA
                //EMERGENCIA

                //cada estrategia tiene sus prioridades

            }


            switch (ThinkStepNumber)
            {
                case 0:
                    // Lo primer es conocer el índice que me ha asignado el gestor del juego
                    MyIndex = RTSGameManager.Instance.GetIndex(this);

                    // Obtengo referencias a mis cosas
                    MyFirstBaseFacility = RTSGameManager.Instance.GetBaseFacilities(MyIndex)[0];
                    MyFirstProcessingFacility = RTSGameManager.Instance.GetProcessingFacilities(MyIndex)[0];

                    // Obtengo referencias a las cosas de mi enemigo
                    // ...
                    var indexList = RTSGameManager.Instance.GetIndexes();
                    indexList.Remove(MyIndex);
                    FirstEnemyIndex = indexList[0];
                    FirstEnemyFirstBaseFacility = RTSGameManager.Instance.GetBaseFacilities(FirstEnemyIndex)[0];
                    FirstEnemyFirstProcessingFacility = RTSGameManager.Instance.GetProcessingFacilities(FirstEnemyIndex)[0];

                    // Construyo por primera vez el mapa de influencia (con las 'capas' que necesite)
                    // ...



                    //List<ExtractionUnit> eeu = RTSGameManager.Instance.GetExtractionUnits(MyIndex);
                    //foreach(ExtractionUnit ee in eeu)
                    //{
                    //    RTSGameManager.Instance.MoveUnit(this, ee, MyFirstProcessingFacility.transform);
                    //}

                    ThinkStepNumber++;


                    break;
                case 1:
                    //entrado en estrategias

                    //mandar a lso extractores pos a minar
                    //List<ExtractionUnit> eeu = RTSGameManager.Instance.GetExtractionUnits(MyIndex);
                    //foreach (ExtractionUnit ee in eeu)
                    //{
                    //    RTSGameManager.Instance.MoveUnit(this, ee, MyFirstProcessingFacility.);
                    //}


                    switch (currentStrategy)
                    {
                        case Estrategias.ATACAR:
                            //decidir que hay que atacar
                            {

                                List<DestructionUnit> DUnits = RTSGameManager.Instance.GetDestructionUnits(MyIndex);
                                List<ExplorationUnit> ExpUnits = RTSGameManager.Instance.GetExplorationUnits(MyIndex);
                                if (influencemap_.valueArea(FirstEnemyFirstBaseFacility.transform.position, 9, 1) < influencemap_.valueArea(FirstEnemyFirstProcessingFacility.transform.position,9,1))
                                {
                                    //a por la base
                                    foreach (DestructionUnit du in DUnits)
                                    {
                                        RTSGameManager.Instance.MoveUnit(this, du, FirstEnemyFirstBaseFacility.transform);
                                    }
                                    //la mitad de los exploradores
                                    for (int i = 0; i < ExpUnits.Count/2; i++)
                                    {
                                        RTSGameManager.Instance.MoveUnit(this, ExpUnits[i], FirstEnemyFirstBaseFacility.transform);
                                    }

                                }
                                else
                                {
                                    //a por la factoria 
                                    foreach (DestructionUnit du in DUnits)
                                    {
                                        RTSGameManager.Instance.MoveUnit(this, du, FirstEnemyFirstProcessingFacility.transform);
                                    }
                                    //la mitad de los exploradores
                                    for (int i = 0; i < ExpUnits.Count/2; i++)
                                    {
                                        RTSGameManager.Instance.MoveUnit(this, ExpUnits[i], FirstEnemyFirstProcessingFacility.transform);
                                    }

                                }
                                estrategiaDeCompra();


                            }

                            break;
                        case Estrategias.DEFENDER:
                            //encontrar que hay que defender y mandar tropas a esa posicion
                            {
                                List<DestructionUnit> DUnits = RTSGameManager.Instance.GetDestructionUnits(MyIndex);
                                List<ExplorationUnit> ExpUnits = RTSGameManager.Instance.GetExplorationUnits(MyIndex);
                                if (influencemap_.valueArea(MyFirstBaseFacility.transform.position, 9, 0) < influencemap_.valueArea(MyFirstProcessingFacility.transform.position, 9, 0))
                                {
                                    //a por la base
                                    foreach (DestructionUnit du in DUnits)
                                    {
                                        RTSGameManager.Instance.MoveUnit(this, du, MyFirstBaseFacility.transform);
                                    }
                                    //la mitad de los exploradores
                                    for (int i = 0; i < ExpUnits.Count; i++)
                                    {
                                        RTSGameManager.Instance.MoveUnit(this, ExpUnits[i], MyFirstBaseFacility.transform);
                                    }

                                }
                                else
                                {
                                    //a por la factoria 
                                    foreach (DestructionUnit du in DUnits)
                                    {
                                        RTSGameManager.Instance.MoveUnit(this, du, MyFirstProcessingFacility.transform);
                                    }
                                    //la mitad de los exploradores
                                    for (int i = 0; i < ExpUnits.Count; i++)
                                    {
                                        RTSGameManager.Instance.MoveUnit(this, ExpUnits[i], MyFirstProcessingFacility.transform);
                                    }

                                }
                                estrategiaDeCompra();

                            }

                            break;
                        case Estrategias.FARMEAR:
                            //objetivo, construir extractores y conseguir mas currency
                            {
                                //mandar exploradores con los extratores para defenderlos
                                List<ExplorationUnit> ExpUnits = RTSGameManager.Instance.GetExplorationUnits(MyIndex);
                                List<ExtractionUnit> extUnits = RTSGameManager.Instance.GetExtractionUnits(MyIndex);
                                for (int i = 0; i < ExpUnits.Count; i++)
                                {
                                    RTSGameManager.Instance.MoveUnit(this, ExpUnits[i], extUnits[i%extUnits.Count].transform); 
                                }
                                estrategiaDeCompra();
                            }
                            break;
                        case Estrategias.GUERRILA:
                            //pickear zonas de extraccion y recuperar las tropas
                            {
                                List<ExplorationUnit> ExpUnits = RTSGameManager.Instance.GetExplorationUnits(MyIndex);
                                List<ExtractionUnit> EnemyExtUnits = RTSGameManager.Instance.GetExtractionUnits(FirstEnemyIndex);
                                //mandar a dos tercios de estos a molestar a las unidades de estraccion del adversario

                                ExtractionUnit objetivo = null;
                                int lowestValue = -1;
                                foreach (ExtractionUnit ee in EnemyExtUnits)
                                {
                                    //comproabr cual es el extractor mas expuetp de todos y mandar las tropas hay
                                    int aux = influencemap_.valueArea(ee.transform.position, 9, 1);

                                    if (lowestValue == -1)
                                    {
                                        lowestValue = aux;
                                        objetivo = ee;
                                    }
                                    else
                                    {
                                        if (lowestValue > aux)
                                        {
                                            lowestValue = aux;
                                            objetivo = ee;
                                        }
                                    }
                                }

                                for (int i = 0; i < ExpUnits.Count *2/3; i++)
                                {
                                    RTSGameManager.Instance.MoveUnit(this, ExpUnits[i], objetivo.transform);
                                }

                                estrategiaDeCompra();
                            }
                            break;
                        case Estrategias.EMERGENCIA:
                            {

                                //dirige todas las tropas a la zona de emergencia
                                List<DestructionUnit> DUnits = RTSGameManager.Instance.GetDestructionUnits(MyIndex);
                                List<ExplorationUnit> ExpUnits = RTSGameManager.Instance.GetExplorationUnits(MyIndex);
                                if (influencemap_.valueArea(MyFirstBaseFacility.transform.position, 9, 0) < influencemap_.valueArea(MyFirstProcessingFacility.transform.position, 9, 0))
                                {
                                    //a por la base
                                    foreach (DestructionUnit du in DUnits)
                                    {
                                        RTSGameManager.Instance.MoveUnit(this, du, MyFirstBaseFacility.transform);
                                    }
                                    //la mitad de los exploradores
                                    for (int i = 0; i < ExpUnits.Count; i++)
                                    {
                                        RTSGameManager.Instance.MoveUnit(this, ExpUnits[i], MyFirstBaseFacility.transform);
                                    }

                                }
                                else
                                {
                                    //a por la factoria 
                                    foreach (DestructionUnit du in DUnits)
                                    {
                                        RTSGameManager.Instance.MoveUnit(this, du, MyFirstProcessingFacility.transform);
                                    }
                                    //la mitad de los exploradores
                                    for (int i = 0; i < ExpUnits.Count; i++)
                                    {
                                        RTSGameManager.Instance.MoveUnit(this, ExpUnits[i], MyFirstProcessingFacility.transform);
                                    }

                                }
                                estrategiaDeCompra();
                            }
                            break;
                        case Estrategias.NINGUNA:
                            //esat solo por si acaso
                            {
                                estrategiaDeCompra();

                            }
                            //yoquese esperar
                            break;
                        default:
                            break;
                    }


                    break;


                    //case 1:

                    //    LastUnit = RTSGameManager.Instance.CreateUnit(this, MyFirstBaseFacility, RTSGameManager.UnitType.EXPLORATION);
                    //    break;

                    //case 2:
                    //    LastUnit = RTSGameManager.Instance.CreateUnit(this, MyFirstBaseFacility, RTSGameManager.UnitType.EXPLORATION);
                    //    break;

                    //case 3:
                    //    LastUnit = RTSGameManager.Instance.CreateUnit(this, MyFirstBaseFacility, RTSGameManager.UnitType.DESTRUCTION);
                    //    break;

                    //case 4:
                    //    LastUnit = RTSGameManager.Instance.CreateUnit(this, MyFirstBaseFacility, RTSGameManager.UnitType.DESTRUCTION);
                    //    break;

                    //case 5:
                    //    LastUnit = RTSGameManager.Instance.CreateUnit(this, MyFirstBaseFacility, RTSGameManager.UnitType.EXTRACTION);
                    //    break;

                    //case 6:
                    //    LastUnit = RTSGameManager.Instance.CreateUnit(this, MyFirstBaseFacility, RTSGameManager.UnitType.EXPLORATION);
                    //    break;

                    //case 7:
                    //    RTSGameManager.Instance.MoveUnit(this, LastUnit, MyFirstProcessingFacility.transform);
                    //    break;

                    //case 8:
                    //    LastUnit = RTSGameManager.Instance.CreateUnit(this, MyFirstBaseFacility, RTSGameManager.UnitType.EXPLORATION);
                    //    break;

                    //case 9:
                    //    RTSGameManager.Instance.MoveUnit(this, LastUnit, MyFirstProcessingFacility.transform);
                    //    break;

                    //case 10:
                    //    LastUnit = RTSGameManager.Instance.CreateUnit(this, MyFirstBaseFacility, RTSGameManager.UnitType.EXPLORATION);
                    //    break;

                    //case 11:
                    //    RTSGameManager.Instance.MoveUnit(this, LastUnit, MyFirstProcessingFacility.transform);
                    //    // No lo hago... pero también se podrían crear y mover varias unidades en el mismo momento, claro...
                    //    break;

                    //case 12:
                    //    //Stop = true;
                    //    break;
            }
            //Debug.Log("Controlador automático " + MyIndex + " ha finalizado el paso de pensamiento " + ThinkStepNumber);
        }
        void estrategiaDeCompra()
        {
            //conseguir el maximo de extractores
            if (money > RTSGameManager.Instance.ExtractionUnitCost && RTSGameManager.Instance.GetExtractionUnits(MyIndex).Count < RTSGameManager.Instance.ExtractionUnitsMax)
            {
                //comprar unidad y mandarla a minar
                LastUnit = RTSGameManager.Instance.CreateUnit(this, MyFirstBaseFacility, RTSGameManager.UnitType.EXTRACTION);
                //RTSGameManager.Instance.MoveUnit(this, LastUnit, MyFirstProcessingFacility.UnloadingTransform);

                // A un punto del mapa cualquiera (tomo la posición de un recurso aleatorio) 
                List<LimitedAccess> resourcesList = RTSScenarioManager.Instance.LimitedAccesses;
                LimitedAccess resource = resourcesList[Random.Range(0, resourcesList.Count - 1)];

                RTSGameManager.Instance.MoveUnit(this, LastUnit, resource.transform);

                

            }
            //vamso a apor exploradores y despues destructores
            else if (money > RTSGameManager.Instance.ExplorationUnitCost && (RTSGameManager.Instance.GetExplorationUnits(MyIndex).Count < RTSGameManager.Instance.ExplorationUnitsMax * 2 / 3))
            {
                LastUnit = RTSGameManager.Instance.CreateUnit(this, MyFirstBaseFacility, RTSGameManager.UnitType.EXPLORATION);

            }
            else if (money > RTSGameManager.Instance.DestructionUnitCost && RTSGameManager.Instance.GetDestructionUnits(MyIndex).Count < RTSGameManager.Instance.DestructionUnitsMax)
            {
                LastUnit = RTSGameManager.Instance.CreateUnit(this, MyFirstBaseFacility, RTSGameManager.UnitType.DESTRUCTION);

            }
            

        }

    }

}
