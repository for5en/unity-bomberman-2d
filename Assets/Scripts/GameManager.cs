using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    private bool gameWait;

    public float squareSize = 2f;
    public float bombSpeed = 1f;
    public float explosionSpeed = 2f;
    public float colLifeSpan = 10f;
    public float explosionWaveStep = 0.08f;
    public float noColChance = 0.5f;

    public GameObject environmentPrefab;
    public GameObject playerPrefabA, playerPrefabB;
    public Vector3 playerGuiPositionA, playerGuiPositionB;

    public Camera mainCamera;

    private Environment environment;
    private Player playerA, playerB;
    private PlayerControls playerControlsA = new PlayerControls(Key.W, Key.A, Key.S, Key.D, Key.Space);
    private PlayerControls playerControlsB = new PlayerControls(Key.UpArrow, Key.LeftArrow, Key.DownArrow, Key.RightArrow, Key.Slash);
    private GameObject env_GO;

    public GameObject explosionPrefab;

    List<Map> maps = new List<Map>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        maps.Add(new Map(17, 15, @"
        #################
        #a.*.***##**.*..#
        #.##.#######.##.#
        #.#**.......**#*#
        #**#*###*###*#.*#
        ##*****#.#*****##
        ##*#.#*.*.*#.#*##
        #**.##*###*##.**#
        ##*#.#*.*.*#.#*##
        ##*****#.#*****##
        #**#*###*###*#.*#
        #.#**.......**#*#
        #.##.#######.##.#
        #..*.***##**.*.b#
        #################
        "));

        StartGame();
    }

    // Update is called once per frame
    void Update()
    {
        if(!gameWait && environment.gameEnded != 0)
        {
            gameWait = true;
            Invoke("StartGame", 3f);
        }
        if(Keyboard.current.escapeKey.wasPressedThisFrame) QuitGame();
    }

    void StartGame()
    {
        int n = Random.Range(0, maps.Count);

        Destroy(env_GO);

        env_GO = Instantiate(environmentPrefab);
        environment = env_GO.GetComponent<Environment>();
        environment.Init(this, maps[n].width, maps[n].height, maps[n].tileCode);

        GameObject playerA_GO = Instantiate(playerPrefabA);
        playerA = playerA_GO.GetComponent<Player>();
        playerA.Init(environment.playerPosA, environment, playerControlsA);

        GameObject playerB_GO = Instantiate(playerPrefabB);
        playerB = playerB_GO.GetComponent<Player>();
        playerB.Init(environment.playerPosB, environment, playerControlsB);

        environment.AddPlayerA(playerA, playerA_GO);
        environment.AddPlayerB(playerB, playerB_GO);
        gameWait = false;
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
