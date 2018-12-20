using System.Collections.Generic;
using System.Windows.Media;

namespace JSReport {
    public class Styles {
        public Styles(Typeface defaultTypeface) {
            _defaultTypeface = defaultTypeface;
            CurrentStyle = new CellStyle(DefaultTypeface);
        }

        Typeface _defaultTypeface;
        public Typeface DefaultTypeface {
            get {
                return _defaultTypeface;
            }
            set {
                _defaultTypeface = value;
                CurrentStyle = new CellStyle(DefaultTypeface);
                foreach (var style in _cellStyles.Values) {
                    style.Typeface = value;
                }
            }
        }

        double _baseFontSize = 14;
        public double BaseFontSize {
            get {
                return _baseFontSize;
            }
            set {
                _baseFontSize = value;
                CurrentStyle.FontSize = _baseFontSize;
                foreach (var style in _cellStyles.Values) {
                    if (style.IsScaledFontSize) {
                        style.FontSize = _baseFontSize * style.FontScale;
                    }
                }
            }
        }

        public CellStyle CurrentStyle { get; private set; }
        Dictionary<string, CellStyle> _cellStyles = new Dictionary<string, CellStyle>();

        /// <summary>
        /// Manage named styles. If the style name exists in the style dictionary load it
        /// into the current style. If it doesn't exist then clone the default style and
        /// add it to the dictionary under the new name.
        /// </summary>
        /// <param name="name">name given to the style "normal","h1" etc</param>
        public void SetStyle(string name) {
            if (name == "default") {
                CurrentStyle = new CellStyle(DefaultTypeface);
                CurrentStyle.FontSize = CurrentStyle.FontScale * _baseFontSize;
            }

            if (_cellStyles.ContainsKey(name)) {
                CurrentStyle = _cellStyles[name];
            } else {
                CurrentStyle = new CellStyle(DefaultTypeface); 
                _cellStyles.Add(name, CurrentStyle);
            }
        }
    }
}
