﻿#nullable disable

using SkiaSharp;
using Yoru.Elements;
using Yoru.Input;
using Yoru.Platforms.GL;

namespace Yoru.ProgramTests;

public class GridTestApp : Application {
    public FillGridElement fillGrid = new() {
        RowSpacing = 10,
        ColumnSpacing = 10,
        Transform = new() {
            ScaleWidth = true,
            ScaleHeight = true
        }
    };
    public GridElement grid = new() {
        MaxRows = 4,
        MaxColumns = 4,
        RowSpacing = 10,
        ColumnSpacing = 10,
        ElementWidth = 100,
        ElementHeight = 100
    };
    
    protected override void OnLoad() {
        base.OnLoad();
        for (var i = 0; i < 16; i++) {
            grid.AddChild(new BoxElement {
                Color = i % 2 == 0 ? SKColors.Red : SKColors.Green,
                Transform = new() {
                    Size = new(100)
                }
            });
        }
        
        for (var i = 0; i < 16; i++) {
            fillGrid.AddChild(new BoxElement {
                Color = i % 2 == 0 ? SKColors.Orange : SKColors.Blue,
                Transform = new() {
                    Size = new(100)
                }
            });
        }
        
        Element.AddChild(fillGrid);
    }
    
    protected override void OnUpdate() {
        base.OnUpdate();
        grid.RowSpacing = 10 + (float)Math.Sin(UpdateTime.Time) * 10;
        grid.ColumnSpacing = 10 + (float)Math.Cos(UpdateTime.Time) * 10;
    }
    
    protected override void OnKeyDown(Key key) {
        base.OnKeyDown(key);
        if (key == Key.Up) grid.MaxRows++;
        if (key == Key.Down) grid.MaxRows--;
        if (key == Key.Left) grid.MaxColumns--;
        if (key == Key.Right) grid.MaxColumns++;
        if (key == Key.X) {
            grid.FlowDirection = grid.FlowDirection == GridFlowDirection.Column ? GridFlowDirection.Row : GridFlowDirection.Column;
            fillGrid.FlowDirection = fillGrid.FlowDirection == GridFlowDirection.Column ? GridFlowDirection.Row : GridFlowDirection.Column;
        }
    }
}

public static class Program {
    public static void Main(string[] args) {
        GLWindow programTestWindow = new();
        Console.CancelKeyPress += (s, e) => {
            programTestWindow.Close();
            e.Cancel = true;
        };
        programTestWindow.App = new GridTestApp();
        programTestWindow.Run();
        Console.WriteLine("Program killed");
        programTestWindow.Dispose();
    }
}
