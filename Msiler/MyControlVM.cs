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

namespace Quart.Msiler
{
    public class MyControlVM : INotifyPropertyChanged, IVsUpdateSolutionEvents, IVsSolutionEvents
    {
        ICollectionView _methodsView;
        byte[] _lastBuildMd5Hash;

        public ListingGenerator _generator = new ListingGenerator();

        public MyControlVM() {
            this.UpdateMethodsFilter();
            InitCommon();
            ProcessOptions();
            Common.Instance.Options.Applied += Options_Applied;
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

        public void UpdateBytecodeListing() {

            if (this.SelectedMethod != null) {
                this.BytecodeListing = _generator.Generate(this.SelectedMethod.Instructions);
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
                if (value == null || Equals(value, _methods)) {
                    return;
                }
                _methods = new ObservableCollection<MethodEntity>(value);
                OnPropertyChanged();
                try {
                    if (this.SelectedMethod == null) {
                        return;
                    }
                    string selectedName = this.SelectedMethod.MethodData.FullName;
                    this.SelectedMethod = value.FirstOrDefault(x => x.MethodData.FullName == selectedName);
                } catch {
                    // ignored
                }
            }
        }

        private MethodEntity _selectedMethod;
        public MethodEntity SelectedMethod {
            get { return _selectedMethod; }
            set
            {
                if (value == null || value.Equals(_selectedMethod)) {
                    return;
                }
                _selectedMethod = value;
                UpdateBytecodeListing();
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public int OnAfterCloseSolution(object pUnkReserved) {
            this.Methods.Clear(); // empty collection
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
                var hash = Helpers.ComputeMD5(assemblyFile);
                // if assembly was not changed
                if (_lastBuildMd5Hash != null && _lastBuildMd5Hash.SequenceEqual(hash)) {
                    return VSConstants.S_OK;
                }
                var msilReader = new MsilReader(assemblyFile);

                var methodsEnumerable = msilReader.EnumerateMethods();
                this.Methods = new ObservableCollection<MethodEntity>(methodsEnumerable);
                _lastBuildMd5Hash = hash;
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
                var obj = (MethodEntity)o;
                if (this.ExcludeProperties) {
                    if (obj.MethodData.IsGetter || obj.MethodData.IsSetter) {
                        return false;
                    }
                }
                if (String.IsNullOrEmpty(this.FilterString)) {
                    return true;
                }

                return obj.Name.ToLower().Contains(this.FilterString.ToLower());
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