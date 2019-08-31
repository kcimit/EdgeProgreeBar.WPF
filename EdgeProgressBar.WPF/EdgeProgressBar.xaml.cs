using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Markup;
using System.IO;

namespace ProgressBarExtension
{
    
    /// <summary>
    /// Interaction logic for CircularProgressBar.xaml
    /// </summary>
    public partial class EdgeProgressBar : UserControl
    {
        private readonly int r0;
        private readonly int l1;
        private readonly int l2;
        private readonly int l3;
        private readonly int l4;
        private readonly int lm1;
        private readonly int lm2;
        private readonly int backRectFill;
        private readonly int backRectStroke;
        private readonly int backEmpty;
        readonly bool bScaleBackToPixels = true;

        WriteableBitmap bm;
        int ProgressWidth;
        public EdgeProgressBar()
        {
            this.Loaded += (s, e) =>
            {
                if (bScaleBackToPixels)
                {
                    Matrix m = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice;
                    ScaleTransform dpiTransform = new ScaleTransform(1 / m.M11, 1 / m.M22);
                    if (dpiTransform.CanFreeze)
                        dpiTransform.Freeze();
                    this.LayoutTransform = dpiTransform;
                }

                ProgressWidth = (int)this.ActualWidth-20;
                bm = BitmapFactory.New(ProgressWidth, 8);
                image.Source = bm;
                image.Effect = null;

                RenderGeometry();
            };

            InitializeComponent();
            backRectFill = WriteableBitmapExtensions.ConvertColor((Color)ColorConverter.ConvertFromString("#B4B4B4"));
            backRectStroke = WriteableBitmapExtensions.ConvertColor((Color)ColorConverter.ConvertFromString("#ABABAB"));
            backEmpty = WriteableBitmapExtensions.ConvertColor((Color)ColorConverter.ConvertFromString("#FFFFFF"));
            r0 = WriteableBitmapExtensions.ConvertColor((Color)ColorConverter.ConvertFromString("#4687E8"));
            l1 = WriteableBitmapExtensions.ConvertColor((Color)ColorConverter.ConvertFromString("#79C0F9"));
            l2 = WriteableBitmapExtensions.ConvertColor((Color)ColorConverter.ConvertFromString("#6DB3F7"));
            l3 = WriteableBitmapExtensions.ConvertColor((Color)ColorConverter.ConvertFromString("#63A8F7"));
            l4 = WriteableBitmapExtensions.ConvertColor((Color)ColorConverter.ConvertFromString("#589CF5"));
            lm1 = WriteableBitmapExtensions.ConvertColor((Color)ColorConverter.ConvertFromString("#59A3F6"));
            lm2 = WriteableBitmapExtensions.ConvertColor((Color)ColorConverter.ConvertFromString("#5095F6"));
        }

        public double Percentage
        {
            get { return (double)GetValue(PercentageProperty); }
            set { SetValue(PercentageProperty, value); }
        }

        public int ProgressPosition
        {
            get { return (int)GetValue(ProgressPositionProperty); }
            set { SetValue(ProgressPositionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Percentage
        public static readonly DependencyProperty PercentageProperty =
            DependencyProperty.Register("Percentage", typeof(double), typeof(EdgeProgressBar), new PropertyMetadata(65d, new PropertyChangedCallback(OnPercentageChanged)));

        // Using a DependencyProperty as the backing store for Position
        public static readonly DependencyProperty ProgressPositionProperty =
            DependencyProperty.Register("ProgressPosition", typeof(int), typeof(EdgeProgressBar), new PropertyMetadata(0, new PropertyChangedCallback(OnPropertyChanged)));

        private static void OnPercentageChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            EdgeProgressBar edge = sender as EdgeProgressBar;
            edge.ProgressPosition = (int)(edge.Percentage*edge.ProgressWidth/100);
        }

        private static void OnPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            EdgeProgressBar edge = sender as EdgeProgressBar;
            if (edge.bm != null)
                edge.RenderGeometry();
        }

        public void RenderGeometry()
        {
            var pos = ProgressPosition-1;
            var width = ProgressWidth;
            unsafe
            {
                using (var cx = bm.GetBitmapContext())
                {
                    if (pos == 0)
                        StraightLine(0, 0, 0, 5, r0, cx);
                    if (pos < width)
                    {
                        StraightLine(pos + 1, 0, width - 1, 0, backRectStroke, cx);
                        StraightLine(pos + 1, 0, pos + 1, 5, backRectStroke, cx);

                        StraightLine(pos + 1, 5, width - 1, 5, backRectStroke, cx);
                        StraightLine((width - pos - 1), 0, width - 1, 5, backRectStroke, cx);

                        if ((width - pos - 2) > 0)
                            for (int y = 1; y < 5; y++)
                                StraightLine(pos + 2, y, width - 2, y, backRectFill, cx);
                        StraightLine(pos + 1, 6, width - 1, 6, backEmpty, cx);
                        StraightLine(pos + 1, 7, width - 1, 7, backEmpty, cx);
                    }
                    if (pos > 2)
                    {
                        StraightLine(1, 1, Math.Max(pos - 1, 1), 1, l1, cx);
                        StraightLine(1, 4, Math.Max(pos - 1, 1), 4, l4, cx);
                        cx.Pixels[2 * cx.Width + 1] = l2;
                        cx.Pixels[2 * cx.Width + pos-1] = l2;
                        cx.Pixels[3 * cx.Width + 1] = l3;
                        cx.Pixels[3 * cx.Width + pos - 1] = l3;
                    }

                    //inner rect
                    if (pos > 4)
                    {
                        StraightLine(2, 2, pos - 2, 2, lm1, cx);
                        StraightLine(2, 3, pos - 2, 3, lm2, cx);
                    }
                    if (pos > 1)
                    {
                        StraightLine(0, 0, 0, 5, r0, cx);
                        StraightLine(0, 0, pos, 0, r0, cx);

                        StraightLine(pos, 0, pos, 5, r0, cx);
                        StraightLine(0, 5, pos, 5, r0, cx);

                        StraightLine(0, 6, pos, 6, backRectFill, cx);
                        StraightLine(0, 7, pos, 7, backRectStroke, cx);
                    }
                }
            }
            /*
            using (var fileStream = new FileStream(@"c:\temp\res2.jpg", FileMode.Create))
            {
                var w = bm.Clone();
                w.WriteJpeg(fileStream);
            }*/
        }

        void StraightLine(int X1, int Y1, int X2, int Y2, int col, BitmapContext cx)
        {
            unsafe
            {
                if (Y2 != Y1)
                    for (int y = Y1; y <= Y2; y++)
                        cx.Pixels[y * cx.Width + X1] = col;
                if (X2 != X1)
                    for (int x = X1; x <= X2; x++)
                        cx.Pixels[Y1 * cx.Width + x] = col;
            }
        }
    }
}
