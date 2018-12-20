using System.ComponentModel;

namespace JSReport {
    public class ReportSettings : INotifyPropertyChanged {
        public ReportSettings() {
            _fontFamily = "Arial";
            _fontSize = 14;
            _printMarginSize = 72; // 96 * 0.75 original 3/4"
        }

        string _fontFamily;
        public string FontFamily {
            get => _fontFamily;
            set {
                _fontFamily = value;
                OnPropertyChanged(new PropertyChangedEventArgs("FontFamily"));
            }
        }

        double _fontSize;
        public int FontSize {
            get => (int)_fontSize;
            set {
                _fontSize = (double)value;
                OnPropertyChanged(new PropertyChangedEventArgs("FontSize"));
            }
        }

        double _printMarginSize;
        public int PrintMarginSize {
            get => (int)_printMarginSize;
            set {
                _printMarginSize = (double)value;
                OnPropertyChanged(new PropertyChangedEventArgs("PrintMarginSize"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(PropertyChangedEventArgs e) {
            if (PropertyChanged != null) {
                PropertyChanged(this, e);
            }
        }
    }
}
