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
namespace es.ucm.fdi.iav.rts
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




        //private float costeMinotauro(Vertex n)
        //{
        //    //si al distancia de la casilla al minotauro es menor de 1, el coste sera el doble, si la distancia es menor de 0.5 diremos que es infinito y por tanto no se deberia de tener en cuenta, si no el coste es 0 
        //    float dist = ManhattanDist(n, GetNearestVertex(minotaruo.transform.position));

        //    if (dist < 0.5f)
        //        return Mathf.Infinity;
        //    else if (dist < 1.5f)
        //        return 5.0f;
        //    return 0;
        //}
    }
}