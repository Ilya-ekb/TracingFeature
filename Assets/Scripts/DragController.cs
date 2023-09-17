using System;
using FluffyUnderware.Curvy.Controllers;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler,
    IPointerExitHandler
{
    public Action<Direction> OnApplyDirection { get; set; }
    public Action<float> OnChangePosition { get; set; }

    private Direction direction = Direction.None;
    private SplineController splineController;
    private float interactionRadius;
    private float lastPosition;
    private float targetPosition;
    private bool isHandlerTouched;
    private bool isCanDrag;

    public void Initiate(SplineController controller, float radius)
    {
        interactionRadius = radius;
        splineController = controller;
        lastPosition = 0;
    }

    public void ForceSetDirection(Direction dir)
    {
        direction = dir;
        lastPosition = splineController.Position = (float)dir;
        targetPosition = 1 - lastPosition;
        OnChangePosition?.Invoke(lastPosition);
    }

    public void UpdateMaxPosition(float value)
    {
        targetPosition = value;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (direction is not Direction.None)
            return;
        var dir = eventData.delta.normalized;
        dir = Vector3.right * dir.x;
        direction = Vector3.Dot(Vector3.right, dir) > 0 ? Direction.Clockwise : Direction.CounterClockwise;
        lastPosition = splineController.Position = (float)direction;
        OnApplyDirection?.Invoke(direction);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (direction is Direction.None || (!isHandlerTouched && !isCanDrag) || !splineController.Spline)
            return;

        var inputPosition = eventData.pointerCurrentRaycast.worldPosition;
        var positionOnSpline = splineController.Spline.GetNearestPointTF(inputPosition, Space.World);
        var worldPosition = transform.position;
        isCanDrag = Vector3.Distance(inputPosition, worldPosition) < interactionRadius &&
                    Math.Abs(lastPosition - positionOnSpline) < interactionRadius;

        if (!isCanDrag)
        {
            isHandlerTouched = false;
            return;
        }

        var assignedPosition = direction is Direction.Clockwise
            ? Math.Max(lastPosition, positionOnSpline)
            : Math.Min(lastPosition, positionOnSpline);

        var min = direction is Direction.Clockwise ? assignedPosition : targetPosition;
        var max = direction is Direction.Clockwise ? targetPosition : assignedPosition;
        splineController.Position = Mathf.Clamp(assignedPosition, min, max);
        lastPosition = assignedPosition;

        OnChangePosition?.Invoke(assignedPosition);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (splineController)
            OnChangePosition?.Invoke(splineController.Position);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHandlerTouched = true;
    }


    public void OnPointerExit(PointerEventData eventData)
    {
        isHandlerTouched = false;
    }
}

public enum Direction
{
    None = -1,
    Clockwise = 0,
    CounterClockwise = 1,
}