using System;
using DefaultNamespace;
using FluffyUnderware.Curvy;
using FluffyUnderware.Curvy.Controllers;
using UnityEngine;

public class Handler : MonoBehaviour
{
    public Action<Direction, Handler> OnSelectDirection { get; set; }
    public Action<float, Handler> OnHandling { get; set; }
    public float Position { get; private set; }

    [SerializeField] private float interactionRadius;

    private FadedImage handlerImage;
    private FadedImage dottedImage;

    private DragController dragController;

    public void SetDirection(Direction direction)
    {
        if (dragController)
            dragController.ForceSetDirection(direction);
    }

    public void UpdateMaxValue(float value, Handler handler)
    {
        if(handler == this)
            return;
        dragController.UpdateMaxPosition(value);
    }

    public void SetDottedView(FadedImage image)
    {
        dottedImage = image;
    }

    public void Drop()
    {
        if(dottedImage)
            dottedImage.Fade();
        if(handlerImage)
            handlerImage.Fade();
        if(dragController)
            dragController.Drop();
        enabled = false;
    }

    private void OnEnable()
    {
        dragController ??= GetComponentInChildren<DragController>();
        handlerImage ??= GetComponentInChildren<FadedImage>();
        if (!dragController)
        {
            enabled = false;
            return;
        }

        dragController.Initiate(interactionRadius);
        dragController.OnApplyDirection += OnApplyDirection;
        dragController.OnChangePosition += OnChangePosition;
    }

    private void OnDisable()
    {
        if (!dragController)
            return;

        dragController.OnApplyDirection -= OnApplyDirection;
        dragController.OnChangePosition -= OnChangePosition;
        OnSelectDirection = null;
        OnHandling = null;
    }

    private void OnChangePosition(float value)
    {
        Position = value;
        if (dottedImage)
            dottedImage.SetPosition(value);
        OnHandling?.Invoke(value, this);
    }

    private void OnApplyDirection(Direction direction)
    {
        if (dragController)
            dragController.OnApplyDirection -= OnApplyDirection;
        OnSelectDirection?.Invoke(direction, this);
    }
}