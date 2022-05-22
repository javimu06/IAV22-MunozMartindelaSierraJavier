using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Maze mazePrefab;
    public GameObject Player;
    public GameObject Minotauro;
    public GameObject jail;

    public GameObject prisonCell;


    private Maze mazeInstance;

    public static GameManager Instance { get; private set; }
    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.

        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        BeginGame();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            RestartGame();
        }
    }

    private void BeginGame()
    {
        mazeInstance = Instantiate(mazePrefab) as Maze;
        mazeInstance.Generate();

        Player.transform.position = new Vector3(-1 * mazePrefab.scale, 10f, 0f);
        Minotauro.transform.position = new Vector3((mazeInstance.size.x - 1) * mazePrefab.scale, 0f, (mazeInstance.size.z - 1) * mazePrefab.scale);
    }

    private void RestartGame()
    {
        StopAllCoroutines();
        Destroy(mazeInstance.gameObject);
        BeginGame();
    }

    public void endGame()
    {
        Debug.Log("acuetate\n");
        Application.Quit();
    }
}
