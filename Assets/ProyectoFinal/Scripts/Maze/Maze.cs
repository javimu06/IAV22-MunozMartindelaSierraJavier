using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Maze : MonoBehaviour
{
    public MazeCell cellPrefab;
    public MazePassage passagePrefab;
    public MazeWall wallPrefab;
    public GameObject powerups;
    public int powerupDensity;
    public GameObject key;
    public GameObject exit;
    public GameObject jail;
    public GameObject lever;


    public int scale;

    public IntVector2 size;
    public float generationStepDelay;

    private MazeCell[,] cells;
    private MazeCell firstCell;

    public List<GameObject> surfaces;

    public MazeCell GetCell(IntVector2 coordinates)
    {
        return cells[coordinates.x, coordinates.z];
    }

    public void Generate()
    {

        WaitForSeconds delay = new WaitForSeconds(generationStepDelay);
        cells = new MazeCell[size.x, size.z];
        List<MazeCell> activeCells = new List<MazeCell>();
        DoFirstGenerationStep(activeCells);
        while (activeCells.Count > 0)
        {
            DoNextGenerationStep(activeCells);
        }

        //Quitar muro entrada
        MazeWall[] children = firstCell.GetComponentsInChildren<MazeWall>();
        for (int i = 0; i < children.Length; ++i)
        {
            if (children[i].gameObject.GetComponent<MazeWall>().direction == MazeDirection.West)
            {
                children[i].gameObject.SetActive(false);
            }
        }

        //Poner sala extra
        MazeCell room = CreateCellNoList(new IntVector2(-1, 0));
        CreateWall(room, null, MazeDirection.West);
        CreateWall(room, null, MazeDirection.North);
        CreateWall(room, null, MazeDirection.South);
        GameObject exitDoor = Instantiate(exit) as GameObject;
        exitDoor.transform.position = new Vector3(-1 * scale, 0, 0);


        surfaces.Add(room.transform.GetChild(0).gameObject);
        lever = Instantiate(lever) as GameObject;
        lever.transform.position = new Vector3(0, 0, (size.z - 1) * scale);

        surfaces[0].GetComponent<NavMeshSurface>().BuildNavMesh();
    }

    private void DoFirstGenerationStep(List<MazeCell> activeCells)
    {
        activeCells.Add(CreateCell(RandomCoordinates));
    }

    private void DoNextGenerationStep(List<MazeCell> activeCells)
    {
        int currentIndex = activeCells.Count - 1;
        MazeCell currentCell = activeCells[currentIndex];
        if (currentCell.IsFullyInitialized)
        {
            activeCells.RemoveAt(currentIndex);
            return;
        }
        MazeDirection direction = currentCell.RandomUninitializedDirection;
        IntVector2 coordinates = currentCell.coordinates + direction.ToIntVector2();
        if (ContainsCoordinates(coordinates))
        {
            MazeCell neighbor = GetCell(coordinates);
            if (neighbor == null)
            {
                neighbor = CreateCell(coordinates);
                CreatePassage(currentCell, neighbor, direction);
                activeCells.Add(neighbor);
            }
            else
            {
                CreateWall(currentCell, neighbor, direction);
                // No longer remove the cell here.
            }
        }
        else
        {
            CreateWall(currentCell, null, direction);
            // No longer remove the cell here.
        }
    }

    private void CreatePassage(MazeCell cell, MazeCell otherCell, MazeDirection direction)
    {
        MazePassage passage = Instantiate(passagePrefab) as MazePassage;
        passage.Initialize(cell, otherCell, direction, scale);
        passage = Instantiate(passagePrefab) as MazePassage;
        passage.Initialize(otherCell, cell, direction.GetOpposite(), scale);
    }

    private void CreateWall(MazeCell cell, MazeCell otherCell, MazeDirection direction)
    {
        MazeWall wall = Instantiate(wallPrefab) as MazeWall;
        wall.Initialize(cell, otherCell, direction, scale);
        if (otherCell != null)
        {
            wall = Instantiate(wallPrefab) as MazeWall;
            wall.Initialize(otherCell, cell, direction.GetOpposite(), scale);
        }
    }

    private MazeCell CreateCell(IntVector2 coordinates)
    {
        MazeCell newCell = Instantiate(cellPrefab) as MazeCell;
        cells[coordinates.x, coordinates.z] = newCell;
        newCell.coordinates = coordinates;
        newCell.name = "Maze Cell " + coordinates.x * scale + ", " + coordinates.z * scale;
        newCell.transform.parent = transform;
        newCell.transform.position =
            new Vector3(coordinates.x * scale, 0f, coordinates.z * scale);
        newCell.transform.localScale = new Vector3(scale * newCell.transform.localScale.x, scale * newCell.transform.localScale.y, scale * newCell.transform.localScale.z);

        surfaces.Add(newCell.transform.GetChild(0).gameObject);

        if (newCell.name == "Maze Cell 0, 0")
            firstCell = newCell;

        if (newCell.name == "Maze Cell " + (size.x - 1) * scale + ", 0")
        {
            //Llave
            GameObject keyy = Instantiate(key) as GameObject;
            keyy.transform.position = new Vector3((size.x - 1) * scale, 0, 0);
            keyy.transform.position += new Vector3(0, 10, 0);
            //Jaula
            GameObject jaill = Instantiate(jail) as GameObject;
            jaill.transform.position = new Vector3((size.x - 1) * scale, -30, 0);
            GameManager.Instance.jail = jaill;

            GameManager.Instance.prisonCell = newCell.transform.GetChild(0).gameObject;
        }
        else
        {
            //Generar Powerup
            int poner = Random.Range(0, powerupDensity);
            if (poner == 0)
            {
                int power = Random.Range(0, 4);
                GameObject powerup = Instantiate(powerups.transform.GetChild(power).gameObject) as GameObject;
                powerup.transform.position = newCell.transform.position;
                powerup.transform.position += new Vector3(0, 10, 0);
            }
        }

        return newCell;
    }

    private MazeCell CreateCellNoList(IntVector2 coordinates)
    {
        MazeCell newCell = Instantiate(cellPrefab) as MazeCell;
        newCell.coordinates = coordinates;
        newCell.name = "Maze Cell " + coordinates.x * scale + ", " + coordinates.z * scale;
        newCell.transform.parent = transform;
        newCell.transform.position =
            new Vector3(coordinates.x * scale, 0f, coordinates.z * scale);
        newCell.transform.localScale = new Vector3(scale * newCell.transform.localScale.x, scale * newCell.transform.localScale.y, scale * newCell.transform.localScale.z);

        surfaces.Add(newCell.transform.GetChild(0).gameObject);

        return newCell;
    }

    public IntVector2 RandomCoordinates
    {
        get
        {
            return new IntVector2(Random.Range(0, size.x), Random.Range(0, size.z));
        }
    }

    public bool ContainsCoordinates(IntVector2 coordinate)
    {
        return coordinate.x >= 0 && coordinate.x < size.x && coordinate.z >= 0 && coordinate.z < size.z;
    }


}
