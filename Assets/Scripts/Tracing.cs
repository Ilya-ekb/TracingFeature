using DefaultNamespace;
using UnityEngine;

public class Tracing : MonoBehaviour
{
    private bool IsCompleted => Mathf.Abs(secondHandler.Position - firstHandler.Position) < completeThreshold;

    [SerializeField] private Handler prefab;
    [SerializeField] private FadedImage completeImage;
    [SerializeField] private FadedImage clockwiseDotted;
    [SerializeField] private FadedImage counterClockwiseDotted;

    private Handler firstHandler;
    private Handler secondHandler;
    private const float completeThreshold = 0.01f;


    private void Start()
    {
        firstHandler = Instantiate(prefab, transform);
        secondHandler = Instantiate(prefab, transform);

        firstHandler.OnSelectDirection += OnSelectDirection;
        secondHandler.OnSelectDirection += OnSelectDirection;
    }

    private void OnSelectDirection(Direction direction, Handler handler)
    {
        var otherHandler = handler == firstHandler ? secondHandler : firstHandler;
        handler.OnSelectDirection -= OnSelectDirection;
        otherHandler.OnSelectDirection -= OnSelectDirection;

        AssignHanding(direction, handler, otherHandler);
    }

    private void AssignHanding(Direction direction, Handler handler, Handler otherHandler)
    {
        var dottedImage = direction is Direction.Clockwise ? clockwiseDotted : counterClockwiseDotted;

        handler.SetDirection(direction);
        handler.SetDottedView(dottedImage);

        direction = direction is Direction.Clockwise ? Direction.CounterClockwise : Direction.Clockwise;
        dottedImage = dottedImage == counterClockwiseDotted ? clockwiseDotted : counterClockwiseDotted;

        otherHandler.SetDirection(direction);
        otherHandler.SetDottedView(dottedImage);

        handler.OnHandling += otherHandler.UpdateMaxValue;
        handler.OnHandling += OnHandling;
        
        otherHandler.OnHandling += handler.UpdateMaxValue;
        otherHandler.OnHandling += OnHandling;
    }

    private void OnHandling(float value, Handler handler)
    {
        if (!IsCompleted) return;
        OnComplete();
    }

    private void OnComplete()
    {
        if (firstHandler)
        {
            firstHandler.OnHandling -= OnHandling;
            firstHandler.Drop();
        }

        if (secondHandler)
        {
            secondHandler.OnHandling -= OnHandling;
            secondHandler.Drop();
        }

        if (completeImage)
            completeImage.Fade();
    }
}