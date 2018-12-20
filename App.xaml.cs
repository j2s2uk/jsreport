using System;
using System.Windows;
using System.Windows.Media;

namespace JSReport {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        public static string CmdLine;

        void Application_Startup(object sender, StartupEventArgs e) {
            var source = e.Args.Length == 1 ?
                e.Args[0] :
                Environment.CurrentDirectory;

            var typeface = new Typeface(new FontFamily("Arial"), 
                FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
            var sourceDocs = DocumentSource.GetDocumentSource(source, typeface);
            var mainWindow = new ReportWindow(sourceDocs);
            mainWindow.Show();
        }
    }
}
