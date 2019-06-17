using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.LWRP;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class ShakerSort : MonoBehaviour
{
    /// <summary>
    /// Fields only populated by the Unity editor. It is assumed that their values are never <code>null</code> or
    /// unset, unless specified by the developer.
    /// </summary>
    #region editor-defined fields
    [SerializeField] private Transform _barContainer;
    [SerializeField] private Slider _sampleSize;
    [SerializeField] private Slider _sortingSpeed;
    [SerializeField] private Dropdown _randomness;
    [SerializeField] private int _minimumValue;
    [SerializeField] private int _maximumValue;
    [SerializeField] private GameObject _barPrefab;
    [SerializeField] private float _waitDuration;
    #endregion

    /// <summary>
    /// Fields only populated by the script.
    /// </summary>

    #region process-defined fields
    private int[] _data;
    private int _leftIndex;
    private int _rightIndex;
    private bool _isAnimationRunning;
    #endregion

    /// <summary>
    /// Properties for convenience.
    /// </summary>
    #region read-only properties
    private Transform LeftBar
        => _barContainer.GetChild(_leftIndex);
    private Transform RightBar
        =>_barContainer.GetChild(_rightIndex);
    private int BarCount
        => _barContainer.childCount;

    private float SortingSpeed
        => _sortingSpeed.value > 0 ? _sortingSpeed.value : 1;
    #endregion

    public void Execute()
    {
        StopCoroutine("Sort");
        Reset();
        SetUp();
        StartCoroutine("Sort");
    }

    public void ExitGame()
    {
        #if UNITY_EDITOR
        // Application.Quit() does not work in the editor so
        // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    #region coroutines
    public IEnumerator Sort()
    {
        var swapped = true;
        for (int min = 0, max = BarCount - 1; swapped && min < max; )
        {
            swapped = false;
            for (var i = min; i < max; i++)
            {
                SetColor(_barContainer.GetChild(i), Color.red);
                if (_data[i] > _data[i + 1])
                {
                    _leftIndex = i;
                    _rightIndex = i + 1;
                    _isAnimationRunning = true;
                    yield return new WaitWhile(() => _isAnimationRunning);
                    yield return new WaitForSeconds(_waitDuration / SortingSpeed);
                    Swap(i, i + 1);
                    swapped = true;
                }
                else
                {
                    yield return new WaitForSeconds(_waitDuration / SortingSpeed);
                }
                SetColor(_barContainer.GetChild(i), Color.white);
            }

            SetColor(_barContainer.GetChild(max), Color.white);
            max--;

            if (!swapped)
                break;

            swapped = false;

            for (var i = max; i > min; i--)
            {
                SetColor(_barContainer.GetChild(i), Color.red);
                if (_data[i - 1] > _data[i])
                {
                    _leftIndex = i - 1;
                    _rightIndex = i;
                    _isAnimationRunning = true;
                    yield return new WaitWhile(() => _isAnimationRunning);
                    yield return new WaitForSeconds(_waitDuration / SortingSpeed);
                    Swap(i - 1, i);
                    swapped = true;
                }
                else
                {
                    yield return new WaitForSeconds(_waitDuration / SortingSpeed);
                }
                SetColor(_barContainer.GetChild(i), Color.white);
            }

            SetColor(_barContainer.GetChild(min), Color.white);
            min++;
        }
    }
    
    #endregion

    #region script functions
    private void Reset()
    {
        _isAnimationRunning = false;
        _rightIndex = -1;
        _leftIndex = -1;

        for (var i = 0; i < 100 && _barContainer.childCount > 0; i++)
        {
            var bar = _barContainer.GetChild(0);
            bar.parent = null;
            Destroy(bar.gameObject);
        }
    }

    private void SetUp()
    {
        var sampleCount = (int)_sampleSize.value;
        _data = new int[sampleCount];
        for (var i = 0; i < sampleCount; i++)
        {
            _data[i] = GetRandomizedValue(i, sampleCount);
            // Only X axis is relevant.
            var scale = new Vector3(1, _data[i] * 0.3f, 1);
            var position = new Vector3(i, scale.y / 2, 0);
            var bar = Instantiate(_barPrefab, position, Quaternion.identity, _barContainer);
            bar.name = "Bar " + i;
            bar.transform.localScale = scale;
        }
    }

    private int GetRandomizedValue(int index, int sampleCount)
    {
        var min = _minimumValue;
        var max = _maximumValue;
        var range = max - min;
        var step = range / (float)sampleCount;
        var reverseIndex = sampleCount - index;

        switch (_randomness.value)
        {
            case 1:
                return min + (int) Random.Range(step * index, step * (index + 1));

            case 2:
                return min + (int) Random.Range(step * reverseIndex, step * (reverseIndex - 1));

            default:
                return Random.Range(min, max);
        }
    }

    private void Swap(int indexA, int indexB)
    {
        var tmp = _data[indexA];
        _data[indexA] = _data[indexB];
        _data[indexB] = tmp;
    }

    private static void SetColor(Component transform, Color color)
    {
        var renderer = transform.GetComponent<Renderer>();
        renderer.material.color = color;
    }

    #endregion

    /// <summary>
    /// Functions called by Unity during the lifecycle of this behaviour.
    /// </summary>
    #region Unity lifecycle functions
    void Start()
    {
        _rightIndex = -1;
        _leftIndex = -1;
    }

    void Update()
    {
        // Abort function, if no animation should run.
        if (!_isAnimationRunning)
            return;

        // Calculates the movement based on the animation progress.
        var movement = new Vector3(Time.deltaTime * SortingSpeed, 0, 0);
        
        // Applies movement to both bars.
        LeftBar.position += movement;
        RightBar.position -= movement;

        // Resets animation progress, if it reached the progress cap.
        if ((int)LeftBar.position.x == _leftIndex || (int)RightBar.position.x == _rightIndex) return;
        // Somewhere here is an issue.
        LeftBar.position = new Vector3(_leftIndex + 1, LeftBar.position.y, LeftBar.position.z);
        RightBar.position = new Vector3(_rightIndex - 1, RightBar.position.y, RightBar.position.z);
        LeftBar.SetSiblingIndex(_rightIndex);
        _leftIndex = -1;
        _rightIndex = -1;
        _isAnimationRunning = false;
    }
    #endregion

}
