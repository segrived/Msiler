using System.Windows.Controls;

namespace Quart.Msiler.Controls
{
    class ListBoxScroll : ListBox
    {
        public ListBoxScroll() {
            SelectionChanged += ListBoxScroll_SelectionChanged;
        }

        void ListBoxScroll_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            ScrollIntoView(SelectedItem);
        }
    }
}
