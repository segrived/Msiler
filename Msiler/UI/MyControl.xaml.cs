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
        private const int MAX_CODE_LINES_IN_HINT = 5;

        private static readonly Regex OffsetRegex = new Regex(@"^(IL_[\dA-F]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private readonly Dictionary<string, int> offsetLinesCache = new Dictionary<string, int>();

        private IHighlightingDefinition currentHighlightDefinition;

        private readonly AssemblyManager assemblyManager = new AssemblyManager();

        private AssemblyMethod currentMethod;

        private TextEditorWordProcesor textWordProcessor;

        private AssemblyMethod CurrentMethod
        {
            get => this.currentMethod;
            set
            {
                this.currentMethod = value;
                this.MethodsList.SelectedItem = value;
            }
        }

        private List<AssemblyMethod> assemblyMethods;

        private readonly Dictionary<AssemblyMethod, string> listingCache = new Dictionary<AssemblyMethod, string>();

        public MyControl()
        {
            this.InitializeComponent();

            this.textWordProcessor = new TextEditorWordProcesor(this.BytecodeListing);

            this.InitConfiguration();
            this.InitEventHandlers();
        }

        private void OnMethodListChanged(object sender, MethodsListEventArgs e)
        {
            this.assemblyMethods = e.Methods;
            this.MethodsList.ItemsSource = new ObservableCollection<AssemblyMethod>(this.assemblyMethods);
            this.listingCache.Clear();

            if (this.CurrentMethod != null)
                this.ProcessMethod(e.Methods.FirstOrDefault(m => m.Equals(this.CurrentMethod)));

            var view = CollectionViewSource.GetDefaultView(this.MethodsList.ItemsSource);
            view.Filter = this.FilterMethodsList;

            bool isAnyMethods = this.assemblyMethods.Count > 0;
            this.MainView.Visibility = ToVisibilityState(isAnyMethods);
            this.WelcomeUserControl.Visibility = ToVisibilityState(!isAnyMethods);
        }

        private void InitConfiguration()
        {
            this.BytecodeListing.Options = new TextEditorOptions
            {
                EnableEmailHyperlinks = false,
                EnableHyperlinks = false
            };
            this.UpdateDisplayOptions();
        }

        private void UpdateDisplayOptions()
        {
            var displayOptions = Common.Instance.DisplayOptions;

            var scheme = MsilerColorScheme.GetHighlightingScheme(displayOptions.ColorScheme);
            this.currentHighlightDefinition = MsilerColorScheme.GetDefinition(scheme);

            string fontFamily = "Consolas";
            if (FontHelpers.IsFontFamilyExist(displayOptions.FontName))
                fontFamily = displayOptions.FontName;

            this.BytecodeListing.FontFamily = new FontFamily(fontFamily);
            this.BytecodeListing.FontSize = displayOptions.FontSize;
            this.BytecodeListing.ShowLineNumbers = displayOptions.LineNumbers;
            this.BytecodeListing.SyntaxHighlighting = this.currentHighlightDefinition;
        }

        private void InitEventHandlers()
        {
            Common.Instance.DisplayOptions.Applied += (s, e)
                => this.UpdateDisplayOptions();

            Common.Instance.ListingGenerationOptions.Applied += (s, e) =>
            {
                this.listingCache.Clear();
                this.ProcessMethod(this.CurrentMethod);
            };

            Common.Instance.ExcludeOptions.Applied += (s, e) =>
            {
                if (this.MethodsList.ItemsSource != null)
                    CollectionViewSource.GetDefaultView(this.MethodsList.ItemsSource).Refresh();
            };

            VSColorTheme.ThemeChanged += e => this.UpdateDisplayOptions();

            FunctionFollower.MethodSelected += this.OnMethodSelected;
            this.assemblyManager.MethodListChanged += this.OnMethodListChanged;
        }

        private void OnMethodSelected(object sender, MethodSignatureEventArgs e) {
            if (this.assemblyMethods == null || this.assemblyMethods.Count == 0) {
                return;
            }
            // do not process if same method
            if (this.CurrentMethod != null && this.CurrentMethod.Signature.Equals(e.MethodSignature)) {
                return;
            }
            // find metyhod with same signature
            var method = this.assemblyMethods.FirstOrDefault(m => m.Signature.Equals(e.MethodSignature));
            this.ProcessMethod(method, false);
        }


        private bool FilterMethodsList(object o)
        {
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

        private ListingGeneratorOptions GeneratorOptions 
            => Common.Instance.ListingGenerationOptions.ToListingGeneratorOptions();

        private void ProcessMethod(AssemblyMethod method, bool clearIfNull = true)
        {
            if (method != null)
            {
                try
                {
                    this.CurrentMethod = method;
                    string listingText;
                    if (Common.Instance.GeneralOptions.EnableCaching)
                    {
                        if (this.listingCache.ContainsKey(method))
                            listingText = this.listingCache[method];
                        else
                        {
                            listingText = this.CurrentMethod.GenerateListing(this.GeneratorOptions);
                            this.listingCache[method] = listingText;
                        }
                    }
                    else
                        listingText = this.CurrentMethod.GenerateListing(this.GeneratorOptions);

                    this.BytecodeListing.Text = listingText;
                    this.BytecodeListing.ScrollToHome();
                    this.BytecodeListing.CaretOffset = 0;

                    this.offsetLinesCache.Clear();
                    var lines = listingText.Lines();
                    for (int i = 0; i < lines.Length; i++)
                    {
                        var match = OffsetRegex.Match(lines[i]);
                        if (match.Success)
                            this.offsetLinesCache.Add(match.Value, i + 1);
                    }
                }
                catch (Exception ex)
                {
                    var errorBuilder = new StringBuilder();
                    errorBuilder.AppendLine($"ERROR: {ex.Message}");
                    errorBuilder.AppendLine($"Source: {ex.Source}");
                    errorBuilder.Append($"Stacktrace: {ex.StackTrace}");
                    this.BytecodeListing.Text = errorBuilder.ToString();
                }
            }
            else
            {
                if (!clearIfNull)
                    return;

                this.CurrentMethod = null;
                this.BytecodeListing.Text = String.Empty;
                this.offsetLinesCache.Clear();
            }
        }

        #region Instruction Hint Tooltip

        private readonly ToolTip toolTip = new ToolTip();

        private bool TryNavigateTo()
        {
            if (!this.textWordProcessor.IsValidWord)
                return false;

            // try naviage to offset
            var match = OffsetRegex.Match(this.textWordProcessor.Word);
            if (match.Success)
            {
                int line = this.offsetLinesCache[match.Value];
                int offset = this.BytecodeListing.Document.GetOffset(line, 0);
                this.BytecodeListing.CaretOffset = offset;
                this.BytecodeListing.ScrollToLine(line);
                this.BytecodeListing.Focus();

                return true;
            }

            // try navigate to method
            var method = this.assemblyMethods?.FirstOrDefault(m => m.Signature.MethodName == this.textWordProcessor.Word);
            if (method == null)
                return false;

            this.CurrentMethod = method;
            return true;
        }

        private void ShowToolTip(string content, IHighlightingDefinition highlight = null)
        {
            int tooltipTransp = Common.Instance.DisplayOptions.TooltipTransparency;

            this.toolTip.PlacementTarget = this;

            this.toolTip.Opacity = 1.0 - (tooltipTransp < 0 || tooltipTransp > 100 ? 0 : tooltipTransp) / 100.0;

            var bgDColor = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowBackgroundBrushKey);
            var bgMColor = Color.FromRgb(bgDColor.R, bgDColor.G, bgDColor.B);
            this.toolTip.Background = new SolidColorBrush(bgMColor);

            var fgDColor = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowTextBrushKey);
            var fgMColor = Color.FromRgb(fgDColor.R, fgDColor.G, fgDColor.B);

            this.toolTip.Content = new TextEditor
            {
                Text = content,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden,
                VerticalScrollBarVisibility = ScrollBarVisibility.Hidden,
                SyntaxHighlighting = highlight,
                Background = Brushes.Transparent,
                FontFamily = new FontFamily(Common.Instance.DisplayOptions.FontName),
                FontSize = Common.Instance.DisplayOptions.FontSize,
                Foreground = new SolidColorBrush(fgMColor)
            };

            this.toolTip.IsOpen = true;
        }

        #endregion

        #region UI handlers

        private void BytecodeListing_MouseHover(object sender, MouseEventArgs e)
        {
            if (!this.textWordProcessor.IsValidWord)
                return;

            var offsetMatch = OffsetRegex.Match(this.textWordProcessor.Word);

            if (offsetMatch.Success && !this.textWordProcessor.IsOnLineStart)
            {
                string offsetStr = offsetMatch.Value;
                if (!this.offsetLinesCache.ContainsKey(offsetStr))
                {
                    e.Handled = true;
                    return;
                }

                int lineNumber = this.offsetLinesCache[offsetStr];
                int lineCount = this.BytecodeListing.LineCount;

                var sb = new StringBuilder();

                var docLine = this.BytecodeListing.Document.GetLineByNumber(lineNumber);
                for (int i = 0; i < MAX_CODE_LINES_IN_HINT; i++)
                {
                    if (docLine.LineNumber >= lineCount)
                        break;

                    string lineContent = this.BytecodeListing.Document.GetText(docLine.Offset, docLine.Length);
                    sb.AppendLine(lineContent);
                    docLine = docLine.NextLine;
                }

                // use to naviage information
                sb.AppendLine();
                sb.AppendLine("// HINT: Double click to navigate");

                this.ShowToolTip(sb.ToString().TrimEnd('\r', '\n'), this.currentHighlightDefinition);
            }

            var numberUnderCursor = StringHelpers.ParseNumber(this.textWordProcessor.Word);

            if (numberUnderCursor != null)
            {
                long v = numberUnderCursor.Value;

                string hint = String.Empty;

                hint += $"Decimal: {v}{Environment.NewLine}";
                hint += $"HEX:     0x{Convert.ToString(v, 16).ToUpper()}{Environment.NewLine}";
                hint += $"Binary:  0b{Convert.ToString(v, 2)}{Environment.NewLine}";
                hint += $"Octal:   0{Convert.ToString(v, 8)}";

                this.ShowToolTip(hint, this.currentHighlightDefinition);
            }

            var info = AssemblyParser.Helpers.GetInstructionInformation(this.textWordProcessor.Word);
            if (info != null)
                this.ShowToolTip($"{info.Name}: {info.Description}");

            e.Handled = true;
        }

        private void BytecodeListing_MouseHoverStopped(object sender, MouseEventArgs e) 
            => this.toolTip.IsOpen = false;

        private void BytecodeListing_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            bool isCtrlMod = Keyboard.Modifiers.HasFlag(ModifierKeys.Control);

            if (e.ChangedButton == MouseButton.Middle || (e.ChangedButton == MouseButton.Left && isCtrlMod))
            {
                if (this.TryNavigateTo())
                    e.Handled = true;
            }
        }

        private void BytecodeListing_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (this.TryNavigateTo())
            {
                e.Handled = true;
                return;
            }

            // detect method name under cursor
            
            e.Handled = true;
        }

        private void BytecodeListing_OnMouseMove(object sender, MouseEventArgs e) 
            => this.textWordProcessor.UpdateByPoint(e.GetPosition(this.BytecodeListing));

        private void MethodsList_SelectionChanged(object sender, SelectionChangedEventArgs e) 
            => this.ProcessMethod((AssemblyMethod)this.MethodsList.SelectedItem);

        private void FilterMethodsTextBox_TextChanged(object sender, TextChangedEventArgs e) 
            => CollectionViewSource.GetDefaultView(this.MethodsList.ItemsSource).Refresh();

        private void HyperlinkOptions_Click(object sender, RoutedEventArgs e) 
            => Common.Instance.Package.ShowOptionPage(typeof(ExtensionGeneralOptions));

        private void HyperlinkGithub_Click(object sender, RoutedEventArgs e) 
            => Process.Start(Common.RepoUrl);

        private void HyperlinkAbout_Click(object sender, RoutedEventArgs e) 
            => new AboutWindow().ShowDialog();

        private void IsFollowModeEnabled_CheckedChange(object sender, RoutedEventArgs e) 
            => FunctionFollower.IsFollowingEnabled = ((CheckBox)sender).IsChecked.Value;

        private void MenuItemGeneralOptions_Click(object sender, RoutedEventArgs e) 
            => Common.Instance.Package.ShowOptionPage(typeof(ExtensionGeneralOptions));

        private void MenuItemListingGenearationOptions_Click(object sender, RoutedEventArgs e) 
            => Common.Instance.Package.ShowOptionPage(typeof(ExtensionListingGenerationOptions));

        private void MenuItemMethodFilteringOptions_Click(object sender, RoutedEventArgs e) 
            => Common.Instance.Package.ShowOptionPage(typeof(ExtensionExcludeOptions));

        private void MenuItemDisplayOptions_Click(object sender, RoutedEventArgs e) 
            => Common.Instance.Package.ShowOptionPage(typeof(ExtensionDisplayOptions));

        private void OptionsLink_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
                return;

            if (sender is TextBlock textBlock)
            {
                var contextMenu = textBlock.ContextMenu;
                if (contextMenu == null)
                    return;

                contextMenu.PlacementTarget = textBlock;
                contextMenu.IsOpen = true;
            }

            e.Handled = true;
        }
        #endregion UI handlers

        #region Helper methods

        private static Visibility ToVisibilityState(bool b) 
            => b ? Visibility.Visible : Visibility.Hidden;

        #endregion
    }
}