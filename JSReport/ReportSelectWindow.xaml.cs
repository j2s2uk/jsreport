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

namespace JSReport
{
    /// <summary>
    /// Interaction logic for ReportSelectWindow.xaml
    /// </summary>
    public partial class ReportSelectWindow : Window
    {
        ReportSelectWindow()
        {
            InitializeComponent();
        }

        public ReportSelectWindow(List<string> choices) : this() {
            choiceListBox.ItemsSource = choices;
        }

        public string Selection {
            get {
                if (choiceListBox.SelectedIndex < 0) return "";
                return choiceListBox.Items[choiceListBox.SelectedIndex].ToString();
            }
        }

        void okButton_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
            Close();
        }
    }
}
