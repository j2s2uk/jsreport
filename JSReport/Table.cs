using System.Collections.Generic;
using System.Windows;

namespace JSReport {
    /// <summary>
    /// Keep track of a layout of cells in one or more columns using absolute coordinates
    /// </summary>
    public class Table {
        public Table(double top, Size pageSize, double margin) {
            Top = Bottom = top;
            _pageSize = pageSize;
            RowHeight = 0;
            _columns = 1;
            _column = 0;
            Span = 1;
            _moveVertical = true;
            _cell = new Cell(new Point(margin, top), new Size(pageSize.Width, 0));
        }

        bool _moveVertical;
        public double Top { get; set; }
        Size _pageSize;
        public double Width => _pageSize.Width;
        public double CurrentY { get; private set; }
        public double Bottom { get; private set; }
        public double RowHeight { get; set; }
        public int Span { get; set; }

        int _column;
        public void SetColumnVertical(int column) { 
            _column = column;
            _moveVertical = true;
            CurrentY = Top;
        }

        int _columns;
        public void ResetColumns(int columns, double margin) {
            _columns = columns;
            MoveDown(margin);
            RowHeight = 0;
            _column = 0;
            Span = 1;
            CurrentY = Top = Bottom;
            _moveVertical = (_columns < 2);
        }

        // Calculated fields
        public double Height => Bottom - Top;
        public double ColumnWidth => Width / _columns;

        Cell _cell;
        public IEnumerable<Cell> PrepareCells(string text, CellStyle style, double margin) {
            _cell.Left = margin + (ColumnWidth * _column);
            _cell.Top = margin + (CurrentY % _pageSize.Height);
            _cell.Width = Span * ColumnWidth;
            _cell.Height = RowHeight;
            _cell.PageNumber = (int)(CurrentY / _pageSize.Height);
            if (text.StartsWith(";")) {
                var subCells = text.Substring(1).Split(';');
                _cell.Width = _cell.Width / subCells.Length;
                foreach (var field in subCells) {
                    _cell.FormatText(field, style);
                    if (_cell.Height > RowHeight) RowHeight = _cell.Height;
                    yield return _cell;
                    _cell.Left += _cell.Width;
                }
            } else {
                _cell.FormatText(text, style);
                if (_cell.Height > RowHeight) RowHeight = _cell.Height;
                yield return _cell;
            }
            Move(margin);
        }

        public void Move(double margin) {
            if (_moveVertical) {
                MoveDown(margin);
            } else {
                MoveRight(margin);
            }
        }

        void MoveRight(double margin) {
            //System.Diagnostics.Debug.Print("MoveRight");
            _column += Span;
            if (_column >= _columns) {
                _column = 0;
                MoveDown(margin);
            }
        }

        void MoveDown(double margin) {
            //System.Diagnostics.Debug.Print(RowHeight.ToString());
            CurrentY += RowHeight;
            // If entire cell won't fit on page move down to the top of the next page
            if (((CurrentY % _pageSize.Height) + RowHeight) > _pageSize.Height) {
                int page =(int)(CurrentY / _pageSize.Height);
                var pageStart = (page + 1) * _pageSize.Height;
                CurrentY = pageStart + margin;
            }
            if (CurrentY > Bottom) {
                Bottom = CurrentY;
            }
            RowHeight = 0;
        }
    }
}
