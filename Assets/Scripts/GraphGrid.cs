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
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Random = UnityEngine.Random;

    public class GraphGrid : Graph
    {
        public static GraphGrid instance_;

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

        public GameObject obstaclePrefab;
        public GameObject salida;
        public Material salidaMat;

        public string mapsDir = "Maps"; // Directorio por defecto
        public string mapName = "arena.map"; // Fichero por defecto
        public bool get8Vicinity = false;
        public float cellSize = 1f;
        [Range(0, Mathf.Infinity)]
        public float defaultCost = 1f;
        [Range(0, Mathf.Infinity)]
        public float maximumCost = Mathf.Infinity;

        public int numCols;
        public int numRows;
        GameObject[] vertexObjs;
        public bool[,] mapVertices;
        public GameObject player;
        public GameObject movepoint;

        public int GridToId(int x, int y)
        {
            return Math.Max(numRows, numCols) * y + x;
        }

        public Vector2 IdToGrid(int id)
        {
            Vector2 location = Vector2.zero;
            location.y = Mathf.Floor(id / numCols);
            location.x = Mathf.Floor(id % numCols);
            return location;
        }

        private void LoadMap(string filename)
        {
            string path = Application.dataPath + "/" + mapsDir + "/" + filename;
            try
            {
                StreamReader strmRdr = new StreamReader(path);
                using (strmRdr)
                {
                    int j = 0;
                    int i = 0;
                    int id = 0;
                    string line;

                    Vector3 position = Vector3.zero;
                    Vector3 scale = Vector3.zero;
                    line = strmRdr.ReadLine();// non-important line
                    line = strmRdr.ReadLine();// height
                    numRows = int.Parse(line.Split(' ')[1]);
                    line = strmRdr.ReadLine();// width
                    numCols = int.Parse(line.Split(' ')[1]);
                    line = strmRdr.ReadLine();// "map" line in file

                    vertices = new List<Vertex>(numRows * numCols);
                    neighbors = new List<List<Vertex>>(numRows * numCols);
                    costs = new List<List<float>>(numRows * numCols);
                    vertexObjs = new GameObject[numRows * numCols];
                    mapVertices = new bool[numRows, numCols];

                    for (i = 0; i < numRows; i++)
                    {
                        line = strmRdr.ReadLine();
                        for (j = 0; j < numCols; j++)
                        {
                            bool isGround = false;
                            if (line[j] == '.' || line[j] == 'S')
                                isGround = true;

                            mapVertices[i, j] = isGround;
                            position.x = j * cellSize;
                            position.z = i * cellSize;
                            id = GridToId(j, i);
                            if (isGround)
                            {
                                vertexObjs[id] = Instantiate(vertexPrefab, position, Quaternion.identity) as GameObject;
                            }
                            else
                                vertexObjs[id] = Instantiate(obstaclePrefab, position, Quaternion.identity) as GameObject;

                            if (line[j] == 'S')
                            {
                                salida = vertexObjs[id];
                                salida.GetComponent<MeshRenderer>().material = salidaMat;
                                player.transform.position = vertexObjs[id].transform.position + new Vector3(0, 0.5f, 1.0f);
                                movepoint.transform.position = player.transform.position;
                            }

                            vertexObjs[id].name = vertexObjs[id].name.Replace("(Clone)", id.ToString());
                            Vertex v = vertexObjs[id].AddComponent<Vertex>();
                            v.id = id;
                            vertices.Add(v);
                            neighbors.Add(new List<Vertex>());
                            costs.Add(new List<float>());
                            float y = vertexObjs[id].transform.localScale.y;
                            scale = new Vector3(cellSize, y, cellSize);
                            vertexObjs[id].transform.localScale = scale;
                            vertexObjs[id].transform.parent = gameObject.transform;
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
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }


            //poner al minotaruo en el medio



            minotaruo.transform.position = vertexObjs[GridToId(numCols / 2, numRows / 2)].transform.position + new Vector3(0, 0.5f, 0);
        }

        //private bool[,] CarvePath(bool[,] maze, Vector2 wallIdx, int isHorizontal, int casilla)
        //{
        //    if (isHorizontal == 1)
        //    {
        //            maze[(int)wallIdx.x, (int)wallIdx.y + casilla - 1] = false;
        //    }
        //    else
        //    {
        //            maze[(int)wallIdx.x + casilla - 1, (int)wallIdx.y] = false;
        //    }
        //    return maze;
        //}
        //private bool[,] BuildWall(bool[,] maze, int wallIdx, int isHorizontal, int casillas, Vector2 offset)
        //{
        //    if (isHorizontal == 1)
        //    {

        //        for (int i = 0; i < casillas; i++)
        //        {
        //            maze[(int)offset.x + wallIdx, (int)offset.y + i] = true;
        //        }

        //    }
        //    else
        //    {
        //        for (int i = 0; i < casillas; i++)
        //        {
        //            maze[(int)offset.x + i, (int)offset.y + wallIdx] = true;
        //        }

        //    }
        //    return maze;
        //}

        //private bool[,] RecursiveDivision(bool[,] maze, int width, int height, Vector2 offSet) {
        //    // Recursion Termination
        //    if (width - (int)offSet.x < 2 || height - (int)offSet.y < 2)
        //        return maze;

        //    // Randomly build the wall either horizontally or vertically
        //    int isHorizontal = 1;// Random.Range(0, 2);

        //    // Randomly select the position to build the wall (disconnect cells along the line)
        //    int wallIdx = isHorizontal == 1 ? Random.Range(2 + (int)offSet.x, height - 2) : Random.Range(2 + (int)offSet.y, width - 2);


        //    //if (maze[(int)offSet.x, (int)offSet.y] == true)
        //    //    return maze;
        //    maze = BuildWall(maze , wallIdx, isHorizontal, isHorizontal == 1 ? width : height, offSet);


        //    for (int i = 0; i < Random.Range(0,3); i++)
        //    {

        //        // Randomly select the position to carve a unique path within this wall
        //        int pathIdx = Random.Range(0, isHorizontal == 1 ? width : height);
        //      //  maze = CarvePath(maze, wallIdx, isHorizontal, pathIdx);
        //    }

        //    // Recurse on sub areas
        //    if (isHorizontal == 1)
        //    {
        //        // Top and Bottom areas
        //        maze = RecursiveDivision(maze, width, wallIdx - 1, offSet);

        //        Vector2 aux = offSet;
        //        aux.x += wallIdx + 1;
        //        maze = RecursiveDivision(maze, width, height - wallIdx, aux);
        //    }
        //    else
        //    {
        //        // Left and Right areas
        //        maze = RecursiveDivision(maze, wallIdx - 1, height, offSet);
        //        Vector2 aux = offSet;
        //        aux.y += wallIdx + 1;
        //        maze = RecursiveDivision(maze, width - wallIdx, height, aux);
        //    }

        //    return maze;
        //}

        //private void GenerateMap(int widht, int height)
        //{
        //    numRows = widht;
        //    numCols = height;

        //    vertices = new List<Vertex>(numRows * numCols);
        //    neighbors = new List<List<Vertex>>(numRows * numCols);
        //    costs = new List<List<float>>(numRows * numCols);
        //    vertexObjs = new GameObject[numRows * numCols];
        //    mapVertices = new bool[numRows, numCols];

        //    mapVertices = RecursiveDivision(mapVertices, numRows, numCols, new Vector2(0, 0));

        //    int j = 0;
        //    int i = 0;
        //    int id = 0;


        //    Vector3 position = Vector3.zero;
        //    Vector3 scale = Vector3.zero;

        //    for (i = 0; i < numRows; i++)
        //    {
        //        for (j = 0; j < numCols; j++)
        //        {
        //            bool isGround = !mapVertices[i, j];
        //            position.x = j * cellSize;
        //            position.z = i * cellSize;
        //            id = GridToId(j, i);
        //            if (isGround)
        //                vertexObjs[id] = Instantiate(vertexPrefab, position, Quaternion.identity) as GameObject;
        //            else
        //                vertexObjs[id] = Instantiate(obstaclePrefab, position, Quaternion.identity) as GameObject;
        //            vertexObjs[id].name = vertexObjs[id].name.Replace("(Clone)", id.ToString());
        //            Vertex v = vertexObjs[id].AddComponent<Vertex>();
        //            v.id = id;
        //            vertices.Add(v);
        //            neighbors.Add(new List<Vertex>());
        //            costs.Add(new List<float>());
        //            float y = vertexObjs[id].transform.localScale.y;
        //            scale = new Vector3(cellSize, y, cellSize);
        //            vertexObjs[id].transform.localScale = scale;
        //            vertexObjs[id].transform.parent = gameObject.transform;
        //        }
        //    }

        //    // now onto the neighbours
        //    for (i = 0; i < numRows; i++)
        //    {
        //        for (j = 0; j < numCols; j++)
        //        {
        //            SetNeighbours(j, i);
        //        }
        //    }




        //}

        void initMaze(int x, int y)
        {
            mapVertices = new bool[x, y];

            for (int i = 0; i < 20; i++)
                for (int j = 0; j < 20; j++)
                    mapVertices[i, j] = false;

            for (int i = 0; i < 20; i++)
                mapVertices[0, i] = true;

            for (int i = 0; i < 20; i++)
                mapVertices[x - 1, i] = true;

            for (int i = 0; i < 20; i++)
                mapVertices[i, 0] = true;

            for (int i = 0; i < 20; i++)
                mapVertices[i, y - 1] = true;

            divison(2, 2, x - 2, y - 2, true);

        }


        void drawVWall(int minY, int maxY, int x)
        {
            int passage = Random.Range(minY, maxY);

            for (int i = minY; i <= maxY; i++)
                if (i != passage)
                    mapVertices[i, x] = true;
        }

        void drawHWall(int minX, int maxX, int y)
        {
            int passage = Random.Range(minX, maxX);

            for (int i = minX; i <= maxX; i++)
                if (i != passage)
                    mapVertices[y, i] = true;
        }

        void divison(int x, int y, int maxx, int maxy, bool h)
        {
            if (h)
            {
                if (maxx <= 2)
                    return;

                int wallY = Random.Range(y, maxy);
                drawHWall(x, maxx, wallY);

                divison(x, wallY, maxx, maxy, !h);
                divison(x, y, maxx, wallY, !h);
            }
            else
            {
                if (maxy <= 2)
                    return;

                int wallX = Random.Range(x, maxx);
                drawVWall(y, maxy, wallX);

                divison(wallX, y, maxx, maxy, !h);
                divison(x, y, wallX, maxy, !h);

            }
        }


        public override void Load()
        {

            //initMaze(30, 30);
            //chargeMap(30,30);

            //GenerateMap(30,30);
            LoadMap(mapName);
        }

        private void chargeMap(int xx, int yy)
        {
            numRows = xx;
            numCols = yy;

            vertices = new List<Vertex>(numRows * numCols);
            neighbors = new List<List<Vertex>>(numRows * numCols);
            costs = new List<List<float>>(numRows * numCols);
            vertexObjs = new GameObject[numRows * numCols];
            mapVertices = new bool[numRows, numCols];


            int j = 0;
            int i = 0;
            int id = 0;


            Vector3 position = Vector3.zero;
            Vector3 scale = Vector3.zero;

            for (i = 0; i < numRows; i++)
            {
                for (j = 0; j < numCols; j++)
                {
                    bool isGround = !mapVertices[i, j];
                    position.x = j * cellSize;
                    position.z = i * cellSize;
                    id = GridToId(j, i);
                    if (isGround)
                        vertexObjs[id] = Instantiate(vertexPrefab, position, Quaternion.identity) as GameObject;
                    else
                        vertexObjs[id] = Instantiate(obstaclePrefab, position, Quaternion.identity) as GameObject;
                    vertexObjs[id].name = vertexObjs[id].name.Replace("(Clone)", id.ToString());
                    Vertex v = vertexObjs[id].AddComponent<Vertex>();
                    v.id = id;
                    vertices.Add(v);
                    neighbors.Add(new List<Vertex>());
                    costs.Add(new List<float>());
                    float y = vertexObjs[id].transform.localScale.y;
                    scale = new Vector3(cellSize, y, cellSize);
                    vertexObjs[id].transform.localScale = scale;
                    vertexObjs[id].transform.parent = gameObject.transform;
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
                if (!mapVertices[i, j])
                    continue;
                int id = GridToId(j, i);
                neighbors[vertexId].Add(vertices[id]);
                costs[vertexId].Add(defaultCost);
            }
        }

        public override Vertex GetNearestVertex(Vector3 position)
        {
            int col = (int)(position.x / cellSize);
            int row = (int)(position.z / cellSize);
            Vector2 p = new Vector2(col, row);
            List<Vector2> explored = new List<Vector2>();
            Queue<Vector2> queue = new Queue<Vector2>();
            queue.Enqueue(p);
            do
            {
                p = queue.Dequeue();
                col = (int)p.x;
                row = (int)p.y;
                int id = GridToId(col, row);
                if (mapVertices[row, col])  //Si no tiene colision, returneas el centro de la casilla que contiene esa coordenada
                    return vertices[id];

                if (!explored.Contains(p))
                {
                    explored.Add(p);
                    int i, j;
                    for (i = row - 1; i <= row + 1; i++)
                    {
                        for (j = col - 1; j <= col + 1; j++)
                        {
                            if (i < 0 || j < 0)
                                continue;
                            if (j >= numCols || i >= numRows)
                                continue;
                            if (i == row && j == col)
                                continue;
                            queue.Enqueue(new Vector2(j, i));
                        }
                    }
                }
            } while (queue.Count != 0);
            return null;
        }

    }
}
