using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class SelectArea : MonoBehaviour
{
    [Header("Parents")]
    [SerializeField] private Transform _firstLayerParent;
    [SerializeField] private Transform _secondLayerParent;

    [Header("Transforms")]
    [SerializeField] private Transform[] _transforms; // Позиции 
    [SerializeField] private Transform[] _areasTransform; // Арены   0 = 1vs1 ...

    [Header("Settings")]
    [SerializeField] private float _moveSpeed = 0.1f;
    [SerializeField] private float _changeLayerOffset = 0.3f;
    [SerializeField] private float _minScale = 0.8f;
    [SerializeField] private int _currentArea = 1;
    private bool _move;

    private void Start()
    {
        SetArea();
    }

    public void NextArea()
    {
        if (!_move)
        {
            _currentArea++;
            if (_currentArea > _transforms.Length - 1)
                _currentArea = 0;

            SetArea();
        }
    }

    public void PreviousArea()
    {
        if (!_move)
        {
            _currentArea--;
            if (_currentArea < 0)
                _currentArea = _transforms.Length - 1;

            SetArea();
        }
    }

    private void SetArea()
    {
        int firstIndex = (_currentArea - 1 < 0) ? _transforms.Length - 1 : _currentArea - 1;
        int lastIndex = (_currentArea + 1 > _transforms.Length - 1) ? 0 : _currentArea + 1;

        switch (_currentArea)
        {
            case 0:
                PlayerPrefs.SetString("GameMode", "one"); // Менять при выборе сцены
                break;

            case 1:
                PlayerPrefs.SetString("GameMode", "two");
                break;

            case 2:
                PlayerPrefs.SetString("GameMode", "three");
                break;
        }
        

        StartCoroutine(AreaTransition(firstIndex, lastIndex));
    }

    private IEnumerator AreaTransition(int firstIndex, int lastIndex)
    {
        _move = true;
        bool layerChanged = false;
        float currentState = 0;

        Vector3[] startPos = {
            _areasTransform[firstIndex].position, 
            _areasTransform[_currentArea].position, 
            _areasTransform[lastIndex].position 
        };

        while (_move)
        {
            if (currentState >= 1)
            {
                _move = false;
                _areasTransform[firstIndex].position = _transforms[0].position;
                _areasTransform[_currentArea].position = _transforms[1].position;
                _areasTransform[lastIndex].position = _transforms[2].position;
                break;
            }

            if (currentState >= _changeLayerOffset && !layerChanged)
            {
                layerChanged = true;

                foreach (Transform areaTransform in _areasTransform)
                {
                    areaTransform.localScale = new Vector3(_minScale, _minScale, _minScale);
                    areaTransform.GetComponent<Image>().color = Color.gray;
                    areaTransform.SetParent(_secondLayerParent);
                }
                
                _areasTransform[_currentArea].SetParent(_firstLayerParent);
                _areasTransform[_currentArea].localScale = new Vector3(1, 1, 1);
                _areasTransform[_currentArea].GetComponent<Image>().color = Color.white;
            }

            _areasTransform[firstIndex].position = Vector3.Lerp(startPos[0], _transforms[0].position, currentState);
            _areasTransform[_currentArea].position = Vector3.Lerp(startPos[1], _transforms[1].position, currentState);
            _areasTransform[lastIndex].position = Vector3.Lerp(startPos[2], _transforms[2].position, currentState);

            currentState += _moveSpeed;
            yield return new WaitForFixedUpdate();
        }

        _move = false;
        yield return null;
    }
}
