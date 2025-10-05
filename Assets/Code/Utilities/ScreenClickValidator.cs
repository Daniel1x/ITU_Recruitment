using System.Collections.Generic;
using UnityEngine;

public abstract class ScreenClickValidator : MonoBehaviour
{
    /// <summary> Represents the collection of currently active screen click validators. </summary>
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

    public abstract bool IsClickValid(Camera _viewCamera, Vector2 _screenPoint, Vector2 _viewportPosition);

    /// <summary> Determines whether any active validator is blocking a click at the specified screen and viewport positions. </summary>
    public static bool IsClickBlocked(Camera _viewCamera, Vector2 _screenPoint, Vector2 _viewportPosition)
    {
        for (int i = activeValidators.Count - 1; i >= 0; i--)
        {
            if (activeValidators[i] == null)
            {
                activeValidators.RemoveAt(i);
                continue;
            }

            if (!activeValidators[i].IsClickValid(_viewCamera, _screenPoint, _viewportPosition))
            {
                return true; // Click is blocked by this validator
            }
        }

        return false; // No validators blocked the click
    }

    /// <summary> Utility method to check if a RectTransform is blocking a click at the given screen point. </summary>
    public static bool IsThisRectBlockingTheScreenClick(RectTransform _rectTransform, Vector2 _screenPoint)
    {
        return _rectTransform != null 
            && RectTransformUtility.RectangleContainsScreenPoint(_rectTransform, _screenPoint);
    }
}
