using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NsisoLauncher.Views.Controls
{
    /// <summary>
    /// Skin3dViewer.xaml 的交互逻辑
    /// </summary>
    public partial class Skin3dViewer : UserControl
    {
        public BitmapImage SkinImage
        {
            get { return (BitmapImage)GetValue(SkinImageProperty); }
            set { SetValue(SkinImageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SkinImage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SkinImageProperty =
            DependencyProperty.Register("SkinImage", typeof(BitmapImage), typeof(Skin3dViewer), new PropertyMetadata(null));




        public Skin3dViewer()
        {
            InitializeComponent();
        }
    }
}
