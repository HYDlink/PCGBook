using System.Windows;

namespace PCG.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public CaveCAViewModel CaViewModel => (CaveCAViewModel)DataContext;
        public MainWindow()
        {
            var vm = new CaveCAViewModel() {View = this};
            DataContext = vm;
            InitializeComponent();
        }
    }
}