using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace JSReport {
    /// <summary>
    /// Simple view class to control the layout of each page in the document
    /// </summary>
    public class PageView : DocumentPage {
        public PageView(DrawingVisual dv, Point topLeft, Size pageSize)
                :base(dv, pageSize, new Rect(topLeft, pageSize), 
                      new Rect(topLeft, pageSize)) {
            TopLeft = topLeft;
            _size = pageSize;
            DrawCtxt = dv.RenderOpen();
        }

        public DrawingContext DrawCtxt { get; private set; }
        public void Close() {
            DrawCtxt.Close();
        }

        public Point TopLeft { get; set; }

        public Size _size;
        public double Width {
            get => _size.Width;
            set => _size.Width = value;
        }

        /// <summary>
        /// Draw a horizontal line the width of the view at the current y coordinate
        /// </summary>
        /// <param name="dc"></param>
        public void HorizontalRule(CellStyle style, double Y) {
            DrawCtxt.DrawLine(style.Pen, new Point(TopLeft.X, Y), 
                                    new Point(TopLeft.X + Width, Y));
        }
    }
}
