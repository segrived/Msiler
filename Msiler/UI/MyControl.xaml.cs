using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Windows.Data;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using System.Text;
using System.Windows;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.PlatformUI;
using Msiler.AssemblyParser;
using Msiler.Helpers;
using Msiler.DialogPages;
using Msiler.Lib;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit;

namespace Msiler.UI
{
    public partial class MyControl : UserControl
    {
        const int MaxCodeLinesInHint = 5;

        static readonly Regex offsetRegex =
            new Regex(@"^(IL_[\dA-F]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        Dictionary<string, int> offsetLinesCache
            = new Dictionary<string, int>();

        IHighlightingDefinition currentHighlightDefinition;

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
        Dictionary<AssemblyMethod, string> _listingCache =
            new Dictionary<AssemblyMethod, string>();

        public MyControl() {
            InitializeComponent();

            InitConfiguration();
            InitEventHandlers();
        }

        void OnMethodListChanged(object sender, MethodsListEventArgs e) {
            this._assemblyMethods = e.Methods;
            this.MethodsList.ItemsSource = new ObservableCollection<AssemblyMethod>(this._assemblyMethods);
            this._listingCache.Clear();

            if (this.CurrentMethod != null) {
                this.ProcessMethod(e.Methods.FirstOrDefault(m => m.Equals(this.CurrentMethod)));
            }
            var view = CollectionViewSource.GetDefaultView(this.MethodsList.ItemsSource);
            view.Filter = FilterMethodsList;

            bool isAnyMethods = this._assemblyMethods.Count > 0;
            this.MainView.Visibility = ToVisibilityState(isAnyMethods);
            this.WelcomeUserControl.Visibility = ToVisibilityState(!isAnyMethods);
        }

        public void InitConfiguration() {
            this.BytecodeListing.Options = new TextEditorOptions {
                EnableEmailHyperlinks = false,
                EnableHyperlinks = false
            };
            UpdateDisplayOptions();
        }

        void UpdateDisplayOptions() {
            var displayOptions = Common.Instance.DisplayOptions;

            var scheme = ColorTheme.GetHighlightingScheme(displayOptions.ColorTheme);
            this.currentHighlightDefinition = ColorTheme.GetDefinition(scheme);

            string fontFamily = "Consolas";
            if (FontHelpers.IsFontFamilyExist(displayOptions.FontName)) {
                fontFamily = displayOptions.FontName;
            }
            BytecodeListing.FontFamily = new FontFamily(fontFamily);
            BytecodeListing.FontSize = displayOptions.FontSize;
            BytecodeListing.ShowLineNumbers = displayOptions.LineNumbers;
            BytecodeListing.SyntaxHighlighting = this.currentHighlightDefinition;
        }

        public void InitEventHandlers() {
            Common.Instance.DisplayOptions.Applied += (s, e)
                => UpdateDisplayOptions();
            Common.Instance.ListingGenerationOptions.Applied += (s, e) => {
                this._listingCache.Clear();
                this.ProcessMethod(this.CurrentMethod);
            };
            Common.Instance.ExcludeOptions.Applied += (s, e) => {
                if (MethodsList.ItemsSource != null) {
                    CollectionViewSource.GetDefaultView(MethodsList.ItemsSource).Refresh();
                }
            };
            VSColorTheme.ThemeChanged += (e) => UpdateDisplayOptions();

            FunctionFollower.MethodSelected += OnMethodSelected;
            _assemblyManager.MethodListChanged += OnMethodListChanged;
        }

        void OnMethodSelected(object sender, MethodSignatureEventArgs e) {
            if (this._assemblyMethods == null || this._assemblyMethods.Count == 0) {
                return;
            }
            // do not process if same method
            if (this.CurrentMethod != null && this.CurrentMethod.Signature.Equals(e.MethodSignature)) {
                return;
            }
            // find metyhod with same signature
            var method = this._assemblyMethods.FirstOrDefault(m => m.Signature.Equals(e.MethodSignature));
            ProcessMethod(method, false);
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

        private void ProcessMethod(AssemblyMethod method, bool clearIfNull = true) {
            if (method != null) {
                try {
                    this.CurrentMethod = method;
                    var listingText = (this._listingCache.ContainsKey(method))
                        ? _listingCache[method]
                        : this.CurrentMethod.GenerateListing(this.GetGeneratorOptions());
                    this._listingCache[method] = listingText;
                    this.BytecodeListing.Text = listingText;
                    this.BytecodeListing.ScrollToHome();
                    this.BytecodeListing.CaretOffset = 0;

                    this.offsetLinesCache.Clear();
                    var lines = listingText.Lines();
                    for (int i = 0; i < lines.Length; i++) {
                        var match = offsetRegex.Match(lines[i]);
                        if (match.Success) {
                            this.offsetLinesCache.Add(match.Value, i + 1);
                        }
                    }
                } catch (Exception ex) {
                    var errorBuilder = new StringBuilder();
                    errorBuilder.AppendLine($"ERROR: {ex.Message}");
                    errorBuilder.AppendLine($"Source: {ex.Source}");
                    errorBuilder.Append($"Stacktrace: {ex.StackTrace}");
                    this.BytecodeListing.Text = errorBuilder.ToString();
                }
            } else {
                if (clearIfNull) {
                    this.CurrentMethod = null;
                    this.BytecodeListing.Text = String.Empty;
                    this.offsetLinesCache.Clear();
                }
            }
        }

        #region Instruction Hint Tooltip
        readonly ToolTip toolTip = new ToolTip();

        string GetWordUnderCursor(Point p) {
            return AvalonEditHelpers.GetWordOnOffset(BytecodeListing, p);
        }

        void BytecodeListing_MouseHover(object sender, MouseEventArgs e) {
            var wordUnderCursor = this.GetWordUnderCursor(e.GetPosition(BytecodeListing));

            var offsetMatch = offsetRegex.Match(wordUnderCursor);
            if (offsetMatch.Success) {
                var offsetStr = offsetMatch.Value;
                if (!this.offsetLinesCache.ContainsKey(offsetStr)) {
                    e.Handled = true;
                    return;
                }
                var lineNumber = this.offsetLinesCache[offsetStr];
                var lineCount = BytecodeListing.LineCount;
                var sb = new StringBuilder();

                var docLine = BytecodeListing.Document.GetLineByNumber(lineNumber);
                for (int i = 0; i < MaxCodeLinesInHint; i++) {
                    if (docLine.LineNumber >= lineCount) {
                        break;
                    }
                    var lineContent = BytecodeListing.Document.GetText(docLine.Offset, docLine.Length);
                    sb.AppendLine(lineContent);
                    docLine = docLine.NextLine;
                }
                ShowToolTip(sb.ToString().TrimEnd('\r', '\n'), this.currentHighlightDefinition);
            }

            long? numberUnderCursor = StringHelpers.ParseNumber(wordUnderCursor);
            if (numberUnderCursor != null) {
                var v = numberUnderCursor.Value;
                var sb = new StringBuilder();
                string hint = String.Empty;
                hint += $"Decimal: {v}{Environment.NewLine}";
                hint += $"HEX:     0x{Convert.ToString(v, 16).ToUpper()}{Environment.NewLine}";
                hint += $"Binary:  0b{Convert.ToString(v, 2)}{Environment.NewLine}";
                hint += $"Octal:   0{Convert.ToString(v, 8)}";
                ShowToolTip(hint, this.currentHighlightDefinition);
            }

            var info = AssemblyParser.Helpers.GetInstructionInformation(wordUnderCursor);
            if (info != null) {
                ShowToolTip($"{info.Name}: {info.Description}");
            }
            e.Handled = true;
        }

        void BytecodeListing_MouseHoverStopped(object sender, MouseEventArgs e) {
            toolTip.IsOpen = false;
        }

        private void BytecodeListing_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e) {
            var wordUnderCursor = this.GetWordUnderCursor(e.GetPosition(BytecodeListing));

            // detect offset under cursor
            var match = offsetRegex.Match(wordUnderCursor);
            if (match.Success) {
                var line = this.offsetLinesCache[match.Value];
                var offset = BytecodeListing.Document.GetOffset(line, 0);
                BytecodeListing.CaretOffset = offset;
                BytecodeListing.ScrollToLine(line);
                e.Handled = true;
                return;
            }

            // detect method name under cursor
            var fMethod = this._assemblyMethods
                .FirstOrDefault(m => m.Signature.MethodName == wordUnderCursor);
            if (fMethod != null) {
                this.CurrentMethod = fMethod;
                e.Handled = true;
                return;
            }
        }


        public void ShowToolTip(string content, IHighlightingDefinition highlight = null) {
            var displayOptions = Common.Instance.DisplayOptions;
            toolTip.PlacementTarget = this;
            int transpLevel = (displayOptions.TooltipTransparency < 0 || displayOptions.TooltipTransparency > 100)
                ? 0
                : displayOptions.TooltipTransparency;
            toolTip.Opacity = 1.0 - (transpLevel / 100.0);

            var bgDColor = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowBackgroundBrushKey);
            var bgMColor = Color.FromRgb(bgDColor.R, bgDColor.G, bgDColor.B);
            toolTip.Background = new SolidColorBrush(bgMColor);

            var fgDColor = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowTextBrushKey);
            var fgMColor = Color.FromRgb(fgDColor.R, fgDColor.G, fgDColor.B);

