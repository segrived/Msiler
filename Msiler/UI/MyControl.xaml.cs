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
using System.Windows.Data;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Msiler.UI
{
    public partial class MyControl : UserControl
    {
        const string RepoUrl = @"https://github.com/segrived/Msiler";

        AssemblyManager _assemblyManager = new AssemblyManager();

        AssemblyMethod _currentMethod;
        AssemblyMethod CurrentMethod {
            get { return this._currentMethod; }
            set
            {
                this._currentMethod = value;
                this.MethodsList.SelectedItem = value;
            }
        }

        List<AssemblyMethod> _assemblyMethods;

        public MyControl() {
            InitializeComponent();

            InitConfiguration();
            InitEventHandlers();
        }

        void OnMethodListChanged(object sender, MethodsListEventArgs e) {
            this._assemblyMethods = e.Methods;
            this.MethodsList.ItemsSource = new ObservableCollection<AssemblyMethod>(this._assemblyMethods);

            if (this.CurrentMethod != null) {
                this.CurrentMethod = e.Methods.FirstOrDefault(m => m.Equals(this.CurrentMethod));
                this.DisassembleCurrentMethod();
            }
            var view = CollectionViewSource.GetDefaultView(this.MethodsList.ItemsSource);
            view.Filter = FilterMethodsList;
        }

        public void InitConfiguration() {
            this.BytecodeListing.Options = new TextEditorOptions {
                EnableEmailHyperlinks = false,
                EnableHyperlinks = false
            };
            UpdateDisplayOptions();
        }

        public void InitEventHandlers() {
            Common.Instance.DisplayOptions.Applied += (s, e)
                => UpdateDisplayOptions();
            Common.Instance.ListingGenerationOptions.Applied += (s, e)
                => DisassembleCurrentMethod();
            Common.Instance.ExcludeOptions.Applied += (s, e) => {
                if (MethodsList.ItemsSource != null) {
                    CollectionViewSource.GetDefaultView(MethodsList.ItemsSource).Refresh();
                }
            };
            VSColorTheme.ThemeChanged += (e)
                => UpdateDisplayOptions();
            FunctionFollower.MethodSelected += OnMethodSelected;
            _assemblyManager.MethodListChanged += OnMethodListChanged;
        }

        void OnMethodSelected(object sender, MethodSignatureEventArgs e) {
            if (!Common.Instance.GeneralOptions.FollowSelectedFunctionInEditor) {
                return;
            }
            if (this._assemblyMethods == null || this._assemblyMethods.Count == 0) {
                return;
            }
            var selMethod = this._assemblyMethods.FirstOrDefault(m => m.Signature.Equals(e.MethodSignature));
            if (selMethod != null) {
                this.CurrentMethod = selMethod;
                DisassembleCurrentMethod();
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

        bool FilterMethodsList(object o) {
            var method = (AssemblyMethod)o;

            // filter method types
            var excludeOptions = Common.Instance.ExcludeOptions;
            if (excludeOptions.ExcludeConstructors && method.IsConstructor)
                return false;
            if (excludeOptions.ExcludeProperties && method.IsProperty)
                return false;
            if (excludeOptions.ExcludeAnonymousMethods && method.IsAnonymous)
                return false;

            // filter methods by search
            var filterQuery = FilterMethodsTextBox.Text;
            if (String.IsNullOrEmpty(filterQuery))
                return true;

            return method.Signature.MethodName.Contains(filterQuery, StringComparison.OrdinalIgnoreCase);
        }

        public ListingGeneratorOptions GetGeneratorOptions()
            => Common.Instance.ListingGenerationOptions.ToListingGeneratorOptions();

        private void DisassembleCurrentMethod() {
            if (this.CurrentMethod != null) {
                var listing = this.CurrentMethod.GenerateListing(this.GetGeneratorOptions());
                this.BytecodeListing.Text = listing;
            }
        }

        #region Instruction Hint Tooltip
        ToolTip toolTip = new ToolTip();

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
        #endregion

        #region UI handler
        void MethodsList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            this.CurrentMethod = (AssemblyMethod)this.MethodsList.SelectedItem;
            this.DisassembleCurrentMethod();
        }

        void FilterMethodsTextBox_TextChanged(object sender, TextChangedEventArgs e) =>
            CollectionViewSource.GetDefaultView(MethodsList.ItemsSource).Refresh();

        void HyperlinkOptions_Click(object sender, System.Windows.RoutedEventArgs e) =>
            Common.Instance.Package.ShowOptionPage(typeof(ExtensionGeneralOptions));

        void HyperlinkGithub_Click(object sender, System.Windows.RoutedEventArgs e) =>
            Process.Start(RepoUrl);

        void HyperlinkAbout_Click(object sender, System.Windows.RoutedEventArgs e) =>
            new AboutWindow().ShowDialog();
        #endregion Hyperlink handlers
    }
}