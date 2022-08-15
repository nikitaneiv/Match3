using System;
using System.Collections;
using System.Collections.Generic;
using Game;
using Signals;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

public class Element : MonoBehaviour
{
    public class Factory: PlaceholderFactory< ElementPosition,ElementConfigItem , Element>
    {
    }

    [SerializeField] private SpriteRenderer bgSpriteRenderer;
    [SerializeField] private SpriteRenderer iconSpriteRenderer;
    
    

    private Vector2 _localPosition;
    private Vector2 _gridPosition;
    private SignalBus _signalBus;
    private ElementConfigItem _configItem;

    public string Key => _configItem.Key;

    public Vector2 GridPosition => _gridPosition;
    public bool IsActive { get; private set; }
    public bool IsInitialized { get; private set; }


    [Inject]
    public void Construct(ElementPosition elementPosition,ElementConfigItem configItem, SignalBus signalBus)
    {
        _localPosition = elementPosition.LocalPosition;
        _gridPosition = elementPosition.GridPosition;
        _signalBus = signalBus;
        _configItem = configItem;
    }

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        SetConfig();
        SetLocalPosition();
        Enable();
    }
    private void SetConfig()
    {
        iconSpriteRenderer.sprite = _configItem.Sprite;
    }
    private void SetLocalPosition()
    {
        transform.localPosition = _localPosition;
    }
    private void Enable()
    {
       gameObject.SetActive(true);
       SetSelected(false);
       //TODO: add animation logic from DoTween;
       IsActive = true;
       IsInitialized = true;
    }

    public void Disable()
    {
        gameObject.SetActive(false);
        //TODO: add animation logic from DoTween; 
    }

    public void SetSelected(bool isOn)
    {
        bgSpriteRenderer.enabled = isOn;
    }

    private void OnMouseUpAsButton()
    {
        OnClick();
    }

    private void OnClick()
    {
        _signalBus.Fire<OnElementSignal>(new OnElementSignal(this));
    }
}
