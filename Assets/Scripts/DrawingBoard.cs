using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DrawingBoard : MonoBehaviour
{
    [SerializeField] protected Image _drawingImage;
    [SerializeField] protected Cursor _cursor;
    [SerializeField] private Slider _colorSlider;
    [SerializeField] private Material _sliderMaterial;
    
    private enum ChannelCheck { R, G, B }
    [SerializeField] private ChannelCheck _heightChannelCheck;

    protected Rect _imageRect;
    public Texture2D Texture { get; private set; }
    public int Radius;
    public float BlurStrength;

    public bool Visible
    {
        get => gameObject.activeSelf;
        set => gameObject.SetActive(value);
    }

    protected Vector2 currentPos = Vector2.zero, oldPos = Vector2.zero;

    public Color DrawingColor;

    public Color BottomColor, TopColor;
    protected Color transparentColor = new Color(0, 0, 0, 0);

    
    protected virtual void Awake()
    {
        // Create drawing texture
        Texture = new Texture2D(512, 512, TextureFormat.RGBA32, 1, true);
        Texture.wrapMode = TextureWrapMode.Clamp;
        Sprite sprite = Sprite.Create(Texture, new Rect(0, 0, 512, 512), Vector2.one * 0.5f);
        _drawingImage.sprite = sprite;

        ClearImage();

        _sliderMaterial.SetColor("_ColorA", BottomColor);
        _sliderMaterial.SetColor("_ColorB", TopColor);
    }

    protected virtual void Update()
    {
        DrawingColor = Color.Lerp(BottomColor, TopColor, _colorSlider.value);
        UpdateImageCorners();
        currentPos.Set(
            Input.mousePosition.x,
            Input.mousePosition.y
        );

        if (_imageRect.Contains(currentPos) && !ControlPanel.OnControlPanel)
        {
            // setup cursor
            _cursor.Setup(
                (currentPos - _imageRect.position) * _drawingImage.rectTransform.rect.size.x / _imageRect.width,
                Radius * 2 * _drawingImage.rectTransform.rect.size.x / Texture.width
            );
            
            // draw on texture
            if ((_cursor.state == Cursor.State.Circle || _cursor.state == Cursor.State.Eraser || _cursor.state == Cursor.State.Sponge) && Input.GetMouseButton(0))
            {
                // connect last position and current position with continuous line
                Vector2 curPos = new Vector2((int) oldPos.x, (int) oldPos.y);
                Vector2 destPos = new Vector2((int) currentPos.x, (int) currentPos.y);
                Vector2 destDir = (destPos - curPos).normalized * 0.1f;
                bool shouldDraw = true;

                int iterations = 
                    destDir.magnitude > 0.001f ?
                    Mathf.Max(1, (int) ((destPos - curPos).magnitude / destDir.magnitude)) :
                    1;
                for (int i = 0; i < iterations; i++)
                {
                    if (shouldDraw)
                    {
                        var pixel = PointToPixel(curPos);
                        if (_cursor.state == Cursor.State.Sponge)
                            BlurCircleAt(
                                Radius,
                                pixel.x, pixel.y
                            );
                        else
                            DrawCircleAt(
                                Radius,
                                pixel.x, pixel.y,
                                _cursor.state == Cursor.State.Eraser ? transparentColor : DrawingColor
                            );
                    }
                    shouldDraw = ((int) (curPos.x + destDir.x) != (int) curPos.x) || ((int) (curPos.y + destDir.y) != (int) curPos.y);
                    curPos += destDir;
                }
                
                Texture.Apply();
            }
            else if (_cursor.state == Cursor.State.Dropper && Input.GetMouseButton(0))
            {
                _colorSlider.value = GetHeightAtPixel(PointToPixel(currentPos));
                DrawingColor = Color.Lerp(BottomColor, TopColor, _colorSlider.value);
            }
        }
        else
        {
            _cursor.Hide();
        }

        oldPos.Set(currentPos.x, currentPos.y);
    }

    void UpdateImageCorners()
    {
        Vector3[] corners = new Vector3[4];
        _drawingImage.rectTransform.GetWorldCorners(corners);
        _imageRect = new Rect(corners[0], corners[2] - corners[0]);
    }

    public float GetHeightAtPixel(Vector2Int point) => GetHeightAtPixel(point.x, point.y);

    public float GetHeightAtPixel(int x, int y)
    {
        if (Texture.GetPixel(x, y).a < 0.01f) return -1;
        switch (_heightChannelCheck)
        {
            case ChannelCheck.R: return Mathf.InverseLerp(BottomColor.r, TopColor.r, Texture.GetPixel(x, y).r);
            case ChannelCheck.G: return Mathf.InverseLerp(BottomColor.g, TopColor.g, Texture.GetPixel(x, y).g);
            case ChannelCheck.B: return Mathf.InverseLerp(BottomColor.b, TopColor.b, Texture.GetPixel(x, y).b);
        }
        return 0;
    }

    void DrawCircleAt(int r, int x, int y, Color color) //х, у - пиксель центр круга (точка на текстуре). р - радиус
    {
        for (int a = Mathf.Max(x - r, 0), height, b; a < x + r && a < Texture.width; a++) //проходимся по х потом по у. хайт просчитывает сколько мы идем по вертикали
        {
            height = Mathf.RoundToInt(Mathf.Sqrt(r * r - (a - x) * (a - x)));
            for (b = Mathf.Max(y - height, 0); b < y + height && b < Texture.height; b++)
                Texture.SetPixel(a, b, color);
        }
    }

    void BlurCircleAt(int r, int x, int y)
    {
        float[] colorAccum = new float[3]; //переменная нахождения ср цвета
        int pixels = 0; 
        for (int a = Mathf.Max(x - r, 0), height, b; a < x + r && a < Texture.width; a++) //подсчет среднего цвета
        {
            height = Mathf.RoundToInt(Mathf.Sqrt(r * r - (a - x) * (a - x))); //сколько щагов вверх нужно сделать (кол-во лесенок)
            for (b = Mathf.Max(y - height, 0); b < y + height && b < Texture.height; b++) 
            {
                Color pixel = Texture.GetPixel(a, b);
                if (pixel.a < 0.99) continue; //игнорируем прозрачные
                colorAccum[0] += pixel.r;
                colorAccum[1] += pixel.g;
                colorAccum[2] += pixel.b;
                pixels++;
            }
        }

        if (pixels <= 1) return;

        Color targetColor = new Color(colorAccum[0] / pixels, colorAccum[1] / pixels, colorAccum[2] / pixels); //рисование губкой 
        float dist;
        for (int a = Mathf.Max(x - r, 0), height, b; a < x + r && a < Texture.width; a++)
        {
            height = Mathf.RoundToInt(Mathf.Sqrt(r * r - (a - x) * (a - x)));
            for (b = Mathf.Max(y - height, 0); b < y + height && b < Texture.height; b++)
            {
                dist = new Vector2(a - x, b - y).magnitude;
                Texture.SetPixel(a, b, Color.Lerp(Texture.GetPixel(a, b), targetColor, BlurStrength * (1 - dist / r))); //функция смешивания (зависит от расстояния)
            }
        }
    }

    public virtual void ClearImage()
    {
        for (int i = 0; i < Texture.width; i++)
            for (int j = 0; j < Texture.height; j++)
                Texture.SetPixel(i, j, transparentColor);
        Texture.Apply();
    }

    protected Vector2Int PointToPixel(Vector2 point)
    {
        point = (point - _imageRect.position) * Texture.width / _imageRect.width;
        return new Vector2Int((int) point.x, (int) point.y);
    }
}
