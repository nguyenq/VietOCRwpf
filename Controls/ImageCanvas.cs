using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml;

namespace VietOCR.Controls
{
    public class ImageCanvas : Canvas
    {
        private Point? dragStartPoint = null;
        private Point startPoint;
        private ContentControl canvasContent;

        private Dictionary<System.Windows.Media.SolidColorBrush, List<System.Drawing.Rectangle>> map;

        public Dictionary<System.Windows.Media.SolidColorBrush, List<System.Drawing.Rectangle>> SegmentedRegions
        {
            get { return map; }
            set {
                map = value;
                if (this.Children.Contains(canvasContent))
                {
                    this.Children.Clear();
                    // re-add selection box
                    this.Children.Add(canvasContent);
                }
                else
                {
                    this.Children.Clear();
                }
                DrawRegions(map);
            }
        }

        void DrawRegions(Dictionary<System.Windows.Media.SolidColorBrush, List<System.Drawing.Rectangle>> map)
        {
            if (map != null)
            {
                foreach (SolidColorBrush color in map.Keys)
                {
                    foreach (System.Drawing.Rectangle reg in map[color])
                    {
                        System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle();
                        rect.Stroke = color;
                        Canvas.SetLeft(rect, reg.X);
                        Canvas.SetTop(rect, reg.Y);
                        rect.Width = reg.Width;
                        rect.Height = reg.Height;

                        this.Children.Add(rect);
                    }
                }
            }
        }

        /// <summary>
        /// Gets Region of Interest.
        /// </summary>
        /// <returns></returns>
        public System.Drawing.Rectangle GetROI()
        {
            if (canvasContent == null || canvasContent.Width == 0 || canvasContent.Height == 0)
            {
                return System.Drawing.Rectangle.Empty;
            }
            return new System.Drawing.Rectangle((int)Canvas.GetLeft(canvasContent), (int)Canvas.GetTop(canvasContent), (int)canvasContent.Width, (int)canvasContent.Height);
        }

        public void Deselect()
        {
            dragStartPoint = null;
            this.Children.Remove(canvasContent);
            canvasContent.Width = 0;
            canvasContent.Height = 0;
            canvasContent.Visibility = Visibility.Hidden;
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            canvasContent = (ContentControl)this.FindName("canvasContent");
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            if (!this.Children.Contains(canvasContent))
            {
                // re-add selection box
                this.Children.Add(canvasContent);
            }

            startPoint = e.GetPosition(this);
            // Hit outside existing selection box
            if (canvasContent.InputHitTest(startPoint) == null)
            {
                canvasContent.Width = 0;
                canvasContent.Height = 0;
                Canvas.SetLeft(canvasContent, startPoint.X);
                Canvas.SetTop(canvasContent, startPoint.Y);
                canvasContent.Visibility = Visibility.Hidden;
                dragStartPoint = startPoint;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (e.LeftButton != MouseButtonState.Pressed)
            {
                this.dragStartPoint = null;
            }

            if (e.LeftButton == MouseButtonState.Released || !this.dragStartPoint.HasValue)
                return;

            var pos = e.GetPosition(this);

            // Set the position of rectangle
            var x = Math.Min(pos.X, startPoint.X);
            var y = Math.Min(pos.Y, startPoint.Y);

            // Set the dimenssion of the rectangle
            var w = Math.Max(pos.X, startPoint.X) - x;
            var h = Math.Max(pos.Y, startPoint.Y) - y;
            canvasContent.Width = w;
            canvasContent.Height = h;
            Canvas.SetLeft(canvasContent, x);
            Canvas.SetTop(canvasContent, y);
            canvasContent.Visibility = Visibility.Visible;

            if (this.dragStartPoint.HasValue)
            {
                //AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this);
                //if (adornerLayer != null)
                //{
                //    RubberbandAdorner adorner = new RubberbandAdorner(this, this.dragStartPoint);
                //    if (adorner != null)
                //    {
                //        adornerLayer.Add(adorner);
                //    }
                //}

                e.Handled = true;
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            this.dragStartPoint = null;
        }

        //protected override Size MeasureOverride(Size constraint)
        //{
        //    Size size = new Size();
        //    foreach (UIElement element in Children)
        //    {
        //        double left = Canvas.GetLeft(element);
        //        double top = Canvas.GetTop(element);
        //        left = double.IsNaN(left) ? 0 : left;
        //        top = double.IsNaN(top) ? 0 : top;

        //        element.Measure(constraint);

        //        Size desiredSize = element.DesiredSize;
        //        if (!double.IsNaN(desiredSize.Width) && !double.IsNaN(desiredSize.Height))
        //        {
        //            size.Width = Math.Max(size.Width, left + desiredSize.Width);
        //            size.Height = Math.Max(size.Height, top + desiredSize.Height);
        //        }
        //    }

        //    // add some extra margin
        //    size.Width += 10;
        //    size.Height += 10;
        //    return size;
        //}
    }
}