            toolTip.Content = new TextEditor {
                Text = content,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden,
                VerticalScrollBarVisibility = ScrollBarVisibility.Hidden,
                SyntaxHighlighting = highlight,
                Background = Brushes.Transparent,
                FontFamily = new FontFamily(displayOptions.FontName),
                FontSize = displayOptions.FontSize,
                Foreground = new SolidColorBrush(fgMColor)
            };
            toolTip.IsOpen = true;
        }

        #endregion

        #region UI handlers
        void MethodsList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            this.ProcessMethod((AssemblyMethod)this.MethodsList.SelectedItem);
        }

        void FilterMethodsTextBox_TextChanged(object sender, TextChangedEventArgs e) =>
            CollectionViewSource.GetDefaultView(MethodsList.ItemsSource).Refresh();

        void HyperlinkOptions_Click(object sender, RoutedEventArgs e) =>
            Common.Instance.Package.ShowOptionPage(typeof(ExtensionGeneralOptions));

        void HyperlinkGithub_Click(object sender, RoutedEventArgs e) =>
            Process.Start(Common.RepoUrl);

        void HyperlinkAbout_Click(object sender, RoutedEventArgs e) =>
            new AboutWindow().ShowDialog();

        void IsFollowModeEnabled_CheckedChange(object sender, RoutedEventArgs e) {
            FunctionFollower.IsFollowingEnabled = ((CheckBox)sender).IsChecked.Value;
        }

        private void MenuItemGeneralOptions_Click(object sender, RoutedEventArgs e) {
            Common.Instance.Package.ShowOptionPage(typeof(ExtensionGeneralOptions));
        }

        private void MenuItemListingGenearationOptions_Click(object sender, RoutedEventArgs e) {
            Common.Instance.Package.ShowOptionPage(typeof(ExtensionListingGenerationOptions));
        }

        private void MenuItemMethodFilteringOptions_Click(object sender, RoutedEventArgs e) {
            Common.Instance.Package.ShowOptionPage(typeof(ExtensionExcludeOptions));
        }

        private void MenuItemDisplayOptions_Click(object sender, RoutedEventArgs e) {
            Common.Instance.Package.ShowOptionPage(typeof(ExtensionDisplayOptions));
        }

        private void OptionsLink_MouseDown(object sender, MouseButtonEventArgs e) {
            if (e.ChangedButton == MouseButton.Left) {
                var block = sender as TextBlock;
                ContextMenu contextMenu = block.ContextMenu;
                contextMenu.PlacementTarget = block;
                contextMenu.IsOpen = true;
                e.Handled = true;
            }
        }
        #endregion UI handlers

        #region Helper methods
        private Visibility ToVisibilityState(bool b) {
            return b ? Visibility.Visible : Visibility.Hidden;
        }
        #endregion
    }
}