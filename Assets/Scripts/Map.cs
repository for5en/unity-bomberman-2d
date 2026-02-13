using UnityEngine;


public class Map
{
    public string tileCode;
    public int width;
    public int height;

    public Map(int width, int height, string tileCode)
    {
        this.width = width;
        this.height = height;
        this.tileCode = TrimCode(tileCode);
    }

    string TrimCode(string raw)
    {
        string[] lines = raw.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            lines[i] = lines[i].Trim();
        }
        return string.Join("", lines);
    }
}
