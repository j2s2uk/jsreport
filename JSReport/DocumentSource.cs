// DocumentSource.cs - (C) Jeff Deacon

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Media;

namespace JSReport {
    public class DocumentSource {
        public DocumentSource() {
        }

        public static DocumentSource GetDocumentSource(string source, Typeface defaultTypeface) {
            DocumentSource docSource = new DocumentSource();
            Uri uri;

            if (Uri.TryCreate(source, UriKind.Absolute, out uri)) {
                if (uri.IsFile) {
                    if (File.Exists(source)) {
                        docSource._baseFolder = Path.GetDirectoryName(source);
                        docSource.AddDocument(Path.GetFileName(source));
                    } else if (Directory.Exists(source)) {
                        docSource._baseFolder = source;
                        if (File.Exists(Path.Combine(source, "doclist.jsr"))) {
                            docSource.AddDocument("docList.jsr");
                        } else {
                            foreach (var item in Directory.EnumerateFiles(source, "*.jsr")) {
                                docSource.AddDocument(Path.GetFileName(item));
                            }
                        }
                    }
                }
                else {
                    docSource.AddDocument(source);
                }
                docSource._styles = new Styles(defaultTypeface);
                docSource._styles.BaseFontSize = 14;
            }

            return docSource;
        }

        List<string> _docList = new List<string>();
        Styles _styles;
        public Styles Styles {
            get {
                return _styles;
            }
        }

        string _baseFolder = "";

        public IEnumerable<string> DocumentList {
            get {
                return _docList;
            }
        }

        public void AddDocument(string docName) {
            _docList.Add(docName);
        }

        public int Count => _docList.Count;

        Document _currentDoc;
        public async Task<Document> GetCurrentDocument() {
            if (_currentDoc == null) {
                if (_docList.Count > 0) {
                    await SetCurrentDocument(_docList[0]);
                } else {
                    _currentDoc = new Document(_styles);
                    _currentDoc.Add("No documents have been specified to open");
                }
            }
            return _currentDoc;
        }

        public async Task SetCurrentDocument(string source) {
            _currentDoc = new Document(_styles);
            try {
                Uri uriSource;
                if (Uri.IsWellFormedUriString(source, UriKind.Absolute)) {
                    uriSource = new Uri(source);
                    var stream = await new WebClient().OpenReadTaskAsync(uriSource);
                    using (var reader = new StreamReader(stream)) {
                        await GetDocument(reader);
                    }
                } else {
                    var localPath = Path.Combine(_baseFolder, source);
                    if (File.Exists(localPath)) {
                        using (StreamReader reader = File.OpenText(localPath)) {
                            await GetDocument(reader);
                        }
                    } else {
                        _currentDoc.Add("The location of the document specified is invalid");
                        _currentDoc.Add(source);
                    }
                }
            }
            catch (Exception ex) {
                _currentDoc.Add(ex.Message);
            }
            return;
        }

        async Task GetDocument(StreamReader reader) {
            string line;
            try {
                // Read first line must be DOCTYPE
                line = (await reader.ReadLineAsync()).ToLower();
                if (line.StartsWith("doctype")) {
                    if (line.Substring(8).StartsWith("doclist")) {
                        _docList.Clear();
                        while (null != (line = await reader.ReadLineAsync())) {
                            _docList.Add(line);
                        }
                        if (_docList.Count > 0) {
                            await SetCurrentDocument(_docList[0]);
                        }
                    }
                    else {
                        while (null != (line = await reader.ReadLineAsync())) {
                            _currentDoc.Add(line);
                        }
                    }
                } else {
                    _currentDoc.Add("Error: Badly formatted document - no document type specified");
                }
            }
            catch (Exception ex) {
                _currentDoc.Add("Error: Cannot read the document\n");
                _currentDoc.Add(ex.Message);
            }
        }
    }
}
