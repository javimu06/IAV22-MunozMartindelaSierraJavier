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
    using System.Collections;
    using System.Collections.Generic;




    /// <summary>
    /// Abstract class for graphs
    /// </summary>
    public abstract class Graph : MonoBehaviour
    {


        struct VertexCost
        {


            public Vertex v;
            public float cost;
            public float distOri;
            public VertexCost(Vertex v_, float c)
            {
                v = v_;
                cost = c;
                distOri = 0;
            }
            public VertexCost(Vertex v_, float c, float d)
            {
                v = v_;
                distOri = d;
                cost = c + d;
            }
        }


        public GameObject vertexPrefab;
        protected List<Vertex> vertices;
        protected List<List<Vertex>> neighbors;
        protected List<List<float>> costs;
        //protected Dictionary<int, int> instIdToId;

        //// this is for informed search like A*
        public delegate float Heuristic(Vertex a, Vertex b);

        // Used for getting path in frames
        public List<Vertex> path;
        public GameObject minotaruo;

        //public bool isFinished;

        public virtual void Start()
        {
            Load();
        }

        public virtual void Load() { }

        public virtual int GetSize()
        {
            if (ReferenceEquals(vertices, null))
                return 0;
            return vertices.Count;
        }

        public virtual Vertex GetNearestVertex(Vector3 position)
        {
            return null;
        }


        public virtual Vertex[] GetNeighbours(Vertex v)
        {
            if (ReferenceEquals(neighbors, null) || neighbors.Count == 0)
                return new Vertex[0];
            if (v.id < 0 || v.id >= neighbors.Count)
                return new Vertex[0];
            return neighbors[v.id].ToArray();
        }



        static int SortByScore(VertexCost p1, VertexCost p2)
        {
            return p1.cost.CompareTo(p2.cost);
        }


        // Encuentra caminos óptimos
        //DJISTRA SIN COSTES
        public List<Vertex> GetPathBFS(GameObject srcO, GameObject dstO)
        {
            if (srcO == null || dstO == null)
                return new List<Vertex>();
            Vertex[] neighbours;
            Queue<Vertex> q = new Queue<Vertex>();
            //Coge el centro de la casilla que contiene la coordenada donde se encuentran los Gameobjects
            Vertex src = GetNearestVertex(srcO.transform.position);
            Vertex dst = GetNearestVertex(dstO.transform.position);
            Vertex v;
            int[] previous = new int[vertices.Count];
            for (int i = 0; i < previous.Length; i++)
                previous[i] = -1;
            previous[src.id] = src.id; // El vértice que tenga de previo a sí mismo, es el vértice origen
            /*Por ejemplo
             -1 -1 -1 -1 -1
             -1  0 -1 -1 -1
             -1 -1 -1 -1 -1
             -1 -1 -1 -1 -1
             */
            q.Enqueue(src);
            while (q.Count != 0)
            {
                v = q.Dequeue();    //Iguala v a src                //Actualiza v al ultimo nodo
                //Si src (v) == dst, construimos el camino          //Si el nodo actual es el de destino construimos el camino
                if (ReferenceEquals(v, dst))
                {
                    return BuildPath(src.id, v.id, ref previous);
                }

                neighbours = GetNeighbours(v);
                foreach (Vertex n in neighbours)
                {
                    if (previous[n.id] != -1)
                        continue;
                    previous[n.id] = v.id; // El vecino n tiene de 'padre' a v
                    q.Enqueue(n);
                }
            }
            return new List<Vertex>();
        }

        // No encuentra caminos óptimos
        public List<Vertex> GetPathDFS(GameObject srcO, GameObject dstO)
        {
            if (srcO == null || dstO == null)
                return new List<Vertex>();
            Vertex src = GetNearestVertex(srcO.transform.position);
            Vertex dst = GetNearestVertex(dstO.transform.position);
            Vertex[] neighbours;
            Vertex v;
            int[] previous = new int[vertices.Count];
            for (int i = 0; i < previous.Length; i++)
                previous[i] = -1;
            previous[src.id] = src.id;
            Stack<Vertex> s = new Stack<Vertex>();
            s.Push(src);
            while (s.Count != 0)
            {
                v = s.Pop();
                if (ReferenceEquals(v, dst))
                {
                    return BuildPath(src.id, v.id, ref previous);
                }

                neighbours = GetNeighbours(v);
                foreach (Vertex n in neighbours)
                {
                    if (previous[n.id] != -1)
                        continue;
                    previous[n.id] = v.id;
                    s.Push(n);
                }
            }
            return new List<Vertex>();
        }

        public List<Vertex> getPathHeuristic(GameObject srcO, GameObject dstO, Heuristic h = null)
        {

            if (srcO == null || dstO == null)
                return new List<Vertex>();
            Vertex[] neighbours;
            List<VertexCost> q = new List<VertexCost>();
            //Coge el centro de la casilla que contiene la coordenada donde se encuentran los Gameobjects
            Vertex src = GetNearestVertex(srcO.transform.position);
            Vertex dst = GetNearestVertex(dstO.transform.position);
            Vertex v;
            int[] previous = new int[vertices.Count];
            for (int i = 0; i < previous.Length; i++)
                previous[i] = -1;
            previous[src.id] = src.id; // El vértice que tenga de previo a sí mismo, es el vértice origen
            /*Por ejemplo
             -1 -1 -1 -1 -1
             -1  0 -1 -1 -1
             -1 -1 -1 -1 -1
             -1 -1 -1 -1 -1
             */
            q.Add(new VertexCost(src, 0));
            while (q.Count != 0)
            {
                v = q[0].v; //Iguala v a src                //Actualiza v al ultimo nodo
                q.RemoveAt(0);
                //Si src (v) == dst, construimos el camino          //Si el nodo actual es el de destino construimos el camino
                if (ReferenceEquals(v, dst))
                {
                    return BuildPath(src.id, v.id, ref previous);
                }

                neighbours = GetNeighbours(v);
                foreach (Vertex n in neighbours)
                {
                    if (previous[n.id] != -1)
                        continue;
                    previous[n.id] = v.id; // El vecino n tiene de 'padre' a v
                    q.Add(new VertexCost(n, EuclidDist(n, dst)));
                    //ordenar por distancia a objetivo
                    q.Sort(SortByScore);
                }
            }
            return new List<Vertex>();
        }

        //con casillas sin coste
        public List<Vertex> GetPathAstar(GameObject srcO, GameObject dstO, Heuristic h = null)
        {
            if (srcO == null || dstO == null)
                return new List<Vertex>();
            Vertex[] neighbours;
            List<VertexCost> q = new List<VertexCost>();
            //Coge el centro de la casilla que contiene la coordenada donde se encuentran los Gameobjects
            Vertex src = GetNearestVertex(srcO.transform.position);
            Vertex dst = GetNearestVertex(dstO.transform.position);
            Vertex v;
            int[] previous = new int[vertices.Count];
            for (int i = 0; i < previous.Length; i++)
                previous[i] = -1;
            previous[src.id] = src.id;
            q.Add(new VertexCost(src, 0, 0));
            while (q.Count != 0)
            {
                v = q[0].v; //Iguala v a src                //Actualiza v al ultimo nodo

                //Si src (v) == dst, construimos el camino          //Si el nodo actual es el de destino construimos el camino
                if (ReferenceEquals(v, dst))
                {
                    return BuildPath(src.id, v.id, ref previous);
                }

                neighbours = GetNeighbours(v);
                foreach (Vertex n in neighbours)
                {
                    if (previous[n.id] != -1)
                        continue;
                    previous[n.id] = v.id; // El vecino n tiene de 'padre' a v
                    q.Add(new VertexCost(n, EuclidDist(n, dst), q[0].distOri + 1));
                    //ordenar por distancia a objetivo
                    q.Sort(SortByScore);
                }
                q.RemoveAt(0);
            }
            return new List<Vertex>();
        }


        public List<Vertex> GetPathAstarCost(GameObject srcO, GameObject dstO, Heuristic h = null)
        {
            if (srcO == null || dstO == null)
                return new List<Vertex>();
            Vertex[] neighbours;
            List<VertexCost> q = new List<VertexCost>();
            //Coge el centro de la casilla que contiene la coordenada donde se encuentran los Gameobjects
            Vertex src = GetNearestVertex(srcO.transform.position);
            Vertex dst = GetNearestVertex(dstO.transform.position);
            Vertex v;
            int[] previous = new int[vertices.Count];
            for (int i = 0; i < previous.Length; i++)
                previous[i] = -1;
            previous[src.id] = src.id;
            q.Add(new VertexCost(src, 0, 0));
            while (q.Count != 0)
            {
                v = q[0].v; //Iguala v a src                //Actualiza v al ultimo nodo

                //Si src (v) == dst, construimos el camino          //Si el nodo actual es el de destino construimos el camino
                if (ReferenceEquals(v, dst))
                {
                    return BuildPath(src.id, v.id, ref previous);
                }

                neighbours = GetNeighbours(v);
                foreach (Vertex n in neighbours)
                {
                    if (previous[n.id] != -1)
                        continue;
                    previous[n.id] = v.id; // El vecino n tiene de 'padre' a v
                    q.Add(new VertexCost(n, EuclidDist(n, dst), q[0].distOri + 1 + costeMinotauro(n)));
                    //ordenar por distancia a objetivo
                    q.Sort(SortByScore);
                }
                q.RemoveAt(0);
            }
            return new List<Vertex>();
        }


        private float costeMinotauro(Vertex n)
        {
            //si al distancia de la casilla al minotauro es menor de 1, el coste sera el doble, si la distancia es menor de 0.5 diremos que es infinito y por tanto no se deberia de tener en cuenta, si no el coste es 0 
            float dist = ManhattanDist(n, GetNearestVertex(minotaruo.transform.position));

            if (dist < 0.5f)
                return Mathf.Infinity;
            else if (dist < 1.5f)
                return 5.0f;
            return 0;
        }


        public List<Vertex> Smooth(List<Vertex> inputPath)
        {
            bool reduced;
            do
            {
                reduced = false;
                for (int ix = 0; ix + 2 < inputPath.Count; ix++)
                {
                    Vertex a = inputPath[ix];
                    Vertex b = inputPath[ix + 1];
                    Vertex c = inputPath[ix + 2];
                    // if travelling via b will result in a equal or longer route, don't bother.
                    if (Vector3.Distance(a.transform.position, c.transform.position) <= Vector3.Distance(a.transform.position, b.transform.position) + Vector3.Distance(b.transform.position, c.transform.position) && isPathClear(a, c))
                    {
                        inputPath.RemoveAt(ix + 1);
                        reduced = true;
                    }
                }
            } while (reduced);

            return inputPath;

        }

        bool isPathClear(Vertex a, Vertex c)
        {
            Vector3 directionAux = (c.transform.position - a.transform.position).normalized * Vector3.Distance(a.transform.position, c.transform.position) + new Vector3(0, 0.3f, 0);
            Ray theRay = new Ray(a.transform.position, directionAux);

            if (Physics.Raycast(theRay, out RaycastHit hit))
            {
                //The ray cast failed, add the last node that passed to
                // the output list.
                if (hit.collider.tag == "Wall")
                    return false;
                else
                {
                    //Debug.DrawRay(a.transform.position, directionAux * Vector3.Distance(a.transform.position, c.transform.position), Color.red, 50);
                    return true;
                }
            }
            return true;
        }

        // Reconstruir el camino, dando la vuelta a la lista de nodos 'padres' /previos que hemos ido anotando
        private List<Vertex> BuildPath(int srcId, int dstId, ref int[] prevList)
        {
            List<Vertex> path = new List<Vertex>();
            int prev = dstId;
            do
            {
                path.Add(vertices[prev]);
                prev = prevList[prev];
            } while (prev != srcId);
            return path;
        }

        // Sí me parece razonable que la heurística trabaje con la escena de Unity
        // Heurística de distancia euclídea
        public float EuclidDist(Vertex a, Vertex b)
        {
            Vector3 posA = a.transform.position;
            Vector3 posB = b.transform.position;
            return Vector3.Distance(posA, posB);
        }

        // Heurística de distancia Manhattan
        public float ManhattanDist(Vertex a, Vertex b)
        {
            Vector3 posA = a.transform.position;
            Vector3 posB = b.transform.position;
            return Mathf.Abs(posA.x - posB.x) + Mathf.Abs(posA.y - posB.y);
        }
    }
}