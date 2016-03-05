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
    public partial class MyControl
    {
        const int MaxCodeLinesInHint = 5;

        static readonly Regex OffsetRegex =
            new Regex(@"^(IL_[\dA-F]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        readonly Dictionary<string, int> _offsetLinesCache
            = new Dictionary<string, int>();

        IHighlightingDefinition _currentHighlightDefinition;

        readonly AssemblyManager _assemblyManager = new AssemblyManager();

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

        readonly Dictionary<AssemblyMethod, string> _listingCache =
            new Dictionary<AssemblyMethod, string>();

        public MyControl() {
            this.InitializeComponent();

            this.InitConfiguration();
            this.InitEventHandlers();
        }

        void OnMethodListChanged(object sender, MethodsListEventArgs e) {
            this._assemblyMethods = e.Methods;
            this.MethodsList.ItemsSource = new ObservableCollection<AssemblyMethod>(this._assemblyMethods);
            this._listingCache.Clear();

            if (this.CurrentMethod != null) {
                this.ProcessMethod(e.Methods.FirstOrDefault(m => m.Equals(this.CurrentMethod)));
            }
            var view = CollectionViewSource.GetDefaultView(this.MethodsList.ItemsSource);
            view.Filter = this.FilterMethodsList;

            bool isAnyMethods = this._assemblyMethods.Count > 0;
            this.MainView.Visibility = this.ToVisibilityState(isAnyMethods);
            this.WelcomeUserControl.Visibility = this.ToVisibilityState(!isAnyMethods);
        }

        public void InitConfiguration() {
            this.BytecodeListing.Options = new TextEditorOptions {
                EnableEmailHyperlinks = false,
                EnableHyperlinks = false
            };
            this.UpdateDisplayOptions();
        }

        void UpdateDisplayOptions() {
            var displayOptions = Common.Instance.DisplayOptions;

            var scheme = ColorTheme.GetHighlightingScheme(displayOptions.ColorTheme);
            this._currentHighlightDefinition = ColorTheme.GetDefinition(scheme);

            string fontFamily = "Consolas";
            if (FontHelpers.IsFontFamilyExist(displayOptions.FontName)) {
                fontFamily = displayOptions.FontName;
            }
            this.BytecodeListing.FontFamily = new FontFamily(fontFamily);
            this.BytecodeListing.FontSize = displayOptions.FontSize;
            this.BytecodeListing.ShowLineNumbers = displayOptions.LineNumbers;
            this.BytecodeListing.SyntaxHighlighting = this._currentHighlightDefinition;
        }

        public void InitEventHandlers() {
            Common.Instance.DisplayOptions.Applied += (s, e)
                => this.UpdateDisplayOptions();
            Common.Instance.ListingGenerationOptions.Applied += (s, e) => {
                this._listingCache.Clear();
                this.ProcessMethod(this.CurrentMethod);
            };
            Common.Instance.ExcludeOptions.Applied += (s, e) => {
                if (this.MethodsList.ItemsSource != null) {
                    CollectionViewSource.GetDefaultView(this.MethodsList.ItemsSource).Refresh();
                }
            };
            VSColorTheme.ThemeChanged += (e) => this.UpdateDisplayOptions();

            FunctionFollower.MethodSelected += this.OnMethodSelected;
            this._assemblyManager.MethodListChanged += this.OnMethodListChanged;
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
            this.ProcessMethod(method, false);
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
            string filterQuery = this.FilterMethodsTextBox.Text;
            return String.IsNullOrEmpty(filterQuery) 
                || method.Signature.MethodName.Contains(filterQuery, StringComparison.OrdinalIgnoreCase);
        }

        public ListingGeneratorOptions GeneratorOptions 
            => Common.Instance.ListingGenerationOptions.ToListingGeneratorOptions();

        private void ProcessMethod(AssemblyMethod method, bool clearIfNull = true) {
            if (method != null) {
                try {
                    this.CurrentMethod = method;
                    string listingText;
                    if (Common.Instance.GeneralOptions.EnableCaching) {
                        if (this._listingCache.ContainsKey(method)) {
                            listingText = this._listingCache[method];
                        } else {
                            listingText = this.CurrentMethod.GenerateListing(this.GeneratorOptions);
                            this._listingCache[method] = listingText;
                        }
                    } else {
                        listingText = this.CurrentMethod.GenerateListing(this.GeneratorOptions);
                    }

                    this.BytecodeListing.Text = listingText;
                    this.BytecodeListing.ScrollToHome();
                    this.BytecodeListing.CaretOffset = 0;

                    this._offsetLinesCache.Clear();
                    var lines = listingText.Lines();
                    for (int i = 0; i < lines.Length; i++) {
                        var match = OffsetRegex.Match(lines[i]);
                        if (match.Success) {
                            this._offsetLinesCache.Add(match.Value, i + 1);
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
                if (!clearIfNull) {
                    return;
                }
                this.CurrentMethod = null;
                this.BytecodeListing.Text = String.Empty;
                this._offsetLinesCache.Clear();
            }
        }

        #region Instruction Hint Tooltip
        readonly ToolTip _toolTip = new ToolTip();

        string GetWordUnderCursor(Point p) {
            return AvalonEditHelpers.GetWordOnOffset(this.BytecodeListing, p);
        }

        void BytecodeListing_MouseHover(object sender, MouseEventArgs e) {
            var wordUnderCursor = this.GetWordUnderCursor(e.GetPosition(this.BytecodeListing));

            var offsetMatch = OffsetRegex.Match(wordUnderCursor);
            if (offsetMatch.Success) {
                var offsetStr = offsetMatch.Value;
                if (!this._offsetLinesCache.ContainsKey(offsetStr)) {
                    e.Handled = true;
                    return;
                }
                var lineNumber = this._offsetLinesCache[offsetStr];
                var lineCount = this.BytecodeListing.LineCount;
                var sb = new StringBuilder();

                var docLine = this.BytecodeListing.Document.GetLineByNumber(lineNumber);
                for (int i = 0; i < MaxCodeLinesInHint; i++) {
                    if (docLine.LineNumber >= lineCount) {
                        break;
                    }
                    var lineContent = this.BytecodeListing.Document.GetText(docLine.Offset, docLine.Length);
                    sb.AppendLine(lineContent);
                    docLine = docLine.NextLine;
                }
                this.ShowToolTip(sb.ToString().TrimEnd('\r', '\n'), this._currentHighlightDefinition);
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
                this.ShowToolTip(hint, this._currentHighlightDefinition);
            }

            var info = AssemblyParser.Helpers.GetInstructionInformation(wordUnderCursor);
            if (info != null) {
                this.ShowToolTip($"{info.Name}: {info.Description}");
            }
            e.Handled = true;
        }

        void BytecodeListing_MouseHoverStopped(object sender, MouseEventArgs e) {
            this._toolTip.IsOpen = false;
        }

        private void BytecodeListing_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e) {
            var wordUnderCursor = this.GetWordUnderCursor(e.GetPosition(this.BytecodeListing));

            // detect offset under cursor
            var match = OffsetRegex.Match(wordUnderCursor);
            if (match.Success) {
                var line = this._offsetLinesCache[match.Value];
                var offset = this.BytecodeListing.Document.GetOffset(line, 0);
                this.BytecodeListing.CaretOffset = offset;
                this.BytecodeListing.ScrollToLine(line);
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
            this._toolTip.PlacementTarget = this;
            int transpLevel = (displayOptions.TooltipTransparency < 0 || displayOptions.TooltipTransparency > 100)
                ? 0
                : displayOptions.TooltipTransparency;
            this._toolTip.Opacity = 1.0 - (transpLevel / 100.0);

            var bgDColor = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowBackgroundBrushKey);
            var bgMColor = Color.FromRgb(bgDColor.R, bgDColor.G, bgDColor.B);
            this._toolTip.Background = new SolidColorBrush(bgMColor);

            var fgDColor = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowTextBrushKey);
            var fgMColor = Color.FromRgb(fgDColor.R, fgDColor.G, fgDColor.B);

            this._toolTip.Content = new TextEditor {
                Text = content,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden,
                VerticalScrollBarVisibility = ScrollBarVisibility.Hidden,
                SyntaxHighlighting = highlight,
                Background = Brushes.Transparent,
                FontFamily = new FontFamily(displayOptions.FontName),
                FontSize = displayOptions.FontSize,
                Foreground = new SolidColorBrush(fgMColor)
            };
            this._toolTip.IsOpen = true;
        }

        #endregion

        #region UI handlers
        void MethodsList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            this.ProcessMethod((AssemblyMethod)this.MethodsList.SelectedItem);
        }

        void FilterMethodsTextBox_TextChanged(object sender, TextChangedEventArgs e) =>
            CollectionViewSource.GetDefaultView(this.MethodsList.ItemsSource).Refresh();

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
            if (e.ChangedButton != MouseButton.Left) {
                return;
            }
            var block = sender as TextBlock;
            var contextMenu = block.ContextMenu;
            contextMenu.PlacementTarget = block;
            contextMenu.IsOpen = true;
            e.Handled = true;
        }
        #endregion UI handlers

        #region Helper methods
        private Visibility ToVisibilityState(bool b) {
            return b ? Visibility.Visible : Visibility.Hidden;
        }
        #endregion
    }
}