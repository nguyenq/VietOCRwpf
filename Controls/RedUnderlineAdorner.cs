/// Adapted from https://github.com/lsd1991/SpellTextBox/blob/master/SpellTextBox/RedUnderlineAdorner.cs

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using System.Collections.ObjectModel;

namespace VietOCR.Controls
{
    public class RedUnderlineAdorner : Adorner
    {
        ScrollChangedEventHandler scrollChangedEventHandler;
        TextChangedEventHandler textChangedEventHandler;

        TextBox textbox;
        SpellCheckHelper mySpeller;
        Pen pen = CreateErrorPen();

        public RedUnderlineAdorner(TextBox textbox, SpellCheckHelper mySpeller) : base(textbox)
        {
            this.textbox = textbox;
            this.mySpeller = mySpeller;

            scrollChangedEventHandler = new ScrollChangedEventHandler(
                delegate
                {
                    SignalInvalidate();
                });

            textChangedEventHandler = new TextChangedEventHandler(
                delegate
                {
                    mySpeller.SpellCheck();
                    SignalInvalidate();
                });

            textbox.AddHandler(ScrollViewer.ScrollChangedEvent, scrollChangedEventHandler);
            textbox.TextChanged += textChangedEventHandler;
        }

        public void Dispose()
        {
            if (textbox != null)
            {
                textbox.RemoveHandler(ScrollViewer.ScrollChangedEvent, scrollChangedEventHandler);
                textbox.TextChanged -= textChangedEventHandler;
            }
        }

        void SignalInvalidate()
        {
            textbox.Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)InvalidateVisual);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (textbox != null)
            {
                ObservableCollection<System.Drawing.CharacterRange> errorRanges = mySpeller.GetSpellingErrorRanges;

                foreach (System.Drawing.CharacterRange mWord in errorRanges)
                {
                    Rect rectangleBounds = new Rect(this.AdornedElement.DesiredSize); 

                    Rect startRect = textbox.GetRectFromCharacterIndex(mWord.First);
                    Rect endRect = textbox.GetRectFromCharacterIndex(mWord.First + mWord.Length);

                    Rect startRectM = startRect; // textbox.GetRectFromCharacterIndex(Math.Min(mWord.First, textbox.Text.Length));
                    Rect endRectM = endRect; // textbox.GetRectFromCharacterIndex(Math.Min(mWord.First + mWord.Length, textbox.Text.Length));

                    startRectM.X += rectangleBounds.X;
                    startRectM.Y += rectangleBounds.Y;
                    endRectM.X += rectangleBounds.X;
                    endRectM.Y += rectangleBounds.Y;

                    if (rectangleBounds.Contains(startRectM) && rectangleBounds.Contains(endRectM))
                    {
                        // don't draw for word that spans multiple lines
                        if (Math.Abs(startRect.BottomLeft.Y - endRect.BottomRight.Y) < 8)
                        {
                            drawingContext.DrawLine(pen, startRect.BottomLeft, endRect.BottomRight);
                        }
                    }
                }
            }
        }

        private static Pen CreateErrorPen()
        {
            double size = 4.0;

            var geometry = new StreamGeometry();
            using (var context = geometry.Open())
            {
                context.BeginFigure(new Point(0.0, 0.0), false, false);
                context.PolyLineTo(new[] {
                    new Point(size * 0.25, size * 0.25),
                    new Point(size * 0.5, 0.0),
                    new Point(size * 0.75, size * 0.25),
                    new Point(size, 0.0)
                }, true, true);
            }

            var brushPattern = new GeometryDrawing
            {
                Pen = new Pen(Brushes.Red, 0.2),
                Geometry = geometry
            };

            var brush = new DrawingBrush(brushPattern)
            {
                TileMode = TileMode.Tile,
                Viewport = new Rect(0.0, size * 0.33, size * 3.0, size),
                ViewportUnits = BrushMappingMode.Absolute
            };

            var pen = new Pen(brush, size);
            pen.Freeze();

            return pen;
        }
    }
}
