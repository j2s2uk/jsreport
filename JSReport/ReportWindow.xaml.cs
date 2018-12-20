using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Threading;
using System.Windows.Media;

namespace JSReport {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ReportWindow : Window {
        public ReportWindow(DocumentSource docSource) {
            _docSource = docSource;
            InitializeComponent();
        }

        ReportSettings _settings = new ReportSettings();
        Size _previewPageSize;
        ReportPaginator _paginator;
        DocumentSource _docSource;
        const double PRINT_PAGE_MARGIN = 96 * 0.75;
        const double SCREEN_PAGE_MARGIN = 30 * 0.75;

        /// <summary>
        ///  Use a timer to hold off from redrawing when a user resizes the window.
        ///  Wait until the size hasn't changed for at least 200ms. In effect when the user 
        ///  lets go of the window drag handles.
        /// </summary>
        System.Timers.Timer _timer = new System.Timers.Timer(200);

        delegate void Redraw();

        void buildInvoice_Click(object sender, RoutedEventArgs e) {
            printPreviewSurface.ClearVisual();
            ShowReport();
        }

        async void Window_Initialized(object sender, EventArgs e) {
            //Console.WriteLine("Window_Initialized");
            invoicePageGrid.DataContext = _settings;
            fontsCombo.ItemsSource = Fonts.SystemFontFamilies;
            fontsCombo.SelectedIndex = 0;
            _settings.PropertyChanged += SettingsChanged;
            Title = (await _docSource.GetCurrentDocument()).Name;

            if (_docSource.Count > 1) {
                choiceListBox.ItemsSource = _docSource.DocumentList;
                documentSelectPanel.Visibility = Visibility.Visible;
                choiceListBox.SelectedIndex = 0;
            }
            else {
                documentSelectPanel.Visibility = Visibility.Collapsed;
            }
            choiceListBox.SelectionChanged += choiceListBox_SelectionChanged;
            _timer.Elapsed += OnTimeElapsedEvent;
            _timer.AutoReset = false;
            LazyReDraw();
        }

        public void SettingsChanged(object sender, EventArgs e) {
            var settings = (ReportSettings)sender;
            _docSource.Styles.DefaultTypeface = new Typeface(new FontFamily(settings.FontFamily), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
            _docSource.Styles.BaseFontSize = settings.FontSize;
            LazyReDraw();
        }

        void Window_Closed(object sender, EventArgs e) {
            _timer.Stop();
            _timer.Dispose();
        }

        /// <summary>
        /// Start a timer to invoke a redraw at some point in the future
        /// </summary>
        void LazyReDraw() {
            _timer.Stop();
            _timer.Start();
        }

        void OnTimeElapsedEvent(object source, System.Timers.ElapsedEventArgs e) {
            _timer.Stop();
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Redraw)DoRedraw);
        }

        async void DoRedraw() {
            SetCanvasSize();
            var doc = await _docSource.GetCurrentDocument();
            BuildReport(doc);
            ShowReport();
        }

        void ShowReport() {
            var page = _paginator.CurrentPage;
            currentPageLabel.Content = String.Format("{0} of {1}", _paginator.CurrentPageNo + 1, _paginator.PageCount);
            //var dpiScale = DpiScale.PixelsPerDip;
            if (page != DocumentPage.Missing) {
                printPreviewSurface.ClearVisual();
                printPreviewSurface.AddVisual(page.Visual);
                printPreviewSurface.Focus();
                previewScrollViewer.ScrollToHome();
            }
            nextPage.IsEnabled = !_paginator.IsLastPage;
            prevPage.IsEnabled = !_paginator.IsFirstPage;
        }

        async void printButton_Click(object sender, RoutedEventArgs e) {
            PrintDialog printDialog = new PrintDialog();

            if (printDialog.ShowDialog() == true) {
                var doc = await _docSource.GetCurrentDocument();
                ReportPaginator paginator =
                    new ReportPaginator(doc, _settings.PrintMarginSize,
                    new Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight));

                printDialog.PrintDocument(paginator, "Invoice");
            }
        }

        void BuildReport(Document doc) {
            _paginator = new ReportPaginator(doc, SCREEN_PAGE_MARGIN, 
                new Size(printPreviewSurface.Width, printPreviewSurface.Height));
        }

        void SetCanvasSize() {
            printPreviewSurface.ClearVisual();
            printPreviewSurface.Width = _previewPageSize.Width - 20.0;
            printPreviewSurface.Height = printPreviewSurface.Width * (297.0 / 210.0);
        }

        void previewScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e) {
            _previewPageSize = e.NewSize;
            LazyReDraw();
        }

        void prevPage_Click(object sender, RoutedEventArgs e) {
            _paginator.PrevPage();
            ShowReport();
        }

        void nextPage_Click(object sender, RoutedEventArgs e) {
            _paginator.NextPage();
            ShowReport();
        }

        async void choiceListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (e.AddedItems.Count > 0) {
                await _docSource.SetCurrentDocument((string)e.AddedItems[0]);
                DoRedraw();
            }
        }
    }
}
