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
using UnityEngine.EventSystems;

namespace es.ucm.fdi.iav.rts
{
    /* 
    * El controlador del jugador permite ofrecer una interfaz al jugador humano para que mande �rdenes a uno de los ej�rcitos del juego.
    * Se intenta que sea posible JUGAR como en un RTS normal, pinchando en las unidades para seleccionarlas y luego sobre el suelo para mandarles moverse.
    * Se trata de una clase abstracta para todos los controladores de bots espec�ficos que programemos.
    * 
    * Posibles mejoras:
    * - Que pueda haber 2 jugadores a la vez y que funcione bien con un mismo rat�n (har�a falta un bot�n para cambiar el turno o algo as�).
    * - A�adir un bot�n para realizar un movimiento masivo (ej. ataque con todas las unidades)
    * - En lugar de usar OnGUI, crear una interfaz de usuario m�s moderna. 
    * - Mantener y mostrar en formato de porcentaje o barra la cantidad de salud que le queda al jugador, n�mero de unidades, etc. Antes se mostraba as�. GUILayout.Label(string.Format("Enemy Health: {0}%", Mathf.RoundToInt((currentHealth / startEnemyHealth) * 100)), labelStyle);
    * - Permitir que se pueda cambiar desde el inspector el tama�o de fuente y los colores, poni�ndolos diferentes para cada tipo de jugador.
    * - Adaptar la interfaz para N jugadores (mantener botones y s�lo ir cambiando el �ndice de a quien le toca).
    */
    public class RTSPlayerController : RTSController
    { 
        // El estilo para las etiquetas de la interfaz
        private GUIStyle _labelStyle;
        private GUIStyle _labelSmallStyle;

        // Mi propio �ndice (parecido al n�mero de jugador que soy)
        private int _index;

        // La unidad seleccionada (de mi ej�rcito), para darle orden de moverse
        private Unit selectedUnit;

        private Ray ray;

        // Despertar el controlador del jugador, las estructuras internas que necesita
        private void Awake()
        {
            // Aumenta el tama�o y cambia el color de fuente de letra para OnGUI (blanco para los humanos)
            _labelStyle = new GUIStyle();
            _labelStyle.fontSize = 16;
            _labelStyle.normal.textColor = Color.white;

            _labelSmallStyle = new GUIStyle();
            _labelSmallStyle.fontSize = 11;
            _labelSmallStyle.normal.textColor = Color.white;
        }

        // Iniciar el controlador del jugador, llamando a los otros objetos que sean necesarios
        private void Start()
        {
            _index = RTSGameManager.Instance.GetIndex(this);
        }

