using UnityEngine;

public class RandomSprite : MonoBehaviour
{
    public Sprite[] sprites;

    void Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.sprite = sprites[Random.Range(0, sprites.Length)];
    }
}