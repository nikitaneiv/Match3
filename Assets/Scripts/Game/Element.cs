using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
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
    //private float _scaleMultiplier = 1.5f;
    private const float _duration = 0.5f;
    

    private Vector2 _localPosition;
    private Vector2 _gridPosition;
    private SignalBus _signalBus;
    private ElementConfigItem _configItem;
    private Vector2 _startLocalScale;

    public string Key => _configItem.Key;

    public Vector2 GridPosition => _gridPosition;
    public bool IsActive { get; private set; }
    public bool IsInitialized { get; private set; }

    public ElementConfigItem ConfigItem => _configItem;


    [Inject]
    public void Construct(ElementPosition elementPosition,ElementConfigItem configItem, SignalBus signalBus)
    {
        _localPosition = elementPosition.LocalPosition;
        _gridPosition = elementPosition.GridPosition;
        _signalBus = signalBus;
        _configItem = configItem;
    }

    public async void Initialize()
    {
        _startLocalScale = transform.localScale;
        SetConfig();
        SetLocalPosition();
        await Enable();
    }
    private void SetConfig()
    {
        iconSpriteRenderer.sprite = _configItem.Sprite;
    }
    
    public void SetConfig(ElementConfigItem elements)
    {
        _configItem = elements;
        iconSpriteRenderer.sprite = elements.Sprite;
    }
    private void SetLocalPosition()
    {
        transform.localPosition = _localPosition;
    }

    public void SetLocalPosition(Vector2 newLocalPosition, Vector2 gridPosition)
    {
        transform.localPosition = newLocalPosition;
        _gridPosition = gridPosition;
    }

    public async UniTask Enable()
    {
        IsActive = true;
        gameObject.SetActive(true);
       SetSelected(false);
       transform.localScale = Vector3.zero;
       IsInitialized = true;
       await transform.DOScale(_startLocalScale, _duration);
    }

    public async UniTask Disable()
    {
        IsActive = false;
        await transform.DOScale(Vector3.zero, _duration);
        gameObject.SetActive(false);
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

    public void DestroySelf()
    {
        Destroy(gameObject);
    }
    
    // private async UniTask DestroyEffect()
    // {
    //     iconSpriteRenderer.DOFade(0f, _duration);
    //     //transform.DOScale(transform.localScale * _scaleMultiplier, _duration);
    //     await UniTask.Yield();
    // }
    

}
