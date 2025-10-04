using System.Collections.Generic;
using UnityEngine;

public abstract class ScreenClickValidator : MonoBehaviour
{
    private static readonly List<ScreenClickValidator> activeValidators = new();

    protected virtual void OnEnable()
    {
        if (!activeValidators.Contains(this))
        {
            activeValidators.Add(this);
        }
    }

    protected virtual void OnDisable()
    {
        activeValidators.Remove(this);
    }

    public abstract bool IsClickValid(Camera _viewCamera, Vector2 _mousePosition, Vector2 _viewportPosition);

    public static bool IsAnyValidatorBlockingClick(Camera _viewCamera, Vector2 _mousePosition, Vector2 _viewportPosition)
    {
        for (int i = activeValidators.Count - 1; i >= 0; i--)
        {
            if (activeValidators[i] == null)
            {
                activeValidators.RemoveAt(i);
                continue;
            }

            if (!activeValidators[i].IsClickValid(_viewCamera, _mousePosition, _viewportPosition))
            {
                return true; // Click is blocked by this validator
            }
        }

        return false; // No validators blocked the click
    }

    public static bool IsThisRectBlockingTheViewportClick(RectTransform _rectTransform, Vector2 _screenPoint)
    {
        return _rectTransform != null 
            && RectTransformUtility.RectangleContainsScreenPoint(_rectTransform, _screenPoint);
    }
}
