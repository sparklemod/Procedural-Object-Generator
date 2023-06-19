using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseGenerator : MonoBehaviour
{
    public DrawingBoard heights;
    public abstract void GenerateDefault();
    public abstract void Generate();

    [SerializeField] protected Transform _targetTransform;

    protected void Clear()
    {
        for (int i = _targetTransform.childCount - 1; i >= 0; i--)
            DestroyImmediate(_targetTransform.GetChild(i).gameObject);
    }

    protected void OnDisable() {
        _targetTransform.gameObject.SetActive(false);
    }

    protected void OnEnable() {
        _targetTransform.gameObject.SetActive(true);
    }
}
