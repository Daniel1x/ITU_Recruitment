using UnityEngine;

public class RectTransformClickValidator : ScreenClickValidator
{
    [Header("Screen click validation settings")]
    [SerializeField] private RectTransform screenClickBlocker = null;

    public override bool IsClickValid(Camera _viewCamera, Vector2 _screenPoint, Vector2 _viewportPosition)
    {
        return !IsThisRectBlockingTheScreenClick(screenClickBlocker, _screenPoint); // Click is valid if not blocked by the RectTransform
    }
}
