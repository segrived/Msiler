using Msiler.Helpers;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Msiler.UI
{
    public partial class ILHelpWindow
    {
        public ILHelpWindow() {
            this.InitializeComponent();
            this.InstructionList.ItemsSource = AssemblyParser.Helpers.GetOpCodesList();
            var view = CollectionViewSource.GetDefaultView(this.InstructionList.ItemsSource);
            view.Filter = this.FilterInstructionsList;
        }

        void InstructionList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var item = ((ListBox)sender).SelectedItem;
            if (item == null) {
                return;
            }
            string selectedInstruction = (string)item;
            var instructionInfo = AssemblyParser.Helpers.GetInstructionInformation(selectedInstruction);
            this.SelectedInstuctionInfo.Text = instructionInfo.Description;
        }

        bool FilterInstructionsList(object o) {
            string methodName = o as string;
            string filterQuery = this.FilterMethodsTextBox.Text;
            return String.IsNullOrEmpty(filterQuery) 
                || methodName.Contains(filterQuery, StringComparison.OrdinalIgnoreCase);
        }

        private void FilterMethodsTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            CollectionViewSource.GetDefaultView(this.InstructionList.ItemsSource).Refresh();
        }
    }
}
