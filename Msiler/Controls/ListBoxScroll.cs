using System.Windows.Controls;

namespace Msiler.Controls
{
    class ListBoxScroll : ListBox
    {
        public ListBoxScroll() {
            this.SelectionChanged += this.ListBoxScroll_SelectionChanged;
        }

        void ListBoxScroll_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            this.ScrollIntoView(this.SelectedItem);
        }
    }
}
