using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Text;

namespace JSReport {
    public class CellStyle {
        public CellStyle(Typeface defaultTypeface) {
            _normal = defaultTypeface;
            HighlightBrush = Brushes.LightGray;
            BackgroundBrush = Brushes.White;
            Pen = new Pen(Brushes.Black, 1.0);
            FontSize = 14;
            FontScale = 1;
            IsScaledFontSize = false;
            Margin = 5;
            Alignment = TextAlignment.Left;
        }

        public const char StyleMarker = '$';
        public const char StyleUnderlined = '_';
        public const char StyleLeftAlign = '<';
        public const char StyleRightAlign = '>';
        public const char StyleCentreAlign = '|';
        public const char StyleBoldFace = '!';
        public const char StyleShaded = '~';
        public const char StyleBorder = '#';
        public const char StyleRoundedBorder = 'O';

        /// <summary>
        /// Set the style from the encoded string passed.
        /// The string will be parsed until an unrecognised character or the end of the string is encountered.
        /// If the former then the remainder of the string will be returned or
        /// an empty string.
        /// </summary>
        /// <param name="encStyle">Style settings encoded as a string</param>
        /// <returns>Remaining characters not understood as style encoded values</returns>
        public int SetInlineStyles(string encStyle) {
            if (encStyle.Length < 1 || encStyle[0] != StyleMarker) return 0;
            var charsEaten = 0;

            bool awaitingBorder = false;

            foreach (var ch in encStyle) {
                if (awaitingBorder && IsBorderType(ch)) {
                    awaitingBorder = false;
                    charsEaten++;
                    continue;
                }

                switch (ch) {
                    case StyleMarker:
                        break;
                    case StyleUnderlined:
                        IsUnderlined = true;
                        break;
                    case StyleLeftAlign:
                        Alignment = TextAlignment.Left;
                        break;
                    case StyleRightAlign:
                        Alignment = TextAlignment.Right;
                        break;
                    case StyleCentreAlign:
                        Alignment = TextAlignment.Center;
                        break;
                    case StyleBoldFace:
                        IsBoldface = true;
                        break;
                    case StyleShaded:
                        IsShaded = true;
                        break;
                    case StyleBorder:
                        awaitingBorder = true;
                        break;
                    case ' ':
                        charsEaten++;
                        return charsEaten;
                    case StyleRoundedBorder:
                        BorderAllIsRounded = true;
                        break;
                    default:
                        return charsEaten;
                }
                charsEaten++;
            }
            return charsEaten;
        }

        public string InlineStyleString {
            get {
                var sb = new StringBuilder(StyleMarker);

                if (IsUnderlined) sb.Append(StyleUnderlined);

                if (IsBoldface) sb.Append(StyleBoldFace);
                if (BorderAllIsRounded) sb.Append(StyleRoundedBorder);
                switch (Alignment) {
                    case TextAlignment.Left:
                        sb.Append(StyleLeftAlign);
                        break;
                    case TextAlignment.Right:
                        sb.Append(StyleRightAlign);
                        break;
                    case TextAlignment.Center:
                        sb.Append(StyleCentreAlign);
                        break;
                    default:
                        break;
                }
                if (IsShaded) sb.Append(StyleShaded);
                if (Borders != BordersStyles.None) {
                    sb.Append(StyleBorder);
                    sb.Append("0123456789ABCDEF"[(int)Borders]);
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// 0 0000 = None
        /// 1 0001 = Left
        /// 2 0010 = Right
        /// 3 0011 = Vertical
        /// 4 0100 = Top
        /// 5 0101 = TopLeft
        /// 6 0110 = TopRight
        /// 7 0111 = Top + Vertical
        /// 8 1000 = Bottom
        /// 9 1001 = BottomLeft
        /// A 1010 = BottomRight
        /// B 1011 = Bottom + Vertical
        /// C 1100 = Horizontal
        /// D 1101 = Horizontal + Left
        /// E 1110 = Horizontal + Right
        /// F 1111 = All
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        bool IsBorderType(char c) {
            char ch = Char.ToUpper(c);
            int index = "0123456789ABCDEF".IndexOf(ch);

            if (index == -1) {
                return false;
            } else {
                Borders = (BordersStyles)index;
            }
            return true;
        }

        public CellStyle Clone() {
            return (CellStyle)this.MemberwiseClone();
        }

        Typeface _normal;
        Typeface _bold;

        public Typeface Typeface {
            get {
                if (IsBoldface) {
                    if (_bold == null) {
                        _bold = new Typeface(new FontFamily(_normal.FontFamily.ToString()),
                        FontStyles.Normal, FontWeights.Bold, FontStretches.Normal);
                    }
                    return _bold;
                } else {
                    return _normal;
                }
            }
            set {
                _normal = value;
                _bold = null;
            }
        }

        public bool BorderAllIsRounded { get; set; }
        public bool IsBoldface { get; set; }
        public bool IsShaded { get; set; }
        public bool IsUnderlined { get; set; }
        public double FontSize { get; set; }
        public double FontScale { get; set; }
        public bool IsScaledFontSize { get; set; }
        public Brush BackgroundBrush { get; set; }
        public Brush HighlightBrush { get; set; }
        public Pen Pen { get; set; }

        /// <summary>
        /// Defines whether to draw a border around the text
        /// </summary>
        [Flags]
        public enum BordersStyles {
            None = 0,
            Left = 1, Right = 2, Top = 4, Bottom = 8,
            Vertical = Left | Right,
            Horizontal = Top | Bottom,
            TopLeft = Top | Left,
            TopRight = Top | Right,
            BottomLeft = Bottom | Left,
            BottomRight = Bottom | Right,
            All = Horizontal | Vertical
        }

        public BordersStyles Borders { get; set; }
        public TextAlignment Alignment { get; set; }
        public double Margin { get; set; }

        double _height;
        public double Height {
            get {
                if (!(_height > 0)) {
                    var ft = GetFormattedText("X");
                    _height = ft.Height;
                }
                return _height;
            }
        }

        public FormattedText GetFormattedText(string text) {
            var ft = new FormattedText(text.Length < 1 ? " " : text,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                Typeface, FontSize, Brushes.Black);
            return ft;
        }
    }
}
