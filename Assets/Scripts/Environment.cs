using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Environment : MonoBehaviour
{
    public float bombSpeed = 1f;
    public float explosionSpeed = 2f;
    public float colLifeSpan = 10f;
    public float explosionWaveStep = 0.08f;
    public float noColChance = 0.5f;
    public int gameEnded = 0;

    public GameObject wallPrefab;
    public GameObject breakablePrefab;
    public GameObject emptyPrefab;
    public GameObject explosionPrefab;

    public GameObject bombPrefab;
    public GameObject bombRangePrefab;
    public GameObject bombAmountPrefab;
    public GameObject healthPrefab;
    public GameObject speedPrefab;

    private int mapWidth;
    private int mapHeight;
    public float squareSize;

    private TileType[][] tileMap;

    private GameObject[][] effectObjects;
    private GameObject[][] tileObjects;

    public Vector3 playerPosA;
    public Vector3 playerPosB;
    private GameObject playerA_GO;
    private GameObject playerB_GO;

    private List<Player> players = new List<Player>();
    private int playerAmount = 0;

    Vector3 playerGuiPositionA, playerGuiPositionB;
    public GameObject playerGuiObjAPrefab;
    public GameObject playerGuiObjBPrefab;
    private GameObject playerGuiObjA;
    private GameObject playerGuiObjB;
    private PlayerGui playerGuiA, playerGuiB;

    private AudioSource audioSource;
    public AudioClip explosionClip;
    public AudioClip collectClip;
    public AudioClip[] musicTracks;
    private int currentTrack = 0;

    /*public void Init(float squareSize, int mapWidth, int mapHeight)
    {
        this.squareSize = squareSize;
        wallOffset = squareSize / 3f;
        xOffset = new Vector3(wallOffset, 0, 0);
        yOffset = new Vector3(0, wallOffset, 0);

        this.mapWidth = mapWidth;
        this.mapHeight = mapHeight;
    
        tileMap = new TileType[mapWidth][];
        effectMap = new EffectType[mapWidth][];
        tileObjects = new GameObject[mapWidth][];
        effectObjects = new GameObject[mapWidth][];

        for(int i=0; i<mapHeight; i++)
        {
            tileMap[i] = new TileType[mapHeight];
            effectMap[i] = new EffectType[mapHeight];
            tileObjects[i] = new GameObject[mapHeight];
            effectObjects[i] = new GameObject[mapHeight];
            for(int j=0; j<mapWidth; j++) effectMap[j][i] = EffectType.None;
        }
    }*/

    void PlayCurrentTrack()
    {
        audioSource.clip = musicTracks[currentTrack];
        audioSource.Play();
    }

    void NextTrack()
    {
        currentTrack = (currentTrack + 1) % musicTracks.Length;
        PlayCurrentTrack();
    }

    public void Init(GameManager game, int mapWidth, int mapHeight, string tileCode)
    {
        this.mapWidth = mapWidth;
        this.mapHeight = mapHeight;

        squareSize = game.squareSize;
        bombSpeed = game.bombSpeed;
        explosionSpeed = game.explosionSpeed;
        colLifeSpan = game.colLifeSpan;
        explosionWaveStep = game.explosionWaveStep;
        noColChance = game.noColChance;
        playerGuiPositionA = game.playerGuiPositionA;
        playerGuiPositionB = game.playerGuiPositionB;
    
        tileMap = new TileType[mapWidth][];
        tileObjects = new GameObject[mapWidth][];
        effectObjects = new GameObject[mapWidth][];

        for(int i=0; i<mapWidth; i++)
        {
            tileMap[i] = new TileType[mapHeight];
            effectObjects[i] = new GameObject[mapHeight];
            tileObjects[i] = new GameObject[mapHeight];
            for(int j=0; j<mapHeight; j++) effectObjects[i][j] = null;
        }
        SetTileMap(tileCode);
    }

    public void AddPlayerB(Player player, GameObject playerB_GO)
    {
        this.playerB_GO = playerB_GO;
        playerGuiObjBPrefab = Resources.Load<GameObject>("Prefabs/PlayerGuiBPrefab");
        playerGuiObjB = Instantiate(playerGuiObjBPrefab, playerGuiPositionB, Quaternion.identity);
        playerGuiB = playerGuiObjB.GetComponent<PlayerGui>();

        players.Add(player);
        player.ConnectGui(playerGuiB);
        playerGuiB.Init(player);
        playerAmount++;
    }

    public void AddPlayerA(Player player, GameObject playerA_GO)
    {
        this.playerA_GO = playerA_GO;
        playerGuiObjAPrefab = Resources.Load<GameObject>("Prefabs/PlayerGuiAPrefab");
        playerGuiObjA = Instantiate(playerGuiObjAPrefab, playerGuiPositionA, Quaternion.identity);
        playerGuiA = playerGuiObjA.GetComponent<PlayerGui>();

        players.Add(player);
        player.ConnectGui(playerGuiA);
        playerGuiA.Init(player);
        playerAmount++;
    }

    public void AddPlayer(Player player)
    {
        players.Add(player);
        playerAmount++;
    }

    public void SetTileMap(string code)
    {
        // # - wall
        // * - breakable
        // . - empty
        
        int it = 0;
        for(int i=mapHeight-1; i>0; i--)
        {
            for(int j=0; j<mapWidth; j++)
            {
                if(code[it] == 'a')
                {
                    tileMap[j][i] = TileType.Empty;
                    playerPosA =  new Vector3(squareSize/2 + j*squareSize, squareSize/2 + i*squareSize, -0.5f);
                }
                else if(code[it] == 'b')
                {
                    tileMap[j][i] = TileType.Empty;
                    playerPosB =  new Vector3(squareSize/2 + j*squareSize, squareSize/2 + i*squareSize, -0.5f);
                }
                else if(code[it] == '#') tileMap[j][i] = TileType.Wall;
                else if(code[it] == '*') tileMap[j][i] = TileType.Breakable;
                else tileMap[j][i] = TileType.Empty;

                it++;
            }
        }
    }

    Vector2Int GridPosition(Vector3 position)
    {
        float x = position.x;
        float y = position.y;
        return new Vector2Int((int)(x / squareSize), (int)(y / squareSize));
    }

    public bool IsEmpty(Vector3 position, Vector3 offset)
    {
        Vector2Int pos = GridPosition(position + offset * 0.51f);
        Vector2Int bombPos = GridPosition(position + offset);
        Vector2Int playerPos = GridPosition(position);
        if(tileMap[pos.x][pos.y] == TileType.Empty || tileMap[pos.x][pos.y] == TileType.Collectible || (tileMap[bombPos.x][bombPos.y] == TileType.Empty && tileMap[playerPos.x][playerPos.y] == TileType.Bomb)) return true;
        return false;
    }

    IEnumerator ExplosionWave(int x, int y, int range)
    {
        float stepDelay = explosionWaveStep; // czas między "pierścieniami"

        // środek
        SpawnExplosionAt(x, y);
        SingleExplosion(x, y);

        yield return new WaitForSeconds(stepDelay);

        // kierunki
        bool blockRight = false;
        bool blockLeft  = false;
        bool blockUp    = false;
        bool blockDown  = false;

        for (int i = 1; i <= range; i++)
        {
            if (!blockRight)
            {
                int xh = x + i;
                int yh = y;

                if (SingleExplosion(xh, yh)) blockRight = true;
                else SpawnExplosionAt(xh, yh);
            }

            if (!blockLeft)
            {
                int xh = x - i;
                int yh = y;

                if (SingleExplosion(xh, yh)) blockLeft = true;
                else SpawnExplosionAt(xh, yh);
            }

            if (!blockUp)
            {
                int xh = x;
                int yh = y + i;

                if (SingleExplosion(xh, yh)) blockUp = true;
                else SpawnExplosionAt(xh, yh);
            }

            if (!blockDown)
            {
                int xh = x;
                int yh = y - i;

                if (SingleExplosion(xh, yh)) blockDown = true;
                else SpawnExplosionAt(xh, yh);
            }

            yield return new WaitForSeconds(stepDelay);
        }
    }

    void SpawnExplosionAt(int gx, int gy)
    {
        float x = squareSize / 2 + gx * squareSize;
        float y = squareSize / 2 + gy * squareSize;

        GameObject exp = Instantiate(
            explosionPrefab,
            new Vector3(x, y, -0.25f),
            Quaternion.identity
        );

        Animator anim = exp.GetComponent<Animator>();
        anim.speed = explosionSpeed;

        float clipLength = anim.runtimeAnimatorController.animationClips[0].length;
        Destroy(exp, clipLength / anim.speed);
    }

    IEnumerator BombExplosion(int x, int y, float time, Player player)
    {
        float elapsed = 0f;
        while (elapsed < time)
        {
            if(effectObjects[x][y] == null) 
            {
                player.bombCounter--;
                StartCoroutine(ExplosionWave(x, y, player.bombRange));
                audioSource.PlayOneShot(explosionClip);
                tileMap[x][y] = TileType.Empty;
                yield break; 
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        effectObjects[x][y] = null;
        player.bombCounter--;
        tileMap[x][y] = TileType.Empty;
        StartCoroutine(ExplosionWave(x, y, player.bombRange));
        audioSource.PlayOneShot(explosionClip);
    }

    void SpawnBombAt(int gx, int gy, Player player)
    {
        if(tileMap[gx][gy] == TileType.Bomb) return;
        float x = squareSize / 2 + gx * squareSize;
        float y = squareSize / 2 + gy * squareSize;

        GameObject bomb = Instantiate(
            bombPrefab,
            new Vector3(x, y, -0.25f),
            Quaternion.identity
        );

        Animator anim = bomb.GetComponent<Animator>();
        anim.speed = bombSpeed;


        effectObjects[gx][gy] = bomb;
        player.bombCounter++;
        tileMap[gx][gy] = TileType.Bomb;

        float clipLength = anim.runtimeAnimatorController.animationClips[0].length;

        StartCoroutine(BombExplosion(gx, gy, clipLength, player));
        Destroy(bomb, clipLength / anim.speed);
    }

    bool SingleExplosion(int x, int y)
    {
        for(int i=0; i<playerAmount; i++)
        {
            Vector2Int playerPos = GridPosition(players[i].transform.position);
            if(playerPos.x == x && playerPos.y == y)
            {
                players[i].BombHit();
            }
        }

        if (tileMap[x][y] == TileType.Wall) return true;
        if (tileMap[x][y] == TileType.Breakable)
        {
            SpawnExplosionAt(x, y);
            ChangeTileType(x, y, TileType.Empty);
            SpawnCollectibleAt(x, y);
            return true;
        }
        if (tileMap[x][y] == TileType.Bomb || tileMap[x][y] == TileType.Collectible)
        {
            Destroy(effectObjects[x][y]);
        }


        return false;
    }

    void ChangeTileType(int x, int y, TileType type)
    {
        Destroy(tileObjects[x][y]);
        tileMap[x][y] = type;
        if(tileMap[x][y] == TileType.Wall) tileObjects[x][y] = Instantiate(wallPrefab, new Vector3(squareSize/2 + x*squareSize, squareSize/2 + y*squareSize, 0), Quaternion.identity);
        else if(tileMap[x][y] == TileType.Breakable) tileObjects[x][y] = Instantiate(breakablePrefab, new Vector3(squareSize/2 + x*squareSize, squareSize/2 + y*squareSize, 0), Quaternion.identity);
        else if(tileMap[x][y] == TileType.Empty) tileObjects[x][y] = Instantiate(emptyPrefab, new Vector3(squareSize/2 + x*squareSize, squareSize/2 + y*squareSize, 0), Quaternion.identity); 
    }

    IEnumerator CollectibleLife(int x, int y, float time, CollectibleType type, GameObject col)
    {
        float elapsed = 0f;
        while (elapsed < time)
        {
            if(effectObjects[x][y] == null) 
            {
                tileMap[x][y] = TileType.Empty;
                Destroy(col);
                yield break; 
            }

            for(int i=0; i<playerAmount; i++)
            {
                Vector2Int playerPos = GridPosition(players[i].transform.position);
                if(playerPos.x == x && playerPos.y == y)
                {
                    if(type == CollectibleType.bombRange)
                    {
                        players[i].BombRangeUp();
                    }
                    else if(type == CollectibleType.bombAmount)
                    {
                        players[i].BombAmountUp();
                    }
                    else if(type == CollectibleType.speed)
                    {
                        players[i].SpeedUp();
                    }
                    else if(type == CollectibleType.health)
                    {
                        players[i].HealthUp();
                    }
                    audioSource.PlayOneShot(collectClip);
                    tileMap[x][y] = TileType.Empty;
                    Destroy(col);
                    yield break;
                }
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        effectObjects[x][y] = null;
        Destroy(col);
        tileMap[x][y] = TileType.Empty;
    }

    void SpawnCollectibleAt(int gx, int gy)
    {
        float chance = Random.Range(0f, 1f);
        if(chance < noColChance) return;
        
        float x = squareSize / 2 + gx * squareSize;
        float y = squareSize / 2 + gy * squareSize;

        GameObject collectiblePrefab = null;
        CollectibleType type = CollectibleType.bombRange;
        int n = Random.Range(0, 4);

        if(n == 0)
        {
            collectiblePrefab = bombRangePrefab;
            type = CollectibleType.bombRange;
        }
        else if(n == 1)
        {
            collectiblePrefab = bombAmountPrefab;
            type = CollectibleType.bombAmount;
        }
        else if(n == 2)
        {
            collectiblePrefab = speedPrefab;
            type = CollectibleType.speed;
        }
        else if(n == 3)
        {
            collectiblePrefab = healthPrefab;
            type = CollectibleType.health;
        }
        else return;

        GameObject col = Instantiate(
            collectiblePrefab,
            new Vector3(x, y, -0.25f),
            Quaternion.identity
        );

        effectObjects[gx][gy] = col;
        tileMap[gx][gy] = TileType.Collectible;


        StartCoroutine(CollectibleLife(gx, gy, colLifeSpan, type, col));
        Destroy(col, colLifeSpan);
    }

    public void PutBomb(Player player)
    {
        Vector2Int position = GridPosition(player.transform.position);
        SpawnBombAt(position.x, position.y, player);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        wallPrefab = Resources.Load<GameObject>("Prefabs/WallPrefab");
        breakablePrefab = Resources.Load<GameObject>("Prefabs/BreakablePrefab");
        emptyPrefab = Resources.Load<GameObject>("Prefabs/EmptyPrefab");
        explosionPrefab = Resources.Load<GameObject>("Prefabs/ExplosionPrefab");
        bombPrefab = Resources.Load<GameObject>("Prefabs/BombPrefab");

        bombRangePrefab = Resources.Load<GameObject>("Prefabs/BombRangeColPrefab");
        bombAmountPrefab = Resources.Load<GameObject>("Prefabs/BombAmountColPrefab");
        speedPrefab = Resources.Load<GameObject>("Prefabs/SpeedColPrefab");
        healthPrefab = Resources.Load<GameObject>("Prefabs/HealthColPrefab");

        audioSource = GetComponent<AudioSource>();
        PlayCurrentTrack();

        for(int i=0; i<mapWidth; i++)
        {
            for(int j=0; j<mapHeight; j++)
            {
                ChangeTileType(i, j, tileMap[i][j]);
            }
        }
    }

    void EndGame()
    {
        audioSource.Stop();
        Destroy(playerGuiObjA);
        Destroy(playerGuiObjB);
        Destroy(playerA_GO);
        Destroy(playerB_GO);
        for(int i=0; i<mapWidth; i++)
        {
            for(int j=0; j<mapHeight; j++) 
            {
                Destroy(tileObjects[i][j]);
                Destroy(effectObjects[i][j]);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!audioSource.isPlaying)
        {
            NextTrack();
        }

        if(!players[0].isAlive)
        {
            gameEnded = 2;
            players[1].isAlive = false;
            Invoke("EndGame", 2f);
        }
        else if(!players[1].isAlive)
        {
            gameEnded = 1;
            players[0].isAlive = false;
            Invoke("EndGame", 2f);
        }
    }
}
