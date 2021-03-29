using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace NsisoLauncher.Views.Controls
{
    /// <summary>
    /// Skin3dViewer.xaml 的交互逻辑
    /// </summary>
    public partial class Skin3dViewer : UserControl
    {
        public Uri SkinUrl
        {
            get { return (Uri)GetValue(SkinUrlProperty); }
            set { SetValue(SkinUrlProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SkinUrl.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SkinUrlProperty =
            DependencyProperty.Register("SkinUrl", typeof(Uri), typeof(Skin3dViewer), new PropertyMetadata(null));


        public Skin3dViewer()
        {
            InitializeComponent();
        }
    }
}
