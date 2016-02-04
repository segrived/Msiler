using System.Windows.Controls;
using ICSharpCode.AvalonEdit;
using Msiler.DialogPages;
using Msiler.Lib;
using System.Windows.Media;
using Microsoft.VisualStudio.PlatformUI;
using System.Collections.Generic;
using Msiler.AssemblyParser;
using Msiler.Helpers;
using System.Linq;
using System;

namespace Msiler.UI
{
    public partial class MyControl : UserControl
    {
        const string RepoUrl = @"https://github.com/segrived/Msiler";

        AssemblyManager _assemblyManager;
        IEnumerable<AssemblyMethod> _methodList;
        AssemblyMethod _selectedMethod;
        ListingGeneratorOptions _generatorOptions;

        ToolTip toolTip = new ToolTip();

        public MyControl() {
            InitializeComponent();

            this._assemblyManager = new AssemblyManager();

            _assemblyManager.MethodListChanged += (s, e) => {
                this._methodList = e.Methods;
                this.MethodsList.ItemsSource = this._methodList;
                ApplySearchFilter();
            };

            this.BytecodeListing.Options = new TextEditorOptions {
                EnableEmailHyperlinks = false,
                EnableHyperlinks = false
            };

            Common.Instance.DisplayOptions.Applied += (s, e) => UpdateDisplayOptions();
            VSColorTheme.ThemeChanged += (e) => UpdateDisplayOptions();
            UpdateDisplayOptions();

            Common.Instance.ListingGenerationOptions.Applied += (s, e) => {
                this._generatorOptions = Common.Instance.ListingGenerationOptions.ToListingGeneratorOptions();
                UpdateBytecodeListing();
            };
            this._generatorOptions = Common.Instance.ListingGenerationOptions.ToListingGeneratorOptions();

            FunctionFollower.MethodSelected += FunctionFollower_MethodSelected;


        }

        void FunctionFollower_MethodSelected(object sender, MethodSignatureEventArgs e) {
            if (!Common.Instance.GeneralOptions.FollowSelectedFunctionInEditor) {
                return;
            }
            if (this._methodList == null) {
                return;
            }
            var editorMethod = this._methodList
                .FirstOrDefault(m => m.Signature.Equals(e.MethodSignature));
            if (editorMethod != null) {
                this._selectedMethod = editorMethod;
                this.MethodsList.SelectedItem = this._selectedMethod;
            }

        }

        void UpdateDisplayOptions() {
            var displayOptions = Common.Instance.DisplayOptions;
            string fontFamily = "Consolas";
            if (FontHelpers.IsFontFamilyExist(displayOptions.FontName)) {
                fontFamily = displayOptions.FontName;
            }
            BytecodeListing.FontFamily = new FontFamily(fontFamily);
            BytecodeListing.FontSize = displayOptions.FontSize;
            BytecodeListing.ShowLineNumbers = displayOptions.LineNumbers;
            BytecodeListing.SyntaxHighlighting = ColorTheme.GetColorTheme(displayOptions.ColorTheme);
        }

        void UpdateBytecodeListing() {
            if (this._selectedMethod != null) {
                this.BytecodeListing.Text = this._selectedMethod.GenerateListing(this._generatorOptions);
            }
        }

        void ApplySearchFilter() {
            var filterQuery = this.FilterMethodsTextBox.Text;
            this.MethodsList.ItemsSource = this._methodList
                .Where(m => m.Signature.MethodName.Contains(filterQuery, StringComparison.OrdinalIgnoreCase));
        }

        void BytecodeListing_MouseHover(object sender, System.Windows.Input.MouseEventArgs e) {
            var pos = BytecodeListing.GetPositionFromPoint(e.GetPosition(BytecodeListing));

            if (pos == null)
                return;

            int off = BytecodeListing.Document.GetOffset(pos.Value.Line, pos.Value.Column);
            var wordUnderCursor = AvalonEditHelpers.GetWordOnOffset(BytecodeListing.Document, off);

            var info = AssemblyParser.Helpers.GetInstructionInformation(wordUnderCursor);
            if (info == null) {
                return;
            }

            toolTip.PlacementTarget = this;
            toolTip.Content = new TextEditor {
                Text = $"{info.Name}: {info.Description}",
                Opacity = 0.6,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden,
                VerticalScrollBarVisibility = ScrollBarVisibility.Hidden
            };
            toolTip.IsOpen = true;
            e.Handled = true;
        }

        void BytecodeListing_MouseHoverStopped(object sender, System.Windows.Input.MouseEventArgs e) {
            toolTip.IsOpen = false;
        }

        void FilterMethodsTextBox_TextChanged(object sender, TextChangedEventArgs e) =>
            ApplySearchFilter();

        private void MethodsList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            this._selectedMethod = (AssemblyMethod)MethodsList.SelectedItem;
            UpdateBytecodeListing();
        }

        #region Hyperlink handler
        void HyperlinkOptions_Click(object sender, System.Windows.RoutedEventArgs e) =>
            Common.Instance.Package.ShowOptionPage(typeof(ExtensionGeneralOptions));

        void HyperlinkGithub_Click(object sender, System.Windows.RoutedEventArgs e) =>
            System.Diagnostics.Process.Start(RepoUrl);

        void HyperlinkAbout_Click(object sender, System.Windows.RoutedEventArgs e) =>
            new AboutWindow().ShowDialog();
        #endregion Hyperlink handlers

    }
}