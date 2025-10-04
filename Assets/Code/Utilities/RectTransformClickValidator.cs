using UnityEngine;

public class RectTransformClickValidator : ScreenClickValidator
{
    [Header("Screen click validation settings")]
    [SerializeField] private RectTransform screenClickBlocker = null;

    public override bool IsClickValid(Camera _viewCamera, Vector2 _mousePosition, Vector2 _viewportPosition)
    {
        return !IsThisRectBlockingTheViewportClick(screenClickBlocker, _mousePosition);
    }
}
