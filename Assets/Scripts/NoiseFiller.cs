using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseFiller : MonoBehaviour
{
    public int Seed;
    public float Scale;

    [SerializeField] private DrawingBoard _board;

    public void GenerateNoiseMap()
    {
        int w = _board.Texture.width, h = _board.Texture.height;
        for (int i = 0; i < w; i++)
            for (int j = 0; j < h; j++)
            {
                float height = Mathf.PerlinNoise((i + w + Seed) * Scale, (j + h + Seed) * Scale);
                _board.Texture.SetPixel(i, j, Color.Lerp(_board.BottomColor, _board.TopColor, height));
            }
            
        _board.Texture.Apply();
    }

}
