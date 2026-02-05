using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

enum ShiftState
{
    lower,
    upper,
    upperLock
}

public class KeyboardManager : MonoBehaviour
{
    [Header("타이핑할 인풋 타겟")]
    [SerializeField]
    private TMP_InputField _input;

    [Header("문자열 버튼 리스트")]
    [SerializeField]
    private List<Button> _charBtns;

    [Header("특수 기능 버튼")]
    [SerializeField]
    private Button _backspaceBtn;
    [Space]
    [SerializeField]
    private Button _shiftBtn;
    [SerializeField]
    private GameObject _lower;
    [SerializeField]
    private GameObject _upper;
    [SerializeField]
    private GameObject _upperLock;
    [Space]
    [SerializeField]
    private Button _spaceBtn;


    private ShiftState _shiftState = ShiftState.lower;

    private List<TMP_Text> keyTexts;

    void Awake()
    {
        InitBtns();

        InitActive();

        InitKeys();
    }

    void InitBtns()
    {
        foreach (var charBtn in _charBtns)
        {
            charBtn.onClick.AddListener(() => ClickCharBtn(charBtn.gameObject.name));
        }

        _backspaceBtn.onClick.AddListener(() => ClickBackscape());
        _shiftBtn.onClick.AddListener(() => ClickShift());
        _spaceBtn.onClick.AddListener(() => ClickCharBtn(" "));
    }

    void InitActive()
    {
        _lower.SetActive(true);
        _upper.SetActive(false);
        _upperLock.SetActive(false);
    }

    void InitKeys()
    {
        keyTexts = new List<TMP_Text>();

        foreach (var charBtn in _charBtns)
        {
            keyTexts.Add(charBtn.GetComponentInChildren<TMP_Text>());
        }
    }

    void ClickCharBtn(string str)
    {
        switch (_shiftState)
        {
            case ShiftState.upper:
                str = str.ToUpper();
                ChangeShiftState(ShiftState.lower);
                break;

            case ShiftState.upperLock:
                str = str.ToUpper();
                break;
        }

        if (_input.text.Length >= 16)
        {
            return;
        }

        _input.text += str;
    }

    void ClickBackscape()
    {
        if (_input.text.Length > 0)
        {
            _input.text = _input.text.Substring(0, _input.text.Length - 1);
        }
    }

    void ClickShift()
    {
        switch(_shiftState)
        {
            case ShiftState.lower:
                ChangeShiftState(ShiftState.upper);
                break;

            case ShiftState.upper:
                ChangeShiftState(ShiftState.upperLock);
                break;

            case ShiftState.upperLock:
                ChangeShiftState(ShiftState.lower);
                break;
        }
    }

    void ChangeShiftState(ShiftState targetState)
    {
        switch(targetState)
        {
            case ShiftState.lower:
                _shiftState = ShiftState.lower;

                _upper.SetActive(false);
                _upperLock.SetActive(false);
                _lower.SetActive(true);

                foreach (var keyText in keyTexts)
                {
                    keyText.text = keyText.text.ToLower();
                }

                break;

            case ShiftState.upper:
                _shiftState = ShiftState.upper;

                _lower.SetActive(false);
                _upper.SetActive(true);

                foreach (var keyText in keyTexts)
                {
                    keyText.text = keyText.text.ToUpper();
                }

                break;

            case ShiftState.upperLock:
                _shiftState = ShiftState.upperLock;

                _upper.SetActive(false);
                _upperLock.SetActive(true);

                break;
        }
    }
}