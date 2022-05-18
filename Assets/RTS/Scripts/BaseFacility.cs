/*    
   Copyright (C) 2020 Federico Peinado
   http://www.federicopeinado.com

   Este fichero forma parte del material de la asignatura Inteligencia Artificial para Videojuegos.
   Esta asignatura se imparte en la Facultad de Informática de la Universidad Complutense de Madrid (España).

   Autores originales: Opsive (Behavior Designer Samples)
   Revisión: Federico Peinado 
   Contacto: email@federicopeinado.com
*/
using System;
using UnityEngine;

namespace es.ucm.fdi.iav.rts
{
    /* 
     * La instalación base hace las veces de barracón del ejército. Tiene como función principal el crear nuevas unidades, que genera y situa de manera ordenada a su alrededor.
     * 
     * Posibles mejoras: 
     * - Averiguar por qué a veces ocurre que se posicionan en otra parte de la malla de navegación, según posición y rotación de la instalación (como si tuviesen problemas en posicionarlas donde corresponde)
     * - Tener en cuenta la altura del suelo en cada situación de unidades
     * - Comprobar físicamente cada vez que no hay obstáculos de ningún tipo obstruyendo las posiciones donde se generan las unidades
     * - Tal vez generar las unidades en el techo de la instalación base, que sería una superficie navegable desde la que dejarse caer al resto de la malla de navegación. Se evitaría generar una nueva unidad si no se ha vaciado previamente dicho techo de la instalación.
     */
    public class BaseFacility : Facility
    {
        // La transformada donde tienen que generarse las unidades, incluye posición (habitualmente abajo a la izquierda), rotación y vectores básicos para orientarse
        [SerializeField] private Transform _spawnTransform = null;
        public Transform SpawnTransform { get { return _spawnTransform; } }

        // Número de unidades que pueden aparecer en una misma fila antes de que se pase a la siguiente fila (nueva)
        [SerializeField] private int _unitsPerRow = 5;
        public int UnitsPerRow { get { return _unitsPerRow; } }
        // Distancia con la que deben separarse las unidades unas de otras cuando son generadas, tanto en filas como en columnas
        // Posibles mejoras: Que dependa del tamaño de la unidad más grande que se pueda generar
        [SerializeField] private int _rowSpacing = 4;
        public int RowSpacing { get { return _rowSpacing; } }
        [SerializeField] private int _columnSpacing = 5;
        public int ColumnSpacing { get { return _columnSpacing; } }

        /******************************************************************************/

        // Los número de columnas y filas que se van usando para situar las unidades
        private int _column = 0;
        private int _row = 0;

        // Despierta la instalación base y comprueba que tiene transformada de generación asignada
        protected override void Awake()
        {
            // Se llama al padre
            base.Awake();

            if (SpawnTransform == null)
                throw new Exception("No hay transformada de generación asignada.");
        }

        // Se comprueba que es posible situar la unidad en una posición correcta de la instalación (ahora mismo siempre se permite hacerlo)
        // Posibles mejoras: Devolver falso en caso de que ya esté ocupado el espacio de generación (sea con unidades ya creadas, o con cualquier otro objeto que esté por allí)
        public bool CanPlaceUnit()
        {
            return true;
        }

        // Se situa la unidad en una posición correcta de la instalación, como sea posible
        // Por seguridad, se pide la referencia del controlador que sea dueño tanto de la instalación como de la unidad
        // Posibles mejoras: Podría devolver simplemente falso si no es posible ubicarla en esta instalación base.
        public void PlaceUnit(RTSController controller, Unit unit)
        {
            if (controller == null || unit == null)
                throw new ArgumentNullException();

            int index = RTSGameManager.Instance.GetIndex(controller);
            if (index != GetControllerIndex())
                throw new ArgumentException("El controlador " + index + " no posee esta instalación.");
            if (index != unit.GetControllerIndex())
                throw new ArgumentException("El controlador " + index + " no posee la unidad que va a crearse.");

            // Código que podía ser estático para comprobar si ha quedado libre la posición de la transformada original de generación (bueno, un poco más alta), para reiniciar el conteo de la situación de unidades
            Vector3 overlapPosition = new Vector3(SpawnTransform.position.x, SpawnTransform.position.y + 1.5f, SpawnTransform.position.z);
            var hitColliders = Physics.OverlapSphere(overlapPosition, 1f); // Podría usar otros valores algo más grandes en vez de subir 1.5 y hacer esfera de radio 1
            if (hitColliders.Length == 0) // No hay ningún collider allí
            {
                _row = 0;
                _column = 0;
            }

            // Se sigue un patrón de rejilla, situando unidades a derechas (columnas) y luego hacia arriba (filas), tomando la posición de generación inicial como esquina inferior izquierda.
            if (_column == UnitsPerRow)
            {
                _row++;
                _column = 0;
            }
            unit.transform.position = SpawnTransform.position + SpawnTransform.right * _column * ColumnSpacing + SpawnTransform.forward * _row * RowSpacing;
            //unit.transform.rotation = SpawnTransform.rotation; // Podría irse cambiando algo esta rotación inicial, aunque sea aleatoriamente

            _column++;
        }                      
    }
}