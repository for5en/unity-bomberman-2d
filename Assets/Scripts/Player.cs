using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public bool isAlive;

    float invisibilityTimer;
    private float invisibilityTime = 1.5f;
    float lyingTimer;
    public int health;
    public float speed;

    public int bombRange;
    public int bombAmount;
    public int bombCounter;

    Environment environment;
    PlayerControls controls;
    float squareSize;
    Animator anim;
    SpriteRenderer sr;
    PlayerGui gui;

    private float timeUp = 0f;
    private float timeDown = 0f;
    private float timeRight = 0f;
    private float timeLeft = 0f;

    public void Init(Vector3 startingPosition, Environment environment, PlayerControls controls)
    {
        isAlive = true;
        health = 3;
        speed = 6f;
        bombRange = 1;
        bombAmount = 1;
        bombCounter = 0;
        transform.position = startingPosition;
        this.environment = environment;
        this.controls = controls;
        squareSize = environment.squareSize;
    }

    void MoveUp()
    {
        int n = (int)(transform.position.x / squareSize);
        if(timeUp >= Mathf.Max(timeUp, timeDown, timeLeft, timeRight))
        {
            anim.SetBool("walkingUp", true);
            anim.SetBool("walkingDown", false);
            anim.SetBool("walkingRight", false);
            sr.flipX = false;
        }
        if(timeUp >= Mathf.Max(timeUp, timeDown, timeLeft, timeRight) && Mathf.Abs(0.5f * squareSize + n * squareSize - transform.position.x) < squareSize / 3f && environment.IsEmpty(transform.position, Vector3.up * squareSize))
        {
            transform.position = new Vector3(
                0.5f * squareSize + n * squareSize,
                transform.position.y + Time.deltaTime * speed,
                transform.position.z
            );
        }
    }

    void MoveDown()
    {
        int n = (int)(transform.position.x / squareSize);
        if(timeDown >= Mathf.Max(timeUp, timeDown, timeLeft, timeRight))
        {
            anim.SetBool("walkingUp", false);
            anim.SetBool("walkingDown", true);
            anim.SetBool("walkingRight", false);
            sr.flipX = false;
        }
        if(timeDown >= Mathf.Max(timeUp, timeDown, timeLeft, timeRight) && Mathf.Abs(0.5f * squareSize + n * squareSize - transform.position.x) < squareSize / 3f && environment.IsEmpty(transform.position, Vector3.down * squareSize))
        {
            transform.position = new Vector3(
                0.5f * squareSize + n * squareSize,
                transform.position.y - Time.deltaTime * speed,
                transform.position.z
            );
        }
    }

    void MoveRight()
    {
        int n = (int)(transform.position.y / squareSize);
        if(timeRight >= Mathf.Max(timeUp, timeDown, timeLeft, timeRight))
        {
            anim.SetBool("walkingUp", false);
            anim.SetBool("walkingDown", false);
            anim.SetBool("walkingRight", true);
            sr.flipX = false;
        }
        if(timeRight >= Mathf.Max(timeUp, timeDown, timeLeft, timeRight) && Mathf.Abs(0.5f * squareSize + n * squareSize - transform.position.y) < squareSize / 3f && environment.IsEmpty(transform.position, Vector3.right * squareSize))
        {
            transform.position = new Vector3(
                transform.position.x + Time.deltaTime * speed,
                0.5f * squareSize + n * squareSize,
                transform.position.z
            );
        }
    }

    void MoveLeft()
    {
        int n = (int)(transform.position.y / squareSize);
        if(timeLeft >= Mathf.Max(timeUp, timeDown, timeLeft, timeRight))
        {
            anim.SetBool("walkingUp", false);
            anim.SetBool("walkingDown", false);
            anim.SetBool("walkingRight", true);
            sr.flipX = true;
        }
        if(timeLeft >= Mathf.Max(timeUp, timeDown, timeLeft, timeRight) && Mathf.Abs(0.5f * squareSize + n * squareSize - transform.position.y) < squareSize / 3f && environment.IsEmpty(transform.position, Vector3.left * squareSize))
        {
            transform.position = new Vector3(
                transform.position.x - Time.deltaTime * speed,
                0.5f * squareSize + n * squareSize,
                transform.position.z
            );
        }
    }

    void PutBomb()
    {
        if(bombCounter < bombAmount) environment.PutBomb(this);
    }

    public void BombRangeUp()
    {
        bombRange++;
        gui.UpdateValues();
    }

    public void BombAmountUp()
    {
        bombAmount++;
        gui.UpdateValues();
    }

    public void SpeedUp()
    {
        speed *= 1.05f;
        gui.UpdateValues();
    }

    public void HealthUp()
    {
        health++;
        if(health > 3) health--;
        gui.UpdateValues();
    }

    public void BombHit()
    {
        if(invisibilityTimer < 0) return;
        health--;
        invisibilityTimer = -invisibilityTime;
        lyingTimer = -invisibilityTime / 3f;
        anim.SetBool("walkingUp", false);
        anim.SetBool("walkingDown", false);
        anim.SetBool("walkingRight", false);
        anim.SetBool("isLying", true);
        if(health <= 0)
        {
            isAlive = false;
        }
        gui.UpdateValues();
    }

    public void ConnectGui(PlayerGui gui)
    {
        this.gui = gui;
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        lyingTimer += Time.deltaTime;
        invisibilityTimer += Time.deltaTime;
        if(isAlive && lyingTimer >= 0)
        {
            anim.SetBool("isLying", false);
            if(Keyboard.current[controls.w].wasPressedThisFrame) timeUp = Time.time;
            if(Keyboard.current[controls.a].wasPressedThisFrame) timeLeft = Time.time;
            if(Keyboard.current[controls.s].wasPressedThisFrame) timeDown = Time.time;
            if(Keyboard.current[controls.d].wasPressedThisFrame) timeRight = Time.time;

            if(Keyboard.current[controls.w].isPressed) MoveUp();
            if(Keyboard.current[controls.a].isPressed) MoveLeft();
            if(Keyboard.current[controls.s].isPressed) MoveDown();
            if(Keyboard.current[controls.d].isPressed) MoveRight();
            if(Keyboard.current[controls.b].wasPressedThisFrame) PutBomb();

            if(Keyboard.current[controls.w].wasReleasedThisFrame) 
            {
                timeUp = 0f;
                anim.SetBool("walkingUp", false);
            }
            if(Keyboard.current[controls.a].wasReleasedThisFrame) 
            {
                timeLeft = 0f;
                anim.SetBool("walkingRight", false);
            }
            if(Keyboard.current[controls.s].wasReleasedThisFrame) 
            {
                timeDown = 0f;
                anim.SetBool("walkingDown", false);
            }
            if(Keyboard.current[controls.d].wasReleasedThisFrame) 
            {
                timeRight = 0f;
                anim.SetBool("walkingRight", false);
            }
        }
    }
}
