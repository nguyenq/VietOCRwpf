using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace VietOCR
{
    /// <summary>
    /// Interaction logic for TrackbarDialog.xaml
    /// </summary>
    public partial class SliderDialog : Window
    {
        public string LabelText
        {
            set
            {
                this.label1.Content = value;
            }
        }

        private double prevValue;

        public delegate void HandleValueChange(object sender, ValueChangedEventArgs e);
        public event HandleValueChange ValueUpdated;

        public SliderDialog()
        {
            InitializeComponent();
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.ValueUpdated != null)
            {
                Slider bar = (Slider)sender;

                //reduce # of unnecessary value changed events
                if (Math.Abs(bar.Value - prevValue) >= bar.SmallChange)
                {
                    prevValue = bar.Value;
                    //Console.WriteLine(prevValue);
                    ValueChangedEventArgs args = new ValueChangedEventArgs(bar.Value);
                    this.ValueUpdated(this, args);
                }
            }
        }

        public class ValueChangedEventArgs : EventArgs
        {
            public double NewValue
            {
                get;
                set;
            }

            public ValueChangedEventArgs(double value)
                : base()
            {
                this.NewValue = value;
            }
        }

        public void SetForContrast()
        {
            this.slider.Minimum = 5;
            this.slider.Value = 25;
            this.slider.TickFrequency = 10;
        }

        private void buttonApply_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Visibility = Visibility.Hidden;
            this.Close();
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
            this.Close();
        }
    }
}
