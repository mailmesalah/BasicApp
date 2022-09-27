using System.Windows;
using WpfAccountClientApp.Registers;
using WpfAccountClientApp.Transactions;

namespace WpfAccountClientApp
{
    /// <summary>
    /// Interaction logic for Dashboard.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        
        public MainWindow()
        {
            InitializeComponent();            
        }

        private void ProductRegister_Click(object sender, RoutedEventArgs e)
        {
            ProductRegister pr = new ProductRegister();
            pr.Show();
        }

        private void Purchase_Click(object sender, RoutedEventArgs e)
        {
            Purchase p = new Purchase();
            p.Show();
        }

        private void PurchaseReturn_Click(object sender, RoutedEventArgs e)
        {
            PurchaseReturn pr = new PurchaseReturn();
            pr.Show();
        }        

        private void Sales_Click(object sender, RoutedEventArgs e)
        {
            Sales s = new Sales();
            s.Show();
        }

        private void SalesReturn_Click(object sender, RoutedEventArgs e)
        {
            SalesReturn sr = new SalesReturn();
            sr.Show();
        }

        private void StockAddition_Click(object sender, RoutedEventArgs e)
        {
            StockAddition sa = new StockAddition();
            sa.Show();
        }

        private void StockDeletion_Click(object sender, RoutedEventArgs e)
        {
            StockDeletion sd = new StockDeletion();
            sd.Show();
        }

    }
}
