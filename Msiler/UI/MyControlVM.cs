using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Quart.Msiler.Annotations;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Highlighting;
using Microsoft.VisualStudio.PlatformUI;
using Quart.Msiler.Lib;
using System.IO;

namespace Quart.Msiler.UI
{
    public class MyControlVM : INotifyPropertyChanged, IVsUpdateSolutionEvents, IVsSolutionEvents
    {
        ICollectionView _methodsView;
        DateTime _previousAssemblyWriteTime;

        public ListingGenerator _generator = new ListingGenerator();

        public MyControlVM() {
            this.UpdateMethodsFilter();
            InitCommon();
            ProcessOptions();
            Common.Instance.Options.Applied += Options_Applied;
            VSColorTheme.ThemeChanged += VSColorTheme_ThemeChanged;
        }

        private void VSColorTheme_ThemeChanged(ThemeChangedEventArgs e) {
            this.HighlightingDefinition = ColorTheme.GetColorTheme(Common.Instance.Options.ColorTheme);
        }

        private void Options_Applied(object sender, EventArgs e) {
            ProcessOptions();
            UpdateBytecodeListing();
        }

        public void ProcessOptions() {
            string fontFamily = "Consolas"; // default font family
            if (Helpers.IsFontFamilyExist(Common.Instance.Options.FontName)) {
                fontFamily = Common.Instance.Options.FontName;
            }
            this.ListingFontName = new FontFamily(fontFamily);
            this.ListingFontSize = Common.Instance.Options.FontSize;
            this.ShowLineNumbers = Common.Instance.Options.LineNumbers;
            this.ExcludeProperties = Common.Instance.Options.ExcludeProperties;
            this.ExcludeSpecialMethods = Common.Instance.Options.ExcludeAnonymousMethods;
            this.ExcludeContructors = Common.Instance.Options.ExcludeConstructors;

            this.HighlightingDefinition = ColorTheme.GetColorTheme(Common.Instance.Options.ColorTheme);
            // reset last write time after config change
            this._previousAssemblyWriteTime = default(DateTime);
        }

        public void InitCommon() {
            uint solutionUpdateCookie;
            uint solutionCookie;
            Common.Instance.BuildManager.AdviseUpdateSolutionEvents(this, out solutionUpdateCookie);
            Common.Instance.SolutionManager.AdviseSolutionEvents(this, out solutionCookie);
            Common.Instance.SolutionUpdateCookie = solutionUpdateCookie;
            Common.Instance.SolutionCookie = solutionCookie;
        }

        private int _listingFontSize;
        public int ListingFontSize {
            get { return _listingFontSize; }
            set
            {
                if (value == _listingFontSize) {
                    return;
                }
                _listingFontSize = value;
                OnPropertyChanged();
            }
        }

        private FontFamily _listingFontName;
        public FontFamily ListingFontName {
            get { return _listingFontName; }
            set
            {
                if (value == _listingFontName) {
                    return;
                }
                _listingFontName = value;
                OnPropertyChanged();
            }
        }

        public bool _showLineNumbers;
        public bool ShowLineNumbers {
            get { return _showLineNumbers; }
            set
            {
                if (value == _showLineNumbers) {
                    return;
                }
                _showLineNumbers = value;
                OnPropertyChanged();
            }
        }

        private bool _excludeProperties;
        public bool ExcludeProperties {
            get { return _excludeProperties; }
            set
            {
                if (value == _excludeProperties) {
                    return;
                }
                _excludeProperties = value;
                this._methodsView.Refresh();
                OnPropertyChanged();
            }
        }

        private bool _excludeSpecialMethods;
        public bool ExcludeSpecialMethods {
            get { return _excludeSpecialMethods; }
            set
            {
                if (value == _excludeSpecialMethods) {
                    return;
                }
                _excludeSpecialMethods = value;
                this._methodsView.Refresh();
                OnPropertyChanged();
            }
        }

        private bool _excludeContructors;
        public bool ExcludeContructors {
            get { return _excludeContructors; }
            set
            {
                if (value == _excludeContructors) {
                    return;
                }
                _excludeContructors = value;
                this._methodsView.Refresh();
                OnPropertyChanged();
            }
        }

        private IHighlightingDefinition _highlightingDefinition;
        public IHighlightingDefinition HighlightingDefinition {
            get { return _highlightingDefinition; }
            set
            {
                if (value == _highlightingDefinition) {
                    return;
                }
                _highlightingDefinition = value;
                OnPropertyChanged();
            }
        }

        public void UpdateBytecodeListing() {

            if (this.SelectedMethod != null) {
                this.BytecodeListing = _generator.Generate(this.SelectedMethod);
            } else {
                this.BytecodeListing = "Please select method";
            }
        }

