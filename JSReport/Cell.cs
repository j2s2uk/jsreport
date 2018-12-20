using System.Globalization;
using System.Windows;
using System.Windows.Media;

/// <summary>
/// 
/// </summary>
namespace JSReport {
    public class Cell {
        public Cell(Point topLeft, Size size) {
            _topLeft = topLeft;
            _size = size;
        }

        FormattedText _fmtText;
        CellStyle _style;

        Point _topLeft;
        public Point TopLeft => _topLeft;
        Size _size;
        public Size Size => _size;
        public double Width { get => _size.Width; set => _size.Width = value; }
        public double Height { get => _size.Height; set => _size.Height = value; }
        public double Top { get => _topLeft.Y; set => _topLeft.Y = value; }
        public double Left { get => _topLeft.X; set => _topLeft.X = value; }
        // zero based page number of this cell
        public int PageNumber { get; set; }

        public void FormatText(string text, CellStyle style) {
            int start = 0;
            if (text.Length > 0 && text[0] == CellStyle.StyleMarker) {
                _style = style.Clone();
                start = _style.SetInlineStyles(text);
            } else {
                _style = style;
            }

            var s = text.Substring(start);
            _fmtText = new FormattedText(s.Length < 1 ? " " : s, 
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                _style.Typeface, _style.FontSize, Brushes.Black);
            if (_size.Height < _fmtText.Height) {
                _size.Height = _fmtText.Height;
            }
            _fmtText.MaxTextWidth = _size.Width;
            _fmtText.MaxTextHeight = _size.Height;
        }

        /// <summary>
        /// Draw the borders and shade cell if required  
        /// </summary>
        /// <param name="dc"></param>
        /// <param name="style"></param>
        /// <param name="topLeft"></param>
        /// <param name="size"></param>
        public void DrawBorders(DrawingContext dc, CellStyle style, Point topLeft, Size size) {
            // If all borders then draw a rectangle, shaded if necessary and return
            if (style.Borders == CellStyle.BordersStyles.All) {
                if (style.BorderAllIsRounded) {
                    dc.DrawRoundedRectangle(
                        style.IsShaded ? style.HighlightBrush : style.BackgroundBrush,
                        style.Borders == CellStyle.BordersStyles.None ? null : style.Pen,
                        new Rect(topLeft, size), 5.0, 5.0);
                } else {
                    dc.DrawRectangle(
                        style.IsShaded ? style.HighlightBrush : style.BackgroundBrush,
                        style.Borders == CellStyle.BordersStyles.None ? null : style.Pen,
                        new Rect(topLeft, size));
                }
                return;
            }

            // Otherwise .....
            // Shade the background if needed
            if (style.IsShaded) {
                dc.DrawRectangle(style.HighlightBrush, null, new Rect(new Point(_topLeft.X + 1, _topLeft.Y + 1), new Size(_size.Width - 2, _size.Height - 2)));
            }

            // Draw the individual borders specified
            if ((style.Borders & CellStyle.BordersStyles.Top) != 0) {
                dc.DrawLine(style.Pen, _topLeft, new Point(_topLeft.X + _size.Width, _topLeft.Y));
            }
            if ((style.Borders & CellStyle.BordersStyles.Bottom) != 0) {
                dc.DrawLine(style.Pen, 
                    new Point(_topLeft.X, _topLeft.Y + _size.Height), 
                    new Point(_topLeft.X + _size.Width, _topLeft.Y + _size.Height));
            }
            if ((style.Borders & CellStyle.BordersStyles.Left) != 0) {
                dc.DrawLine(style.Pen, _topLeft, new Point(_topLeft.X, _topLeft.Y + _size.Height));
            }
            if ((style.Borders & CellStyle.BordersStyles.Right) != 0) {
                dc.DrawLine(style.Pen, 
                new Point(_topLeft.X + _size.Width, _topLeft.Y), 
                new Point(_topLeft.X + _size.Width, _topLeft.Y + _size.Height));
            }
        }

        /// <summary>
        /// Draw the cell
        /// </summary>
        /// <param name="dc">Drawing context to draw to</param>
        public void Draw(DrawingContext dc) {
            // Always vertically align text
            double y = _topLeft.Y + ((_size.Height - _fmtText.Height) / 2);
            double x;
            switch (_style.Alignment) {
                case TextAlignment.Center:
                    x = ((_size.Width - _fmtText.Width) / 2) + _topLeft.X;
                    break;
                case TextAlignment.Right:
                    x = _topLeft.X + (_size.Width - _fmtText.Width - _style.Margin);
                    break;
                default:
                    x = _topLeft.X;
                    break;
            }
            
            DrawBorders(dc, _style, _topLeft, _size);
            if (_style.IsUnderlined) {
                var baseY = y + _fmtText.Height;
                dc.DrawLine(_style.Pen, new Point(x, baseY), new Point(x + _fmtText.Width, baseY));
            }
            dc.DrawText(_fmtText, new Point(x, y));
        }
    }
}
