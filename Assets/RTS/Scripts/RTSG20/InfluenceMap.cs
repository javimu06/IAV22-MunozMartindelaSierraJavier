using System.Collections;
using System.Collections.Generic;
using UnityEngine;



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
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Random = UnityEngine.Random;

    public class InfluenceMap : Graph
    {
        public static InfluenceMap instance_;

        private void Awake()
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
        }

        public bool get8Vicinity = false;
        public float cellSizeX;
        public float cellSizeZ;

        public Vector2 BottomLeft;
        public Vector2 TopRight;

        [Range(0, Mathf.Infinity)]
        public float defaultCost = 1f;
        [Range(0, Mathf.Infinity)]
        public float maximumCost = Mathf.Infinity;

        public int numCols;
        public int numRows;
        public GameObject movepoint;

        GameObject[,] debugMap;
        public GameObject debugPrefabParent = new GameObject();
        public GameObject getDebugParent() { return debugPrefabParent; }
        GameObject debugPrefab;
        public int colorPotence;
        public void setPotence(int p) { colorPotence = p; }

        int teamColor;
        public void changeTeamColor(int color) { teamColor = color; }

        public InfluenceMap(Vector2 btomLeft, Vector2 topright, int xDiv, int zDiv)
        {
            BottomLeft = btomLeft; //= RTSScenarioManager.Instance.Scenario.GetPosition();
            TopRight = topright; //= RTSScenarioManager.Instance.Scenario.GetPosition() + RTSScenarioManager.Instance.Scenario.terrainData.size;
            numCols = xDiv;
            numRows = zDiv;

        }

        public void setDebugPrefab(GameObject dp) { debugPrefab = dp; LoadMap(); }

        private void LoadMap()
        {
            try
            {
                int j = 0;
                int i = 0;
                int id = 0;

                vertices = new List<Vertex>(numRows * numCols);
                neighbors = new List<List<Vertex>>(numRows * numCols);
                costs = new List<List<float>>(numRows * numCols);

                cellSizeX = Math.Abs(BottomLeft.x - TopRight.x) / numCols;
                cellSizeZ = Math.Abs(BottomLeft.y - TopRight.y) / numRows;

                debugMap = new GameObject[numRows, numCols];
                for (i = 0; i < numRows; i++)
                {
                    for (j = 0; j < numCols; j++)
                    {
                        id = GridToId(j, i);

                        Vertex v = new Vertex();    //!null
                        v.id = id;
                        vertices.Add(v);
                        neighbors.Add(new List<Vertex>());
                        costs.Add(new List<float>());

                        if (debugPrefab != null)
                        {
                            debugMap[j, i] = Instantiate(debugPrefab, new Vector3(i * cellSizeX + BottomLeft.x, 0, j * cellSizeZ + BottomLeft.y), Quaternion.identity) as GameObject;
                            debugMap[j, i].transform.localScale = new Vector3(cellSizeX, 1, cellSizeZ);
                            debugMap[j, i].transform.parent = debugPrefabParent.transform;
                        }
                    }
                }

                // now onto the neighbours
                for (i = 0; i < numRows; i++)
                {
                    for (j = 0; j < numCols; j++)
                    {
                        SetNeighbours(j, i);
                    }
                }


            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }


            //poner al minotaruo en el medio
            //minotaruo.transform.position = vertexObjs[GridToId(numCols / 2, numRows / 2)].transform.position + new Vector3(0, 0.5f, 0);
        }

        public void updateCost()
        {
            foreach (Vertex v in vertices)
                for (int i = 0; i < v.influence.Length; i++)
                    v.influence[i] = 0;
        }

        public void propagateInfluence(Vector3 position, int power, int team)
        {
            Vertex ori = GetNearestVertex(position);
            //Queue<Vertex> q = new Queue<Vertex>();
            //Vertex[] neighbours;
            //Vertex v;

            int[] previous = new int[vertices.Count];
            for (int i = 0; i < previous.Length; i++)
                previous[i] = -1;
            //previous[ori.id] = ori.id; // El vértice que tenga de previo a sí mismo, es el vértice origen

            //Vertex[] neighbours;
            //neighbours = GetNeighbours(ori);
            //foreach (Vertex n in neighbours)
            //{
            //    if (previous[n.id] != -1)
            //        continue;
            //    previous[n.id] = ori.id; // El vecino n tiene de 'padre' a v
            //    n.influence = power - 1;
            //    vertices.Add(n);
            //}
            //vertices.RemoveAt(0);


            auxFucionInfluence(ori, power, team, ref previous);
            //updateDebug();
            //q.Enqueue(ori);
            //int powerN = power;

            //while (powerN > 0)
            //{
            //    v = q.Dequeue();    //Iguala v a src                //Actualiza v al ultimo nodo
            //    //Si src (v) == dst, construimos el camino          //Si el nodo actual es el de destino construimos el camino

            //    v.influence += (powerN * team);

            //    powerN--;
            //    neighbours = GetNeighbours(v);
            //    foreach (Vertex n in neighbours)
            //    {
            //        if (previous[n.id] != -1)
            //            continue;
            //        previous[n.id] = v.id; // El vecino n tiene de 'padre' a v
            //        q.Enqueue(n);
            //    }
            //}
        }

        void auxFucionInfluence(Vertex v, int power, int team, ref int[] previous)
        {

            if (power > 0)
            {
                if (previous[v.id] == -1)
                {
                    v.influence[team] += (power);
                    v.lastInfluence = (power);
                    previous[v.id] = v.id; //es un mapa de visitados no de previus, y solamente lo sera si se le suma valor
                }
                else
                {
                    if (v.lastInfluence < power)
                    {
                        v.influence[team] += (power);
                        v.lastInfluence = power;
                    }
                }



                power--;
                Vertex[] neighbours = GetNeighbours(v);
                foreach (Vertex n in neighbours)
                {
                    //if (previous[n.id] != -1)
                    //    continue;
                    //previous[n.id] = v.id; // El vecino n tiene de 'padre' a v
                    auxFucionInfluence(n, power, team, ref previous);
                }
            }
        }



        public int GridToId(int x, int y)
        {
            return Math.Max(numRows, numCols) * y + x;
        }

        public Vector2 IdToGrid(int id)
        {
            Vector2 location = Vector2.zero;
            location.y = Mathf.Floor(id / numCols);
            location.x = Mathf.Floor(id / numCols);
            return location;
        }


        void divison(int x, int y, int maxx, int maxy, bool h)
        {
            if (h)
            {
                if (maxx <= 2)
                    return;

                int wallY = Random.Range(y, maxy);

                divison(x, wallY, maxx, maxy, !h);
                divison(x, y, maxx, wallY, !h);
            }
            else
            {
                if (maxy <= 2)
                    return;

                int wallX = Random.Range(x, maxx);

                divison(wallX, y, maxx, maxy, !h);
                divison(x, y, wallX, maxy, !h);

            }
        }



        protected void SetNeighbours(int x, int y, bool get8 = false)
        {
            int col = x;
            int row = y;
            int i, j;
            int vertexId = GridToId(x, y);
            neighbors[vertexId] = new List<Vertex>();
            costs[vertexId] = new List<float>();
            Vector2[] pos = new Vector2[0];
            if (get8)
            {
                pos = new Vector2[8];
                int c = 0;
                for (i = row - 1; i <= row + 1; i++)
                {
                    for (j = col - 1; j <= col; j++)
                    {
                        pos[c] = new Vector2(j, i);
                        c++;
                    }
                }
            }
            else
            {
                pos = new Vector2[4];
                pos[0] = new Vector2(col, row - 1);
                pos[1] = new Vector2(col - 1, row);
                pos[2] = new Vector2(col + 1, row);
                pos[3] = new Vector2(col, row + 1);
            }
            foreach (Vector2 p in pos)
            {
                i = (int)p.y;
                j = (int)p.x;
                if (i < 0 || j < 0)
                    continue;
                if (i >= numRows || j >= numCols)
                    continue;
                if (i == row && j == col)
                    continue;

                int id = GridToId(j, i);
                neighbors[vertexId].Add(vertices[id]);
                costs[vertexId].Add(defaultCost);
            }
        }

        public override Vertex GetNearestVertex(Vector3 position)
        {
            int col = (int)Math.Abs((BottomLeft.x - position.x) / cellSizeX);
            int row = (int)Math.Abs((BottomLeft.y - position.z) / cellSizeZ);
            Vector2 p = new Vector2(col, row);
            Queue<Vector2> queue = new Queue<Vector2>();
            queue.Enqueue(p);

            p = queue.Dequeue();
            col = (int)p.x;
            row = (int)p.y;
            int id = GridToId(col, row);

            return vertices[id];
        }

        public void updateDebug()
        {


            for (int i = 0; i < numRows; i++)
            {
                for (int j = 0; j < numCols; j++)
                {
                    int auxColor = GetInfluencePosVertex(vertices[GridToId(j, i)], teamColor) * colorPotence;
                    Color aux = Color.red;

                    if (auxColor > 0)
                    {
                        debugMap[i, j].GetComponent<MeshRenderer>().enabled = true;
                        aux = new Color(255f / 255f, (255 - auxColor) / 255f, 0f / 255f, 0.25f);
                    }
                    else if (auxColor < 0)
                    {
                        debugMap[i, j].GetComponent<MeshRenderer>().enabled = true;
                        aux = new Color(0f / 255f, (255 - Math.Abs(auxColor)) / 255f, 255f / 255f,0.25f);
                    }
                    else
                        debugMap[i, j].GetComponent<MeshRenderer>().enabled = false;
                    //aux = new Color(255 / 255f, 255 / 255f, 255 / 255f);

                    debugMap[i, j].GetComponent<MeshRenderer>().material.SetColor("_Color", aux);    //vertices[GridToId((int)debugMap[j, i].transform.position.x, (int)debugMap[j, i].transform.position.z)].influence
                }
            }
        }

        public int GetInfluencePos(Vector3 position, int team)
        {
            Vertex ori = GetNearestVertex(position);

            int result = ori.influence[team];

            for (int i = 0; i < ori.influence.Length; i++)
            {
                if (i != team)
                    result -= ori.influence[i];
            }

            return result;
        }

        public int GetInfluencePosVertex(Vertex ori, int team)
        {
            int result = ori.influence[team];

            for (int i = 0; i < ori.influence.Length; i++)
            {
                if (i != team)
                    result -= ori.influence[i];
            }

            return result;
        }

        public int valueArea(Vector3 position, int area, int team)
        {
            Vertex ori = GetNearestVertex(position);

            bool[] visited = new bool[vertices.Count];
            for (int i = 0; i < visited.Length; i++)
                visited[i] = false;


            return valueASreaFunc(ori, area, team, ref visited);
        }

        int valueASreaFunc(Vertex ori, int area, int team, ref bool[] visited)
        {
            //visited[ori.id] = true;
            int value = GetInfluencePosVertex(ori, team);

            //lso vecinos
            area--;
            if (area < 1)
                return value;
            Vertex[] neighbours = GetNeighbours(ori);
            foreach (Vertex n in neighbours)
            {
                if (visited[n.id])
                    continue;
                visited[n.id] = true; // El vecino n tiene de 'padre' a v
                value += valueASreaFunc(n, area, team, ref visited);
            }


            return value;
        }


    }
}
