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
using UnityEngine;

namespace es.ucm.fdi.iav.rts
{
    /* 
     * La instalaci�n base hace las veces de barrac�n del ej�rcito. Tiene como funci�n principal el crear nuevas unidades, que genera y situa de manera ordenada a su alrededor.
     * 
     * Posibles mejoras: 
     * - Averiguar por qu� a veces ocurre que se posicionan en otra parte de la malla de navegaci�n, seg�n posici�n y rotaci�n de la instalaci�n (como si tuviesen problemas en posicionarlas donde corresponde)
     * - Tener en cuenta la altura del suelo en cada situaci�n de unidades
     * - Comprobar f�sicamente cada vez que no hay obst�culos de ning�n tipo obstruyendo las posiciones donde se generan las unidades
     * - Tal vez generar las unidades en el techo de la instalaci�n base, que ser�a una superficie navegable desde la que dejarse caer al resto de la malla de navegaci�n. Se evitar�a generar una nueva unidad si no se ha vaciado previamente dicho techo de la instalaci�n.
     */
    public class BaseFacility : Facility
    {
        // La transformada donde tienen que generarse las unidades, incluye posici�n (habitualmente abajo a la izquierda), rotaci�n y vectores b�sicos para orientarse
        [SerializeField] private Transform _spawnTransform = null;
        public Transform SpawnTransform { get { return _spawnTransform; } }

        // N�mero de unidades que pueden aparecer en una misma fila antes de que se pase a la siguiente fila (nueva)
        [SerializeField] private int _unitsPerRow = 5;
        public int UnitsPerRow { get { return _unitsPerRow; } }
        // Distancia con la que deben separarse las unidades unas de otras cuando son generadas, tanto en filas como en columnas
        // Posibles mejoras: Que dependa del tama�o de la unidad m�s grande que se pueda generar
        [SerializeField] private int _rowSpacing = 4;
        public int RowSpacing { get { return _rowSpacing; } }
        [SerializeField] private int _columnSpacing = 5;
        public int ColumnSpacing { get { return _columnSpacing; } }

        /******************************************************************************/

        // Los n�mero de columnas y filas que se van usando para situar las unidades
        private int _column = 0;
        private int _row = 0;

        // Despierta la instalaci�n base y comprueba que tiene transformada de generaci�n asignada
        protected override void Awake()
        {
            // Se llama al padre
            base.Awake();

            if (SpawnTransform == null)
                throw new Exception("No hay transformada de generaci�n asignada.");
        }

        // Se comprueba que es posible situar la unidad en una posici�n correcta de la instalaci�n (ahora mismo siempre se permite hacerlo)
        // Posibles mejoras: Devolver falso en caso de que ya est� ocupado el espacio de generaci�n (sea con unidades ya creadas, o con cualquier otro objeto que est� por all�)
        public bool CanPlaceUnit()
        {
            return true;
        }

        // Se situa la unidad en una posici�n correcta de la instalaci�n, como sea posible
        // Por seguridad, se pide la referencia del controlador que sea due�o tanto de la instalaci�n como de la unidad
        // Posibles mejoras: Podr�a devolver simplemente falso si no es posible ubicarla en esta instalaci�n base.
        public void PlaceUnit(RTSController controller, Unit unit)
        {
            if (controller == null || unit == null)
                throw new ArgumentNullException();

            int index = RTSGameManager.Instance.GetIndex(controller);
            if (index != GetControllerIndex())
                throw new ArgumentException("El controlador " + index + " no posee esta instalaci�n.");
            if (index != unit.GetControllerIndex())
                throw new ArgumentException("El controlador " + index + " no posee la unidad que va a crearse.");

            // C�digo que pod�a ser est�tico para comprobar si ha quedado libre la posici�n de la transformada original de generaci�n (bueno, un poco m�s alta), para reiniciar el conteo de la situaci�n de unidades
            Vector3 overlapPosition = new Vector3(SpawnTransform.position.x, SpawnTransform.position.y + 1.5f, SpawnTransform.position.z);
            var hitColliders = Physics.OverlapSphere(overlapPosition, 1f); // Podr�a usar otros valores algo m�s grandes en vez de subir 1.5 y hacer esfera de radio 1
            if (hitColliders.Length == 0) // No hay ning�n collider all�
            {
                _row = 0;
                _column = 0;
            }

            // Se sigue un patr�n de rejilla, situando unidades a derechas (columnas) y luego hacia arriba (filas), tomando la posici�n de generaci�n inicial como esquina inferior izquierda.
            if (_column == UnitsPerRow)
            {
                _row++;
                _column = 0;
            }
            unit.transform.position = SpawnTransform.position + SpawnTransform.right * _column * ColumnSpacing + SpawnTransform.forward * _row * RowSpacing;
            //unit.transform.rotation = SpawnTransform.rotation; // Podr�a irse cambiando algo esta rotaci�n inicial, aunque sea aleatoriamente

            _column++;
        }                      
    }
}