using System;
using System.Windows;

namespace GUI.Utils;

public class ResizeEndHelper
{
    private readonly UIElement _uiElement;

    private bool _isResizing;
    private Size _sizeOnPreviousTick;

    public event Action ResizeEnd;

    public ResizeEndHelper(UIElement uiElement)
    {
        _uiElement = uiElement;
    }

    public void OnTick()
    {
        if (_uiElement.RenderSize != _sizeOnPreviousTick)
        {
            _isResizing = true;
            _sizeOnPreviousTick = _uiElement.RenderSize;
            return;
        }

        if (_isResizing)
        {
            _sizeOnPreviousTick = _uiElement.RenderSize;
            ResizeEnd?.Invoke();
            _isResizing = false;
        }
    }
}