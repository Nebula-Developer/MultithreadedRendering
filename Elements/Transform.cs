#nullable disable

using OpenTK.Mathematics;
using SkiaSharp;

public class Transform {
    public Element Element {
        get => _element;
        set {
            if (_element != null && _element.Transform == this)
                _element.Transform = new();
            _element = value;
            if (_element != null && _element.Transform != this)
                _element.Transform = this;

            UpdateTransforms();
        }
    }
    private Element _element;

    public void ExecuteChildren(Action<Element> action) {
        if (Element == null) return;
        foreach (Element child in Element.Children)
            action(child);
    }

    public void UpdateTransforms() {
        UpdateSize();
        UpdatePosition();
    }

    public Vector2 RotationOffset = new(0, 0);

    public void ApplyToCanvas(SKCanvas canvas) {
        Vector2 rotationPos = new(PivotPosition.X + (Size.X * RotationOffset.X), PivotPosition.Y + (Size.Y * RotationOffset.Y));
        canvas.RotateDegrees(WorldRotation, rotationPos.X, rotationPos.Y);
        canvas.Translate(PivotPosition.X, PivotPosition.Y);
    }

    #region Size
    private Vector2 _parentSize => (Element?.Parent?.Transform.Size ?? Vector2.Zero);
    private Vector2 _parentScaleSize => _parentSize * ParentScale;

    public Vector2 ParentScale {
        get => _parentScale;
        set { _parentScale = value; UpdateSize(); }
    }
    private Vector2 _parentScale = Vector2.Zero;

    public bool ScaleWidth { get => ParentScale.X != 0; set => ParentScale = new(value ? 1 : 0, ParentScale.Y); }
    public bool ScaleHeight { get => ParentScale.Y != 0; set => ParentScale = new(ParentScale.X, value ? 1 : 0); }

    public Vector2 LocalSizeOffset {
        get => _localSizeOffset;
        set { _localSizeOffset = value; UpdateSize(); }
    }
    private Vector2 _localSizeOffset = Vector2.Zero;

    public Vector2 Size {
        get => _size;
        set {
            if (ParentScale.X != 0) _localSizeOffset.X = value.X - _parentScaleSize.X;
            if (ParentScale.Y != 0) _localSizeOffset.Y = value.Y - _parentScaleSize.Y;
            _size = value;

            ExecuteChildren(child => child.Transform.UpdateSize());
            UpdatePosition();
        }
    }
    private Vector2 _size = Vector2.Zero;

    public void UpdateSize() {
        if (ParentScale.X != 0) _size.X = _parentScaleSize.X + _localSizeOffset.X;
        if (ParentScale.Y != 0) _size.Y = _parentScaleSize.Y + _localSizeOffset.Y;
        ExecuteChildren(child => child.Transform.UpdateSize());
    }
    #endregion

    #region Position
    private Vector2 _parentPosition => (Element?.Parent?.Transform.WorldPosition ?? Vector2.Zero);

    public Vector2 LocalPosition {
        get => _localPosition;
        set { _localPosition = value; UpdatePosition(); }
    }
    private Vector2 _localPosition = Vector2.Zero;

    public Vector2 AnchorPosition {
        get => _anchorPosition;
        set { _anchorPosition = value; UpdatePosition(); }  
    }
    private Vector2 _anchorPosition = Vector2.Zero;
    
    public Vector2 OffsetPosition {
        get => _offsetPosition;
        set { _offsetPosition = value; UpdatePosition(); }
    }
    private Vector2 _offsetPosition = Vector2.Zero;

    public Vector2 WorldPosition {
        get => _worldPosition;
        set {
            _worldPosition = value;
            LocalPosition = _worldPosition - _parentPosition - ((_parentSize * AnchorPosition) - (Size * OffsetPosition));
        }
    }
    private Vector2 _worldPosition = Vector2.Zero;

    public Vector2 PivotPosition {
        get => _pivotPosition;
        set {
            LocalPosition = value - ((_parentSize * AnchorPosition) - (Size * OffsetPosition));
            UpdatePosition();
        }
    }
    private Vector2 _pivotPosition = Vector2.Zero;

    public void UpdatePosition() {
        _pivotPosition = (_parentSize * AnchorPosition) - (Size * OffsetPosition) + LocalPosition;
        _worldPosition = _parentPosition + _pivotPosition;
        ExecuteChildren(child => child.Transform.UpdatePosition());
    }
    #endregion

    #region Rotation
    private float _parentRotation => (Element?.Parent?.Transform.WorldRotation ?? 0);

    public float LocalRotation {
        get => _localRotation;
        set { _localRotation = value; UpdateRotation(); }
    }
    private float _localRotation = 0;

    public float WorldRotation {
        get => _worldRotation;
        set {
            _worldRotation = value;
            LocalRotation = _worldRotation - _parentRotation;
        }
    }
    private float _worldRotation = 0;

    public void UpdateRotation() {
        _worldRotation = _parentRotation + _localRotation;
        ExecuteChildren(child => child.Transform.UpdateRotation());
    }
    #endregion
}