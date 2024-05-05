using Engine;
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

namespace FEMmini
{
    /// <summary>
    /// Логика взаимодействия для InputInfo.xaml
    /// </summary>
    public partial class InputInfo : UserControl
    {
        public InputInfo()
        {
            InitializeComponent();
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var vm = (MainWindowVM)this.DataContext;
            nodesGrid.ItemsSource = vm.Info.NodeVM;
            elementsGrid.ItemsSource = vm.Info.ElementVM;
            stiffnessGrid.ItemsSource = vm.Info.StiffnessVM;
            loadsGrid.ItemsSource = vm.Info.LoadsVM;
        }
    }
}
