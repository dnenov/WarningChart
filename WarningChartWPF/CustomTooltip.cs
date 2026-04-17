using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Kernel;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.Drawing;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.VisualElements;
using LiveChartsCore;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiveChartsCore.SkiaSharpView.VisualElements;
using System.Windows.Controls;

namespace WC.WarningChartWPF
{

    public class CustomTooltip : IChartTooltip<SkiaSharpDrawingContext>
    {
        private StackPanel<RoundedRectangleGeometry, SkiaSharpDrawingContext>? _stackPanel;
        private static readonly int s_zIndex = 10100;
        private readonly SolidColorPaint _backgroundPaint = new(new SKColor(250, 250, 250)) { ZIndex = s_zIndex };
        private readonly SolidColorPaint _fontPaint = new(new SKColor(40, 40, 40)) { ZIndex = s_zIndex + 1 };

        public void Show(IEnumerable<ChartPoint> foundPoints, Chart<SkiaSharpDrawingContext> chart)
        {
            if (_stackPanel is null)
            {
                _stackPanel = new StackPanel<RoundedRectangleGeometry, SkiaSharpDrawingContext>
                {
                    Padding = new Padding(12),
                    Orientation = ContainerOrientation.Vertical,
                    HorizontalAlignment = Align.Start,
                    VerticalAlignment = Align.Middle,
                    BackgroundPaint = _backgroundPaint,
                };

                _stackPanel
                    .Animate(
                        new Animation(EasingFunctions.BounceOut, TimeSpan.FromSeconds(1)),
                        nameof(_stackPanel.X),
                        nameof(_stackPanel.Y));
            }

            // clear the previous elements.
            foreach (var child in _stackPanel.Children.ToArray())
            {
                _ = _stackPanel.Children.Remove(child);
                chart.RemoveVisual(child);
            }

            foreach (var point in foundPoints)
            {
                var sketch = ((IChartSeries<SkiaSharpDrawingContext>)point.Context.Series).GetMiniaturesSketch();
                var relativePanel = sketch.AsDrawnControl(s_zIndex);
                var name = (point.Context.DataSource as WarningChartPoint)?.Name;

                var label = new LabelVisual
                {
                    Text = point.Coordinate.PrimaryValue.ToString(),
                    Paint = _fontPaint,
                    TextSize = 15,
                    Padding = new Padding(20, 0, 0, 0),
                    ClippingMode = ClipMode.None, // required on tooltips 
                    VerticalAlignment = Align.Start,
                    HorizontalAlignment = Align.Start
                };

                var sp = new StackPanel<RoundedRectangleGeometry, SkiaSharpDrawingContext>
                {
                    Padding = new Padding(8, 2),
                    VerticalAlignment = Align.Middle,
                    HorizontalAlignment = Align.Middle,
                    Children =
                    {
                        relativePanel,
                        label
                    }
                };

                var tb = new LabelVisual
                {
                    Text = name,
                    Paint = _fontPaint,
                    TextSize = 13,
                    Padding = new Padding(8, 10, 20, 0),
                    ClippingMode = ClipMode.None, // required on tooltips
                    VerticalAlignment = Align.Start,
                    HorizontalAlignment = Align.Start,
                    MaxWidth = 240,
                };

                _stackPanel?.Children.Add(sp);
                _stackPanel?.Children.Add(tb);

                // For severity-grouped points: append a "Top types" breakdown,
                // one label per line so each respects the tooltip's natural width.
                var breakdown = (point.Context.DataSource as WarningChartPoint)?.Breakdown;
                if (breakdown != null && breakdown.Count > 0)
                {
                    const int TopN = 4;
                    const int MaxChars = 42;

                    _stackPanel?.Children.Add(new LabelVisual
                    {
                        Text = "Top warning types:",
                        Paint = _fontPaint,
                        TextSize = 11,
                        Padding = new Padding(8, 8, 20, 2),
                        ClippingMode = ClipMode.None,
                        VerticalAlignment = Align.Start,
                        HorizontalAlignment = Align.Start,
                    });

                    foreach (var kv in breakdown.Take(TopN))
                    {
                        var typeText = kv.Key ?? "(unnamed)";
                        if (typeText.Length > MaxChars) typeText = typeText.Substring(0, MaxChars - 3) + "...";
                        _stackPanel?.Children.Add(new LabelVisual
                        {
                            Text = $"  {kv.Value} \u00d7 {typeText}",
                            Paint = _fontPaint,
                            TextSize = 11,
                            Padding = new Padding(8, 0, 20, 0),
                            ClippingMode = ClipMode.None,
                            VerticalAlignment = Align.Start,
                            HorizontalAlignment = Align.Start,
                        });
                    }

                    var remainder = breakdown.Skip(TopN).Sum(kv => kv.Value);
                    if (remainder > 0)
                    {
                        _stackPanel?.Children.Add(new LabelVisual
                        {
                            Text = $"  + {remainder} more across {breakdown.Count - TopN} type(s)",
                            Paint = _fontPaint,
                            TextSize = 11,
                            Padding = new Padding(8, 0, 20, 0),
                            ClippingMode = ClipMode.None,
                            VerticalAlignment = Align.Start,
                            HorizontalAlignment = Align.Start,
                        });
                    }
                }
            }

            var size = _stackPanel.Measure(chart);

            var location = foundPoints.GetTooltipLocation(size, chart);

            _stackPanel.X = location.X;
            _stackPanel.Y = location.Y;

            chart.AddVisual(_stackPanel);
        }

        public void Hide(Chart<SkiaSharpDrawingContext> chart)
        {
            if (chart is null || _stackPanel is null) return;
            chart.RemoveVisual(_stackPanel);
        }
    }
}
