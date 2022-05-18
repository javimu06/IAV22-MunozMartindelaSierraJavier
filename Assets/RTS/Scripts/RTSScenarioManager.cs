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
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace es.ucm.fdi.iav.rts
{
    /* 
     * El gestor del escenario es responsable de identificar y mantener la lista de todos los recursos y otros elementos 'neutrales' del escenario que estén activos, como poblados y torretas.
     * Sirve para que los bots tácticos puedan percibir y actuar en respuesta a situaciones tácticas como estar cerca de peligrosas torretas o de interesantes recursos.
     * 
     * Posibles mejoras:
     * - Administrar desde aquí algunas cuestiones como localizar el recurso más cercano que tiene una unidad extractora o cosas así, ofrecer el servicio de poder iterar por ellos como con las cámaras, por ejemplo.
     * - Crear un Script Scenario y darle algunas propiedades como el nombre, autor, nivel de dificultad, nivel de justicia o simetría, número de jugadores, o algo así.
     * - Podría introducir más tiempos que controlar, para congelar la acción en algunos momentos como al reiniciar
     */
    public class RTSScenarioManager : MonoBehaviour
    {
        // El escenario es un objeto de tipo terreno (puede consultarse su tamaño con terrainData.size)
        [SerializeField] private Terrain _scenario = null;
        public Terrain Scenario { get { return _scenario; } }

        // Velocidad del juego. Por defecto está puesta al doble de lo normal.
        [SerializeField] private float _timeScale = 2f;
        public float TimeScale { get { return _timeScale; } private set { _timeScale = value; } }

        // Lista de toda las cámaras con las que poder visualizar el escenario
        [SerializeField] private List<Camera> _cameras = new List<Camera>();
        public List<Camera> Cameras { get { return _cameras; } }

        // Índice de la cámara activa 
        [SerializeField] private int _cameraIndex = 0;
        public int CameraIndex { get { return _cameraIndex; } private set { _cameraIndex = value; } }

        // Los recursos del escenario (todos serán de acceso limitado... como lo son las instalaciones de procesamiento, por ejemplo)
        public List<LimitedAccess> LimitedAccesses { get; private set; } = new List<LimitedAccess>();

        // Las torretas (neutrales) del escenario
        public List<Tower> Towers { get; private set; } = new List<Tower>();

        // Los poblados (neutrales) del escenario
        public List<Village> Villages { get; private set; } = new List<Village>();
 
        // Utiliza un sencillo patrón Singleton para dar acceso global y eliminar duplicados, aunque no crea un objeto si no estamos en una escena ni se mantiene si cambiamos de escena
        private static RTSScenarioManager _instance;
        public static RTSScenarioManager Instance { get { return _instance; } }

        /********************************************************/

        // El zoom de la cámara
        // Posibles mejoras: Externalizarlo como valores que se puedan cambiar desde el inspector
        private float minFov = 20f;
        private float maxFov = 80f;
        private float sensitivity = 20f;

        // El estilo para las etiquetas de la interfaz
        private GUIStyle _labelStyle { get; set; }

        // Despierta el Singleton (lo crea) y elimina duplicados de la misma clase que pueda haber en la escena.
        // Inicializa las estructuras internas del escenario, como la escala temporal, las cámaras...
        // Posibles mejoras: 
        // - Por seguridad podrían también destruirse torretas, poblados, obstáculos... o el escenario al completo...  y recrearlo todo de alguna manera, por ejemplo desde fichero en el Start. 
        // - Se podrían buscar las cámaras automáticamente, debajo del objecto Scenario
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                _instance = this;
            }

            // Aumenta el tamaño y cambia el color de fuente de letra para OnGUI
            _labelStyle = new GUIStyle();
            _labelStyle.fontSize = 20;
            _labelStyle.normal.textColor = Color.white;

            // Se guardan todos los recursos, torretas y poblados en listas
            LimitedAccesses = new List<LimitedAccess>(Scenario.GetComponentsInChildren<LimitedAccess>()); // No se puede buscar fuera del escenario, porque están las instalaciones de procesamiento, que también son LimitedAccess
            Towers = new List<Tower>(Scenario.GetComponentsInChildren<Tower>());
            Villages = new List<Village>(Scenario.GetComponentsInChildren<Village>());

            // Cambia la velocidad del juego al valor inicial que hayamos dado
            Time.timeScale = TimeScale;

            // Todas las cámaras estarán desactivadas y activo la primera
            Cameras[CameraIndex].enabled = true;
        }

        // Cambia el punto de vista, ciclando por todas las cámaras que tiene el escenario
        private void SwitchViewpoint()
        {
            // Desactivar la cámara actual y activar la siguiente
            Cameras[CameraIndex].enabled = false;
            CameraIndex = (CameraIndex + 1) % Cameras.Count;
            Cameras[CameraIndex].enabled = true;
        }

        // Actualiza el gestor del escenario ajustando el zoom, según la entrada de la rueda del ratón.
        private void Update()
        {
            float fov = Cameras[CameraIndex].fieldOfView;
            fov += Input.GetAxis("Mouse ScrollWheel") * sensitivity;
            fov = Mathf.Clamp(fov, minFov, maxFov);
            Cameras[CameraIndex].fieldOfView = fov;      
        }

        // Cambia la velocidad del juego, ciclando por 0.5x, 1x, 2x, 5x, 10x
        private void SwitchScenarioSpeed()
        {
            // Tal vez se pueda usar Switch, pero hay que tener cuidado con la precisión de los floats
            if (TimeScale > 9.9f)
                TimeScale = 0.5f;
            else
                if (TimeScale > 4.9f)
                    TimeScale = 10f;
                else
                    if (TimeScale > 1.9f)
                        TimeScale = 5f;
                    else
                        if (TimeScale > 0.9f)
                            TimeScale = 2f;
                        else
                            if (TimeScale > 0.49f)
                                TimeScale = 1f;

            Time.timeScale = TimeScale;
        }

        // Dibuja la interfaz gráfica de usuario para que el administrador humano pueda trabajar mientras estudia la batalla entre dos controladores cualesquiera.
        public void OnGUI()
        {
            // Abrimos un área de distribución centrada y abajo, con contenido en horizontal
            float halfWidth = Screen.width / 2;
            float halfAreaWidth = 100;
            float halfHeight = Screen.height / 2;
            float areaHeight = 50;
            GUILayout.BeginArea(new Rect(halfWidth - halfAreaWidth, Screen.height - areaHeight, halfWidth + halfAreaWidth, Screen.height));
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace(); // Realmente no lo centra... estaría bien centrarlo

            if (GUILayout.Button("Switch Viewpoint", GUILayout.ExpandWidth(false)))
            {
                // Se va ciclando entre las cámaras disponibles
                SwitchViewpoint();
            }

            if (GUILayout.Button("Switch Scenario Speed", GUILayout.ExpandWidth(false)))
            {
                // Se va ciclando entre las distintas proporciones temporales
                SwitchScenarioSpeed();
            }

            if (GUILayout.Button("Pause", GUILayout.ExpandWidth(false)))
            {
                // Se va ciclando entre estar parado y al ritmo normal de juego
                // Se podría hacer que hubiese un ritmo que fuese EL DOBLE de rápido
                if (Time.timeScale == 0)
                    Time.timeScale = TimeScale;
                else
                    Time.timeScale = 0;
            }

            if (GUILayout.Button("Reset", GUILayout.ExpandWidth(false)))
            {
                // El reinicio es simplemente recargar la escena actual
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }

            if (GUILayout.Button("Quit", GUILayout.ExpandWidth(false)))
            {
                Application.Quit();
            }

            // Cerramos el área de distribución con contenido en horizontal
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        // Comprueba si una posición está sobre la superficie de un terreno
        // Posible mejora: Comprobar que la altura también se corresponde con la altura del terreno en ese punto
        public bool InsideScenario(Vector3 position)
        {
            // Creo que no hace falta buscar el terreno activo con Terrain.activeTerrain
            // Además la posición de un terreno siempre es su esquina con la X y la Z más pequeñas
            Vector3 terrainPosition = Scenario.transform.position;

            if (position.x < terrainPosition.x)
                return false;
            if (position.x > terrainPosition.x + Scenario.terrainData.size.x)
                return false;
            if (position.z < terrainPosition.z)
                return false;
            if (position.z > terrainPosition.z + Scenario.terrainData.size.z)
                return false;
            return true;
        }

        // Cuando un poblado va a ser destruido, avisa antes de autodestruirse para que se le elimine de las listas del gestor del juego.
        // Posibles mejoras: Acabar con los poblados neutrales podría suponer algún tipo de penalización para los controladores que ejecutan esas órdenes...
        public void VillageDestroyed(Village village)
        {
            if (village == null)
                throw new ArgumentNullException("No se ha pasado un poblado.");

            Villages.Remove(village);
            // Podría sacar un mensaje en consola
        }

        // Cuando una torreta va a ser destruida, avisa antes de autodestruirse para que se le elimine de las listas del gestor del juego. 
        public void TowerDestroyed(Tower tower)
        {
            if (tower == null)
                throw new ArgumentNullException("No se ha pasado una torreta.");

            Towers.Remove(tower);
            // Podría sacar un mensaje en consola
        }
    }
}
