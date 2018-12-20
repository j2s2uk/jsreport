using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;

namespace JSReport {
    public class PrintPreviewCanvas : Canvas {
        private List<Visual> visuals = new List<Visual>();

        protected override int VisualChildrenCount {
            get {
                return visuals.Count;
            }
        }

        protected override Visual GetVisualChild(int index) {
            return visuals[index];
        }

        public void AddVisual(Visual visual) {
            visuals.Add(visual);

            base.AddVisualChild(visual);
            base.AddLogicalChild(visual);
        }

        public void DeleteVisual(Visual visual) {
            visuals.Remove(visual);

            base.RemoveVisualChild(visual);
            base.RemoveLogicalChild(visual);
        }

        public void ClearVisual() {
            foreach (Visual visual in visuals) {
                base.RemoveVisualChild(visual);
                base.RemoveLogicalChild(visual);
            }
            visuals.Clear();
        }
    }
}