        // Dibuja la interfaz gr�fica de usuario para que la utilice el jugador humano
        private void OnGUI()
        {
            // Abrimos un �rea de distribuci�n arriba y a la izquierda (si el �ndice del controlador es par) o a la derecha (si el �ndice es impar), con contenido en vertical
            float areaWidth = 150; 
            float areaHeight = 250;
            if (_index % 2 == 0)
                GUILayout.BeginArea(new Rect(0, 0, areaWidth, areaHeight));
            else
                GUILayout.BeginArea(new Rect(Screen.width - areaWidth, 0, Screen.width, areaHeight));
            GUILayout.BeginVertical(); 

            // Lista las variables importantes como el �ndice del jugador y su cantidad de dinero
            GUILayout.Label("[ C" + _index + " ] " + RTSGameManager.Instance.GetMoney(_index) + " solaris", _labelStyle); 

            // Botones que permite al jugador humano hacer ciertas acciones con su ej�rcito

            if (GUILayout.Button("Create Extractor", GUILayout.ExpandWidth(false))) {
                // S�lo si tengo al menos una instalaci�n base, esto va a funcionar, si no no har� nada
                List<BaseFacility> facilities = RTSGameManager.Instance.GetBaseFacilities(_index);
                if (facilities.Count > 0)
                    // Se pasa una instalaci�n base cualquiera como par�metro (aleatoria, obviamente nuestra) 
                    RTSGameManager.Instance.CreateUnit(this, facilities[Random.Range(0, facilities.Count - 1)], RTSGameManager.UnitType.EXTRACTION); 
            }
            if (GUILayout.Button("Create Explorator", GUILayout.ExpandWidth(false)))
            {
                // S�lo si tengo al menos una instalaci�n base, esto va a funcionar, si no no har� nada
                List<BaseFacility> facilities = RTSGameManager.Instance.GetBaseFacilities(_index);
                if (facilities.Count > 0)
                    // Se pasa una instalaci�n base cualquiera como par�metro (aleatoria, obviamente nuestra) 
                    RTSGameManager.Instance.CreateUnit(this, facilities[Random.Range(0, facilities.Count - 1)], RTSGameManager.UnitType.EXPLORATION);
            }
            if (GUILayout.Button("Create Destructor", GUILayout.ExpandWidth(false)))
            {
                // S�lo si tengo al menos una instalaci�n base, esto va a funcionar, si no no har� nada
                List<BaseFacility> facilities = RTSGameManager.Instance.GetBaseFacilities(_index);
                if (facilities.Count > 0)
                    // Se pasa una instalaci�n base cualquiera como par�metro (aleatoria, obviamente nuestra) 
                    RTSGameManager.Instance.CreateUnit(this, facilities[Random.Range(0, facilities.Count - 1)], RTSGameManager.UnitType.DESTRUCTION);
            }

            if (selectedUnit != null)
                // Una etiqueta para indicar la unidad seleccionada, si la hay
                GUILayout.Label(selectedUnit.gameObject.name + " selected", _labelSmallStyle);

            // Cerramos el �rea de distribuci�n con contenido en vertical
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        // Actualiza el componente y est� pendiente de detectar pulsaciones de rat�n para seleccionar o mover unidades
        void Update()
        {
            // Podemos dibujar el rayo para verlo en la escena
            //Debug.DrawRay(ray.origin, ray.direction * 500, Color.yellow);

            // Si pulsamos el bot�n del rat�n y no estamos sobre ning�n elemento de la GUI
            if (Input.GetMouseButtonDown(0)) //&& !EventSystem.current.IsPointerOverGameObject()
            {
                // NO FUNCIONA, NI METIENDO UN EVENTSYSTEM EN EL ESCENARIO (ver si los botones valen para ello
                if (EventSystem.current.IsPointerOverGameObject())
                    Debug.Log(EventSystem.current.currentSelectedGameObject);

                RaycastHit hitInfo = new RaycastHit();
                ray = RTSScenarioManager.Instance.Cameras[RTSScenarioManager.Instance.CameraIndex].ScreenPointToRay(Input.mousePosition);
                bool hit = Physics.Raycast(ray, out hitInfo);

                if (hit)
                {
                    //Debug.Log("Hit!");
                    Unit clickedUnit = hitInfo.transform.gameObject.GetComponent<Unit>();
                    // Si he pinchado en una de mis unidades, siempre es para seleccionarla
                    if (clickedUnit != null && clickedUnit.GetControllerIndex() == _index)
                    {
                        //Debug.Log("My Unit!");
                        selectedUnit = clickedUnit;
                    }
                    else // Si no, en caso de tener unidad seleccionada, la mando para all�
                    {
                        if (selectedUnit != null)
                        {
                            //Debug.Log("Sending Move...");
                            // Si he dado al collider del suelo (terreno), mando la unidad a un punto... si no, a la transformada
                            if (hitInfo.collider.gameObject.GetComponent<Terrain>())
                                RTSGameManager.Instance.MoveUnit(this, selectedUnit, hitInfo.point);
                            else
                                RTSGameManager.Instance.MoveUnit(this, selectedUnit, hitInfo.collider.transform);
                        }
                            
                    }
                }
            }
        }
    } 
}