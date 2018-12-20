using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

/// <summary>
/// Based on tots invoicePaginator.cs
/// </summary>

namespace JSReport {
    public class ReportPaginator : DocumentPaginator {
        List<PageView> _pages;
        Table _table;
        double _pageMargin;
        Document _doc;

        const int PAGECOUNT = 2;    // Initial estimate of the page count

        public ReportPaginator(Document doc,
                               double pageMargin,
                               Size pageSize) {
            _doc = doc;
            _pageMargin = pageMargin;
            _pageSize = new Size(pageSize.Width - (pageMargin * 2), pageSize.Height - (pageMargin * 2));
            var top = pageMargin;
            _table = new Table(top, _pageSize, _pageMargin);
            _pages = new List<PageView>(PAGECOUNT);
            CurrentPageNo = 0;
            PaginateDocument();
        }

        Size _pageSize;
        public override Size PageSize {
            get => _pageSize;
            set {
                _pageSize = new Size(value.Width - _pageMargin * 2, value.Height - _pageMargin * 2);
                _pages = new List<PageView>(PAGECOUNT);
                PaginateDocument();
            }
        }

        public int CurrentPageNo { get; private set; }

        public override int PageCount {
            get => _pages.Count;
        }

        public override IDocumentPaginatorSource Source {
            get { return null; }
        }

        bool _isPageCountValid = false;
        public override bool IsPageCountValid {
            get => _isPageCountValid;
        }

        /// <summary>
        /// Return a formatted document page containing the requested invoice 
        /// page number
        /// </summary>
        /// <param name="pageNumber">The current page number to print</param>
        /// <returns>DocumentPage object rendered ready to print</returns>
        public override DocumentPage GetPage(int pageNumber) {
            if (_pages.Count < 1) {
                return DocumentPage.Missing;
            }
            if (pageNumber >= _pages.Count) {
                CurrentPageNo = _pages.Count - 1;
            } else if (pageNumber < 0) {
                CurrentPageNo = 0;
            } else {
                CurrentPageNo = pageNumber;
            }
            return _pages[CurrentPageNo];
        }

        PageView GetNewPage() {
            var page = new PageView(new DrawingVisual(), new Point(_pageMargin, _pageMargin), _pageSize);
            _pages.Add(page);
            CurrentPageNo = _pages.Count - 1;
            return page;
        }

        void SetPageToDraw(int pageNo) {
            while (pageNo > _pages.Count - 1) {
                GetNewPage();
            }
            CurrentPageNo = pageNo;
        }

        public PageView CurrentPage => _pages[CurrentPageNo];

        public bool IsFirstPage { get => CurrentPageNo == 0; }
        public bool IsLastPage { get => CurrentPageNo == _pages.Count - 1; }

        public void NextPage() {
            GetPage(CurrentPageNo + 1);
        }
        public void PrevPage() {
            GetPage(CurrentPageNo - 1);
        }

        bool LayoutCommandHandled(string[] cmds) {
            switch (cmds[0]) {
                case "columns":
                    if (cmds.Length == 2) {
                        int columns;
                        if (int.TryParse(cmds[1], out columns)) {
                            _table.ResetColumns(columns, _pageMargin);
                            _table.RowHeight = _doc.Styles.CurrentStyle.Height;
                        }
                    }
                    break;
                case "column":
                    if (cmds.Length > 1) {
                        int column;
                        int span = 1;
                        if (int.TryParse(cmds[1], out column)) {
                            if (cmds.Length == 3) {
                                int.TryParse(cmds[2], out span);
                            }
                            _table.SetColumnVertical(column);
                            _table.Span = span;
                        }
                    }
                    break;
                default:
                    return false;
            }
            return true;
        }

        bool CommandHandled(string line) {
            // Is it a dot command?
            if (line.Length > 0 && line[0] == '.') {
                var clean = line.Trim().ToLower().Substring(1);
                var cmds = clean.Split(' ');
                if (cmds.Length > 0) {
#if DEBUG
                    if (cmds[0] == "debug") {
                        System.Diagnostics.Debugger.Break();
                        return true;
                    }
#endif
                    if (cmds[0] == "hr") {
                        //SetPageToDraw(_table.Bottom);
                        //CurrentPage.HorizontalRule(_doc.CurrentStyle, _table.Bottom); //_maxY);
                        return true;
                    }
                    if (LayoutCommandHandled(cmds)) {
                        return true;
                    }
                    System.Diagnostics.Debug.Print("Ignoring unknown .command \"{0}\"", clean);
                    return true;
                }
            }
            return false;
        }

        void PaginateDocument() {
            //System.Diagnostics.Debug.Print("PaginateDocument");
            try {
                foreach (var line in _doc.Contents) {
                    if (line.StartsWith(".quit")) {
                        break;
                    }
                    if (!CommandHandled(line)) {
                        // Draw the cell contents, a line may contain multiple cells delimited by ';'
                        foreach (var cell in _table.PrepareCells(line, _doc.CurrentStyle, _pageMargin)) {
                            SetPageToDraw(cell.PageNumber);
                            cell.Draw(CurrentPage.DrawCtxt);
                        }
                    }
                }
            } finally {
                foreach (var p in _pages) {
                    p.Close();
                }
            }
            _isPageCountValid = true;
            GetPage(0);
        }
    }
}
