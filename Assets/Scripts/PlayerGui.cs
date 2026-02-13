using UnityEngine;
using TMPro;

public class PlayerGui : MonoBehaviour
{
    public TextMeshPro health, bombRange, bombAmount, speed;
    public Player player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Init(Player player)
    {
        this.player = player;
        UpdateValues();
    }

    public void UpdateValues()
    {
        health.text = player.health.ToString();
        bombRange.text = player.bombRange.ToString();
        bombAmount.text = player.bombAmount.ToString();
        speed.text = player.speed.ToString("F2");
    }
}
