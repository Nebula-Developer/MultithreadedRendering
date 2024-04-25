#nullable disable

using System.Numerics;
using Yoru.Graphics;

namespace Yoru.Input;

public class InputContext(Application app) : AppContext(app) {
    private readonly Dictionary<MouseButton, int> _pressedButtons = new();
    private readonly Dictionary<Key, int> _pressedKeys = new();
    private readonly Dictionary<MouseButton, int> _releasedButtons = new();
    private readonly Dictionary<Key, int> _releasedKeys = new();
    private Element hoverElement;
    
    public HashSet<Key> Keys { get; } = new();
    public HashSet<MouseButton> Buttons { get; } = new();
    public Vector2 MousePosition { get; private set; }
    
    public List<Element> InputElements { get; } = new();
    private List<Element> HoveredElements { get; } = new();
    private Dictionary<MouseButton, List<Element>> MouseDownElements { get; } = new();
    private bool MouseDown {
        get => Buttons.Count > 0;
    }
    
    // TODO: Make this all more efficient
    public void UpdateMousePosition(Vector2 position) {
        MousePosition = position;
        
        var maxElementPath = 0;
        Element maxElement = null;
        var path = 0;
        
        foreach (var element in InputElements) {
            path++;
            if (!element.MouseInteractions) continue;

            if (element.CheckMouseIntersect(position)) {
                if (path > maxElementPath) {
                    maxElementPath = path;
                    maxElement = element;
                }
                
                if (!HoveredElements.Contains(element)) {
                    HoveredElements.Add(element);
                    element.MouseEnter();
                }
            } else {
                if (HoveredElements.Contains(element)) {
                    HoveredElements.Remove(element);
                    element.MouseLeave();
                }
            }
        }
        
        if (hoverElement == null || !MouseDownElements.Values.Any(list => list.Contains(hoverElement))) {
            if (HoveredElements.Count > 0) {
                hoverElement = maxElement;
            }
            else hoverElement = null;
        } else if (hoverElement != null) hoverElement.MouseDrag();
    }
    
    public void Update() {
        UpdateCollection(_pressedKeys);
        UpdateCollection(_releasedKeys);
        UpdateCollection(_pressedButtons);
        UpdateCollection(_releasedButtons);
    }
    
    private void UpdateCollection<T>(Dictionary<T, int> collection) {
        var keysToRemove = new List<T>();
        
        foreach (var item in collection) {
            collection[item.Key]--;
            
            if (collection[item.Key] <= 0)
                keysToRemove.Add(item.Key);
        }
        
        keysToRemove.ForEach(key => collection.Remove(key));
    }
    
    public void HandleKeyDown(Key key) {
        Keys.Add(key);
        _pressedKeys.TryGetValue(key, out var count);
        _pressedKeys[key] = count + 1;
    }
    
    public void HandleKeyUp(Key key) {
        Keys.Remove(key);
        _releasedKeys.TryGetValue(key, out var count);
        _releasedKeys[key] = count + 1;
    }
    
    public void HandleMouseDown(MouseButton button) {
        Buttons.Add(button);
        _pressedButtons.TryGetValue(button, out var count);
        _pressedButtons[button] = count + 1;
        
        foreach (var element in HoveredElements) {
            if (!MouseDownElements.ContainsKey(button))
                MouseDownElements[button] = new();
            
            if (!MouseDownElements[button].Contains(element))
                MouseDownElements[button].Add(element);
        }
        
        hoverElement?.MouseDown(button);
    }
    
    public void HandleMouseUp(MouseButton button) {
        Buttons.Remove(button);
        _releasedButtons.TryGetValue(button, out var count);
        _releasedButtons[button] = count + 1;
        
        hoverElement?.MouseUp(button);
        if (MouseDownElements.ContainsKey(button))
            MouseDownElements[button].Clear();

        UpdateMousePosition(MousePosition);
    }
    
    public bool GetKey(Key key) => Keys.Contains(key);
    public bool GetKeyDown(Key key) => _pressedKeys.ContainsKey(key);
    public bool GetKeyUp(Key key) => _releasedKeys.ContainsKey(key);
    
    public bool GetMouseButton(MouseButton button) => Buttons.Contains(button);
    public bool GetMouseButtonDown(MouseButton button) => _pressedButtons.ContainsKey(button);
    public bool GetMouseButtonUp(MouseButton button) => _releasedButtons.ContainsKey(button);
}
