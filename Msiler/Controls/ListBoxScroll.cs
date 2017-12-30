using System.Windows.Controls;

namespace Msiler.Controls
{
    internal class ListBoxScroll : ListBox
    {
        public ListBoxScroll()
        {
            this.SelectionChanged += this.ListBoxScroll_SelectionChanged;
        }

        private void ListBoxScroll_SelectionChanged(object sender, SelectionChangedEventArgs e) 
            => this.ScrollIntoView(this.SelectedItem);
    }
}
