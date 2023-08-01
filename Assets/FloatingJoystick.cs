using UnityEngine.EventSystems;
using UnityEngine;
using System;
using DG.Tweening;
using UniRx;

public interface IJoystickState
{
    public IReactiveProperty<bool> IJoystickActiveState { get; }
    public IObservable<Unit> UpdateJoystickStream { get; }
}

public class FloatingJoystick :
    MonoBehaviour,
    IDragHandler,
    IPointerDownHandler,
    IPointerUpHandler,
    IJoystickState
{
    [SerializeField, Space] private RectTransform Background;
    [SerializeField, Space] private CanvasGroup BackgroundCanvas;
    [SerializeField, Space] private RectTransform Handle;
    [SerializeField, Space, Range(0, 1f)] private float HandleLimit = 1f;

    public IReactiveProperty<bool> IJoystickActiveState => _joystickActiveState;
    private ReactiveProperty<bool> _joystickActiveState = new();
    public IObservable<Unit> UpdateJoystickStream => _updateJoystickSubject;

    private Subject<Unit> _updateJoystickSubject = new();

    private Vector2 input = Vector2.zero;
    public float Vertical =>  input.normalized.y;
    public float Horizontal =>  input.normalized.x;

    private Vector2 playerRotation;
    public float RotationVertical => playerRotation.y;
    public float RotationHorizontal => playerRotation.x;

    private Vector2 JoyPosition = Vector2.zero;

    public bool Inverse;
    public float Magnitude => _magnitude;
    private float _magnitude;

    private IDisposable _updateDisposable;

    public void OnPointerDown(PointerEventData eventdata)
    {
        _joystickActiveState.Value = true;
        Background.gameObject.SetActive(true);
        Background.DOKill();
        Background.DOScale(1, 0.15f).SetEase(Ease.OutBack);
        BackgroundCanvas.DOFade(1, 0.1f);
        OnDrag(eventdata);
        JoyPosition = eventdata.position;
        Background.position = eventdata.position;
        Handle.anchoredPosition = Vector2.zero;
        input = Vector2.zero;

        _updateDisposable?.Dispose();
        _updateDisposable = Observable.EveryUpdate().Subscribe(_ => { _updateJoystickSubject.OnNext(Unit.Default); })
            .AddTo(this);
    }

    private IDisposable _joystickDirectionDisposable;

    public void SetJoystickRotation(bool fromRight)
    {
        _joystickDirectionDisposable?.Dispose();
        _joystickDirectionDisposable = Observable.Timer(TimeSpan.FromSeconds(0.5f)).Subscribe(_ =>
        {
            Inverse = fromRight;
        }).AddTo(this);
    }

    private void OnDisable()
    {
        _magnitude = 0;
        Background.gameObject.SetActive(false);

        _updateJoystickSubject.OnNext(Unit.Default);
        _updateDisposable?.Dispose();
        _joystickActiveState.Value = false;
    }

    public void OnDrag(PointerEventData eventdata)
    {
        Vector2 JoyDriection = eventdata.position - JoyPosition;

        input = JoyDriection.magnitude > Background.sizeDelta.x / 2f
            ? JoyDriection.normalized
            : JoyDriection / (Background.sizeDelta.x / 2f);

        var sizeDelta = Background.sizeDelta;

        Handle.anchoredPosition = (input * sizeDelta.x / 2f) * HandleLimit;

        playerRotation = JoyDriection.magnitude > sizeDelta.x / 2f
            ? JoyDriection.normalized
            : JoyDriection / (Background.sizeDelta.x / 2f);

        _magnitude = Mathf.Abs(input.x) + Mathf.Abs(input.y);
    }

    public void OnPointerUp(PointerEventData eventdata)
    {
        Background.DOKill();

        BackgroundCanvas.DOFade(0, 0.1f);
        Background.DOScale(0, 0.25f).SetEase(Ease.OutBack).OnComplete(() =>
        {
            Background.gameObject.SetActive(false);
        });
        input = Vector2.zero;
        Handle.anchoredPosition = Vector2.zero;
        _magnitude = 0;

        _updateJoystickSubject.OnNext(Unit.Default);
        _updateDisposable.Dispose();
        _joystickActiveState.Value = false;
    }
}