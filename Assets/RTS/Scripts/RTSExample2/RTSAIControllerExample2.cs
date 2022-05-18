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

namespace es.ucm.fdi.iav.rts.example2
{
    /*
     * Ejemplo básico sobre cómo crear un controlador basado en IA para el minijuego RTS.
     * Únicamente mandan unas órdenes cualquiera, para probar cosas aleatorias... pero no realiza análisis táctico, ni considera puntos de ruta tácticos, ni coordina acciones de ningún tipo .
     */ 
    public class RTSAIControllerExample2: RTSAIController
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

        // Última unidad creada
        private Unit LastUnit { get; set; }

        // Despierta el controlado y configura toda estructura interna que sea necesaria
        private void Awake()
        {
            Name = "Example 2";
            Author = "Federico Peinado";
        }

        // El método de pensar que sobreescribe e implementa el controlador, para percibir (hacer mapas de influencia, etc.) y luego actuar.
        protected override void Think()
        {
            // Actualizo el mapa de influencia 
            // ...

            // Para las órdenes aquí estoy asumiendo que tengo dinero de sobra y que se dan las condiciones de todas las cosas...
            // (Ojo: esto no debería hacerse porque si me equivoco, causaré fallos en el juego... hay que comprobar que cada llamada tiene sentido y es posible hacerla)

            // Aquí lo suyo sería elegir bien la acción a realizar. 
            // En este caso como es para probar, voy dando a cada vez una orden de cada tipo, todo de seguido y muy aleatorio... 
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
                    break;

                case 1:

                    LastUnit = RTSGameManager.Instance.CreateUnit(this, MyFirstBaseFacility, RTSGameManager.UnitType.EXPLORATION);
                    break;

                case 2:
                    LastUnit = RTSGameManager.Instance.CreateUnit(this, MyFirstBaseFacility, RTSGameManager.UnitType.EXPLORATION);
                    break;

                case 3:
                    LastUnit = RTSGameManager.Instance.CreateUnit(this, MyFirstBaseFacility, RTSGameManager.UnitType.DESTRUCTION);
                    break;

                case 4:
                    LastUnit = RTSGameManager.Instance.CreateUnit(this, MyFirstBaseFacility, RTSGameManager.UnitType.DESTRUCTION);
                    break;

                case 5:
                    LastUnit = RTSGameManager.Instance.CreateUnit(this, MyFirstBaseFacility, RTSGameManager.UnitType.EXTRACTION);
                    break;

                case 6:
                    LastUnit = RTSGameManager.Instance.CreateUnit(this, MyFirstBaseFacility, RTSGameManager.UnitType.EXPLORATION);
                    break;

                case 7:
                    RTSGameManager.Instance.MoveUnit(this, LastUnit, MyFirstProcessingFacility.transform);
                    break;

                case 8:
                    LastUnit = RTSGameManager.Instance.CreateUnit(this, MyFirstBaseFacility, RTSGameManager.UnitType.EXPLORATION);
                    break;

                case 9:
                    RTSGameManager.Instance.MoveUnit(this, LastUnit, MyFirstProcessingFacility.transform);
                    break;

                case 10:
                    LastUnit = RTSGameManager.Instance.CreateUnit(this, MyFirstBaseFacility, RTSGameManager.UnitType.EXPLORATION);
                    break;

                case 11:
                    RTSGameManager.Instance.MoveUnit(this, LastUnit, MyFirstProcessingFacility.transform);
                    // No lo hago... pero también se podrían crear y mover varias unidades en el mismo momento, claro...
                    break;

                case 12:
                    Stop = true;
                    break;
            }
            //Debug.Log("Controlador automático " + MyIndex + " ha finalizado el paso de pensamiento " + ThinkStepNumber);
            ThinkStepNumber++;
        }
    }
}