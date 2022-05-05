/*    
    Obra original:
        Copyright (c) 2018 Packt
        Unity 2018 Artificial Intelligence Cookbook - Second Edition, by Jorge Palacios
        https://github.com/PacktPublishing/Unity-2018-Artificial-Intelligence-Cookbook-Second-Edition
        MIT License

    Modificaciones:
        Copyright (C) 2020-2022 Federico Peinado
        http://www.federicopeinado.com

        Este fichero forma parte del material de la asignatura Inteligencia Artificial para Videojuegos.
        Esta asignatura se imparte en la Facultad de Informática de la Universidad Complutense de Madrid (España).
        Contacto: email@federicopeinado.com
*/
namespace UCM.IAV.Navegacion
{

    using UnityEngine;
    using System.Collections.Generic;

    // Posibles algoritmos para buscar caminos en grafos
    public enum TesterGraphAlgorithm
    {
        BFS, DFS, ASTAR, HEURISTICA, ASTARCOSTE
    }

    //
    public class TesterGraph : MonoBehaviour
    {
        public Graph graph;
        public TesterGraphAlgorithm algorithm;
        public bool smoothPath;
        public string vertexTag = "Vertex"; // Etiqueta de un nodo normal
        public string obstacleTag = "Wall"; // Etiqueta de un obstáculo, tipo pared...
        public Color pathColor;
        [Range(0.1f, 1f)]
        public float pathNodeRadius = .3f;

        Camera mainCamera;
        GameObject srcObj;
        GameObject dstObj;
        public List<Vertex> path; // La variable con el camino calculado

        public static TesterGraph instance_;

        // Despertar inicializando esto
        void Awake()
        {
            if (instance_ == null)
            {
                instance_ = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Destroy(this.gameObject);
            }

            mainCamera = Camera.main;
            srcObj = null;
            dstObj = null;
            path = new List<Vertex>();
        }

        // Update is called once per frame
        void Update()
        {
            // El origen se marca haciendo click
            if (Input.GetMouseButtonDown(0))
            {
                srcObj = GetNodeFromScreen(Input.mousePosition);
            }
            // El destino simplemente poniendo el ratón encima
            dstObj = GetNodeFromScreen(Input.mousePosition);

            // Con la barra espaciadora se activa la búsqueda del camino mínimo
            if (Input.GetKeyDown(KeyCode.Space))
            {

                // Si hay ya camino calculado, la muestro en color blanco, y borro la variable con el camino
                if (path.Count != 0)
                {
                    ShowPath(path, Color.white);
                    path = new List<Vertex>();
                }
                switch (algorithm)
                {
                    case TesterGraphAlgorithm.ASTAR:
                        path = graph.GetPathAstar(srcObj, dstObj, null); // Se pasa la heurística
                        break;
                    default:
                    case TesterGraphAlgorithm.BFS:
                        path = graph.GetPathBFS(srcObj, dstObj);
                        break;
                    case TesterGraphAlgorithm.DFS:
                        path = graph.GetPathDFS(srcObj, dstObj);
                        break;
                    case TesterGraphAlgorithm.HEURISTICA:
                        path = graph.getPathHeuristic(srcObj, dstObj);
                        break;
                    case TesterGraphAlgorithm.ASTARCOSTE:
                        path = graph.GetPathAstarCost(srcObj, dstObj);
                        break;
                }
                if (smoothPath)
                    path = graph.Smooth(path); // Suavizar el camino, una vez calculado
            }
        }

        // Dibujado de artilugios en el editor
        // OJO, ESTO SÓLO SE PUEDE VER EN LA PESTAÑA DE SCENE DE UNITY
        public void OnDrawGizmos()
        {
            if (!Application.isPlaying)
                return;

            if (ReferenceEquals(graph, null))
                return;

            Vertex v;
            if (!ReferenceEquals(srcObj, null))
            {
                Gizmos.color = Color.green; // Verde es el nodo inicial
                v = graph.GetNearestVertex(srcObj.transform.position);
                Gizmos.DrawSphere(v.transform.position, pathNodeRadius);
            }
            if (!ReferenceEquals(dstObj, null))
            {
                Gizmos.color = Color.red; // Rojo es el color del nodo de destino
                v = graph.GetNearestVertex(dstObj.transform.position);
                Gizmos.DrawSphere(v.transform.position, pathNodeRadius);
            }
            int i;
            Gizmos.color = pathColor;
            for (i = 0; i < path.Count; i++)
            {
                v = path[i];
                Gizmos.DrawSphere(v.transform.position, pathNodeRadius);
                if (smoothPath && i != 0)
                {
                    Vertex prev = path[i - 1];
                    Gizmos.DrawLine(v.transform.position, prev.transform.position);

                }

            }
        }

        // Mostrar el camino calculado
        public void ShowPath(List<Vertex> path, Color color)
        {
            int i;
            for (i = 0; i < path.Count; i++)
            {
                Vertex v = path[i];
                Renderer r = v.GetComponent<Renderer>();
                if (ReferenceEquals(r, null))
                    continue;
                r.material.color = color;
            }
        }

        // Cuantificación, cómo traduce de posiciones del espacio (la pantalla) a nodos
        private GameObject GetNodeFromScreen(Vector3 screenPosition)
        {
            GameObject node = null;
            Ray ray = mainCamera.ScreenPointToRay(screenPosition);
            RaycastHit[] hits = Physics.RaycastAll(ray);
            foreach (RaycastHit h in hits)
            {
                if (!h.collider.CompareTag(vertexTag) && !h.collider.CompareTag(obstacleTag))
                    continue;
                node = h.collider.gameObject;
                break;
            }
            return node;
        }
    }
}
