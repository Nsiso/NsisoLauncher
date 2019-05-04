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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NsisoLauncher.Controls
{
    /// <summary>
    /// HeadSculControl.xaml 的交互逻辑
    /// </summary>
    public partial class HeadSculControl : UserControl
    {
        public HeadSculControl()
        {
            InitializeComponent();
        }

        public async Task RefreshIcon(string uuid)
        {
            NsisoLauncherCore.Net.CrafatarAPI.APIHandler handler = new NsisoLauncherCore.Net.CrafatarAPI.APIHandler();
            progressRing.IsActive = true;
            iconImage.Source = await handler.GetHeadSculSource(uuid);
            progressRing.IsActive = false;
        }
    }
}
