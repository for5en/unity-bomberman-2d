using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControls
{
    public Key w;
    public Key a;
    public Key s;
    public Key d;
    public Key b;
    
    public PlayerControls(Key w, Key a, Key s, Key d, Key b)
    {
        this.w = w;
        this.a = a;
        this.s = s;
        this.d = d;
        this.b = b;
    }
}
