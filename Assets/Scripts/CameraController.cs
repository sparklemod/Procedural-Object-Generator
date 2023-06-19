using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{
    [SerializeField] private RawImage _image;
    [SerializeField] private Vector2 _zoomRange = new Vector2(1, 10);

    private Camera _camera;
    private RenderTexture _renderTexture;
    private Vector3 _rotation;
    private float _currentZoom;

    void Start()
    {
        _camera = GetComponent<Camera>();
        _camera.aspect = 1;
        _currentZoom = _camera.orthographicSize;

        _renderTexture = new RenderTexture(1024, 1024, 24, RenderTextureFormat.RGB111110Float);
        _camera.targetTexture = _renderTexture;
        _image.texture = _renderTexture;

        _rotation = new Vector3(30, 45, 0);
        Zoom(0);
        Rotate(Vector2.zero);
    }

    public void Rotate(Vector2 delta)
    {
        _rotation += new Vector3(-delta.y, delta.x, 0);
        _rotation.x = Mathf.Clamp(_rotation.x, -90, 90);
        transform.eulerAngles = _rotation;
        transform.position = -transform.forward * 2;
    }

    public void Zoom(float delta)
    {
        _currentZoom = Mathf.Clamp(_currentZoom + delta, _zoomRange.x, _zoomRange.y);
        _camera.orthographicSize = _currentZoom;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
