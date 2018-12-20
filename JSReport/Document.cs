using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace JSReport {
    public class Document {
        public Document(Styles styles) {
            Styles = styles;
            Name = " << No Name >> ";
        }

        List<string> _srcDoc = new List<string>();
        public Styles Styles { get; set; }

        public CellStyle CurrentStyle {
            get {
                return Styles.CurrentStyle;
            }
        }

        public bool IsEmpty => _srcDoc.Count < 1;
        public string Name { get; set; }

        /// <summary>
        /// Pass the contents of the doc to the caller
        /// applying any style commands as they are encountered
        /// </summary>
        public IEnumerable<string> Contents {
            get {
                foreach (var line in _srcDoc) {
                    if (line.StartsWith(".")) {
                        var cmds = GetCommands(line);

                        if (cmds.Length > 0) {
                            if (cmds[0] == "style") {
                                if (cmds.Length == 2) {
                                    Styles.SetStyle(cmds[1].Trim());
                                    continue;
                                }
                            }
                            else {
                                if (StyleCommandHandled(cmds)) {
                                    continue;
                                }
                            }
                        }
                    }
                    yield return line;
                }
            }
        }

        string[] GetCommands(string line) {
            var clean = line.Trim().ToLower().Substring(1);
            return clean.Split(' ');
        }

        bool _isDocMode = true;
        public void Add(string line) {
            //System.Diagnostics.Debug.Print(line);
            if (line.StartsWith(".styles")) {
                _isDocMode = false;
                return;
            } else if (line.StartsWith(".name ")) {
                Name = line.Substring(6);
                return;
            }

            if (_isDocMode) {
                _srcDoc.Add(line);
            }
            else {
                _isDocMode = ParseStyles(line);
            }
        }

        /// <summary>
        /// Parse the line assuming it to be a style defintion
        /// </summary>
        /// <param name="line">Line containing style definition</param>
        /// <returns>true if parsed or false if blank line signifying the end of style definitions</returns>
        public bool ParseStyles(string line) {
            if (line.Length > 0) {  // Blank line signifies end of style definitions
                var cmds = GetCommands(line);

                if (cmds.Length > 0) {
                    switch (cmds[0]) {
                        case "style":
                            if (cmds.Length == 2) {
                                var styleName = cmds[1].Trim();
                                Styles.SetStyle(styleName);
                            }
                            break;
                        default:
                            StyleCommandHandled(cmds);
                            break;
                    }
                    return false;
                }
            }
            return true;
        }

        public bool StyleCommandHandled(string[] cmds) {
            bool handled = true;
            var style = Styles.CurrentStyle;

            switch (cmds[0]) {
                case "right":
                    style.Alignment = TextAlignment.Right;
                    break;
                case "left":
                    style.Alignment = TextAlignment.Left;
                    break;
                case "centre":
                case "center":  // Allow American and European spelling
                    style.Alignment = TextAlignment.Center;
                    break;
                case "brush":
                    style.HighlightBrush = Brushes.LightPink;
                    break;
                case "borders":
                    if (cmds.Length == 2) {
                        switch (cmds[1]) {
                            case "left":
                                style.Borders = CellStyle.BordersStyles.Left;
                                break;
                            case "top":
                                style.Borders = CellStyle.BordersStyles.Top;
                                break;
                            case "right":
                                style.Borders = CellStyle.BordersStyles.Right;
                                break;
                            case "bottom":
                                style.Borders = CellStyle.BordersStyles.Bottom;
                                break;
                            case "all":
                                style.Borders = CellStyle.BordersStyles.All;
                                break;
                            case "horizontal":
                                style.Borders = CellStyle.BordersStyles.Horizontal;
                                break;
                            case "vertical":
                                style.Borders = CellStyle.BordersStyles.Vertical;
                                break;
                            case "none":
                                style.Borders = CellStyle.BordersStyles.None;
                                break;
                        }
                    }
                    break;
                case "underline":
                    style.IsUnderlined = true;
                    break;
                case "shaded":
                    style.IsShaded = true;
                    break;
                case "shadedoval":
                    style.Borders = CellStyle.BordersStyles.All;
                    style.BorderAllIsRounded = true;
                    break;
                case "clear":
                    style.IsShaded = false;
                    break;
                case "fontsize":
                    double fontsize;
                    if (double.TryParse(cmds[1], out fontsize)) {
                        style.FontScale = 1.0;
                        style.FontSize = fontsize;
                        style.IsScaledFontSize = false;
                    }
                    break;
                case "fontscale":
                    double fontscale;
                    if (double.TryParse(cmds[1], out fontscale)) {
                        style.FontSize = Styles.BaseFontSize * fontscale;
                        style.FontScale = fontscale;
                        style.IsScaledFontSize = true;
                    }
                    break;
                case "bold":
                    style.IsBoldface = true;
                    break;
                case "normal":
                    style.IsBoldface = false;
                    break;
                default:
                    handled = false;
                    break;
            }
            return handled;
        }

        bool GetBooleanOption(string expected) {
            if (expected.Trim().ToLower() == "on") {
                return true;
            }
            return false;
        }
    }
}