        private string _bytecodeListing;
        public string BytecodeListing {
            get { return _bytecodeListing; }
            set
            {
                if (Equals(value, _bytecodeListing)) {
                    return;
                }
                _bytecodeListing = value;
                OnPropertyChanged();
            }
        }

        private string _filterString = "";
        public string FilterString {
            get { return _filterString; }
            set
            {
                if (value == _filterString) {
                    return;
                }
                _filterString = value;
                this._methodsView.Refresh();
                OnPropertyChanged();
            }
        }

        private ObservableCollection<MethodEntity> _methods = new ObservableCollection<MethodEntity>();
        public ObservableCollection<MethodEntity> Methods {
            get { return _methods; }
            set
            {
                _methods = new ObservableCollection<MethodEntity>(value);
                if (this.SelectedMethod != null) {
                    this.SelectedMethod = value.FirstOrDefault(m => m.MethodName == this.SelectedMethod.MethodName);
                }
                OnPropertyChanged();
            }
        }

        private MethodEntity _selectedMethod;
        public MethodEntity SelectedMethod {
            get { return _selectedMethod; }
            set
            {
                _selectedMethod = value;
                UpdateBytecodeListing();
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public int OnAfterCloseSolution(object pUnkReserved) {
            this.Methods.Clear(); // empty collection
            this._previousAssemblyWriteTime = default(DateTime);
            this.SelectedMethod = null;
            return VSConstants.S_OK;
        }

        public int UpdateSolution_Done(int fSucceeded, int fModified, int fCancelCommand) {
            if (Common.Instance.Options.UpdateListingOnlyIfVisible && !MsilerToolWindow.IsVisible) {
                return VSConstants.S_OK;
            }
            if (fSucceeded != 1) {
                return VSConstants.S_OK;
            }
            Debug.Write("Compiled, generating IL code...");
            string assemblyFile = Helpers.GetOutputAssemblyFileName();
            try {
                var assemblyWriteTime = new FileInfo(assemblyFile).LastWriteTime;
                // if assembly was not changed
                if (_previousAssemblyWriteTime == assemblyWriteTime) {
                    return VSConstants.S_OK;
                }
                var msilReader = new MsilReader(assemblyFile, Common.Instance.Options.ProcessPDBFiles);

                var methodsEnumerable = msilReader.EnumerateMethods();
                this._generator.ClearSourceCache();
                this.Methods = new ObservableCollection<MethodEntity>(methodsEnumerable);
                _previousAssemblyWriteTime = assemblyWriteTime;
                this.UpdateMethodsFilter();
            } catch {
                this.Methods = new ObservableCollection<MethodEntity>();
            }
            Debug.WriteLine("Done");
            return VSConstants.S_OK;
        }

        private void UpdateMethodsFilter() {
            this._methodsView = CollectionViewSource.GetDefaultView(this.Methods);
            this._methodsView.Filter = o => {
                var me = (MethodEntity)o;
                if (this.ExcludeSpecialMethods && me.IsAnonymous()) {
                    return false;
                }
                if (this.ExcludeProperties && me.IsProperty()) {
                    return false;
                }
                if (this.ExcludeContructors && me.MethodData.IsConstructor) {
                    return false;
                }
                if (String.IsNullOrEmpty(this.FilterString)) {
                    return true;
                }
                return me.MethodName.ToLower().Contains(this.FilterString.ToLower());
            };
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            var handler = PropertyChanged;
            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #region Unused handlers

        public int UpdateSolution_Begin(ref int pfCancelUpdate) {
            return VSConstants.S_OK;
        }

        public int UpdateSolution_StartUpdate(ref int pfCancelUpdate) {
            return VSConstants.S_OK;
        }

        public int UpdateSolution_Cancel() {
            return VSConstants.S_OK;
        }

        public int OnActiveProjectCfgChange(IVsHierarchy pIVsHierarchy) {
            return VSConstants.S_OK;
        }

        public int OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded) {
            return VSConstants.S_OK;
        }

        public int OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel) {
            return VSConstants.S_OK;
        }

        public int OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved) {
            return VSConstants.S_OK;
        }

        public int OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy) {
            return VSConstants.S_OK;
        }

        public int OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel) {
            return VSConstants.S_OK;
        }

        public int OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy) {
            return VSConstants.S_OK;
        }

        public int OnAfterOpenSolution(object pUnkReserved, int fNewSolution) {
            return VSConstants.S_OK;
        }

        public int OnQueryCloseSolution(object pUnkReserved, ref int pfCancel) {
            return VSConstants.S_OK;
        }

        public int OnBeforeCloseSolution(object pUnkReserved) {
            return VSConstants.S_OK;
        }

        #endregion
    }
}