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
using TsukiziSearch.Win.VM;

namespace TsukiziSearch.Win
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        MainVM _vm;

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _vm = new MainVM();
            this.DataContext = _vm;
        }

        /// <summary>
        /// 検索
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void clickSearch(object sender, RoutedEventArgs e)
        {
            await _vm.Search();
        }
    }
}
