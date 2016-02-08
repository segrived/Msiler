using Msiler.Helpers;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Msiler.UI
{
    public partial class ILHelpWindow : Window
    {
        public ILHelpWindow() {
            InitializeComponent();
            this.InstructionList.ItemsSource = AssemblyParser.Helpers.GetOpCodesList();
            var view = CollectionViewSource.GetDefaultView(InstructionList.ItemsSource);
            view.Filter = FilterInstructionsList;
        }

        void InstructionList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            string selectedInstruction = (string)((ListBox)sender).SelectedItem;
            var instructionInfo = AssemblyParser.Helpers.GetInstructionInformation(selectedInstruction);
            this.SelectedInstuctionInfo.Text = instructionInfo.Description;
        }

        bool FilterInstructionsList(object o) {
            var methodName = o as string;
            var filterQuery = FilterMethodsTextBox.Text;
            if (String.IsNullOrEmpty(filterQuery))
                return true;

            return methodName.Contains(filterQuery, StringComparison.OrdinalIgnoreCase);
        }

        private void FilterMethodsTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            CollectionViewSource.GetDefaultView(InstructionList.ItemsSource).Refresh();
        }
    }
}
