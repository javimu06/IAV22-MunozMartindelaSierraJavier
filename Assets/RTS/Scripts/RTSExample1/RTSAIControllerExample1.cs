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
using System.Collections.Generic;

namespace es.ucm.fdi.iav.rts.example1
{
    // El controlador t�ctico que proporciono de ejemplo... simplemente manda �rdenes RANDOM, y no hace ninguna interpretaci�n (localizar puntos de ruta bien, an�lisis t�ctico, acci�n coordinada...) 
    public class RTSAIControllerExample1 : RTSAIController
    {
        // No necesita guardar mucha informaci�n porque puede consultar la que desee por sondeo, incluida toda la informaci�n de instalaciones y unidades, tanto propias como ajenas

        // Mi �ndice de controlador y un par de instalaciones para referenciar
        private int MyIndex { get; set; }
        private BaseFacility MyBaseFacility { get; set; }
        private BaseFacility OtherBaseFacility { get; set; }
        private ProcessingFacility MyProcessingFacility { get; set; }

        // N�mero de paso de pensamiento 
        private int ThinkStepNumber { get; set; } = 0;

        // �ltima unidad creada
        private Unit LastUnit { get; set; }

        // Despierta el controlado y configura toda estructura interna que sea necesaria
        private void Awake()
        {
            Name = "Example 1";
            Author = "Federico Peinado";
        }

        // El m�todo de pensar que sobreescribe e implementa el controlador, para percibir (hacer mapas de influencia, etc.) y luego actuar.
        protected override void Think()
        {
            // Actualizo el mapa de influencia 
            // ...

            // Para las �rdenes aqu� estoy asumiendo que tengo dinero de sobra y que se dan las condiciones de todas las cosas...
            // (Ojo: esto no deber�a hacerse porque si me equivoco, causar� fallos en el juego... hay que comprobar que cada llamada tiene sentido y es posible hacerla)

            // Aqu� lo suyo ser�a elegir bien la acci�n a realizar. 
            // En este caso como es para probar, voy dando a cada vez una orden de cada tipo, todo de seguido y muy aleatorio...

            switch (ThinkStepNumber)
            {
                case 0: // El primer contacto, un paso especial
                    // Lo primer es conocer el �ndice que me ha asignado el gestor del juego
                    MyIndex = RTSGameManager.Instance.GetIndex(this);

                    // Obtengo referencias a mis cosas
                    MyBaseFacility = RTSGameManager.Instance.GetBaseFacilities(MyIndex)[0];
                    List<int> OtherIndexes = RTSGameManager.Instance.GetIndexes();
                    OtherIndexes.Remove(MyIndex); // Entiendo que no estoy modificando la lista original de �ndices...
                    OtherBaseFacility = RTSGameManager.Instance.GetBaseFacilities(OtherIndexes[0])[0];
                    MyProcessingFacility = RTSGameManager.Instance.GetProcessingFacilities(MyIndex)[0];
                    // ...

                    // Obtengo referencias a las cosas de mi enemigo
                    // ...

                    // Construyo por primera vez el mapa de influencia (con las 'capas' que necesite)
                    // ...
                    break;

                case 1:

                    LastUnit = RTSGameManager.Instance.CreateUnit(this, MyBaseFacility, RTSGameManager.UnitType.EXTRACTION);
                    break;

                case 2:
                    RTSGameManager.Instance.MoveUnit(this, LastUnit, OtherBaseFacility.transform);
                    break;

                case 3:
                    LastUnit = RTSGameManager.Instance.CreateUnit(this, MyBaseFacility, RTSGameManager.UnitType.DESTRUCTION);
                    break;

                case 4:
                    RTSGameManager.Instance.MoveUnit(this, LastUnit, OtherBaseFacility.transform);
                    break;

                case 5:
                    LastUnit = RTSGameManager.Instance.CreateUnit(this, MyBaseFacility, RTSGameManager.UnitType.EXTRACTION);
                    break;

                case 6:
                    RTSGameManager.Instance.MoveUnit(this, LastUnit, OtherBaseFacility.transform);
                    break;

                case 7:
                    RTSGameManager.Instance.MoveUnit(this, LastUnit, MyProcessingFacility.transform);
                    break;

                case 8:
                    LastUnit = RTSGameManager.Instance.CreateUnit(this, MyBaseFacility, RTSGameManager.UnitType.EXPLORATION);
                    break;

                case 9:
                    RTSGameManager.Instance.MoveUnit(this, LastUnit, OtherBaseFacility.transform);
                    break;

                case 10:
                    LastUnit = RTSGameManager.Instance.CreateUnit(this, MyBaseFacility, RTSGameManager.UnitType.EXPLORATION);
                    break;

                case 11:
                    RTSGameManager.Instance.MoveUnit(this, LastUnit, MyProcessingFacility.transform);
                    // No lo hago... pero tambi�n se podr�an crear y mover varias unidades en el mismo momento, claro...
                    break;

                case 12:
                    Stop = true;
                    break;
            }
            //Debug.Log("Controlador autom�tico " + MyIndex + " ha finalizado el paso de pensamiento " + ThinkStepNumber);
            ThinkStepNumber++;
        }
    }
}