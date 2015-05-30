using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Mono.Cecil.Cil;
using Quart.Msiler.Annotations;

namespace Quart.Msiler
{
    public class MyControlVM : INotifyPropertyChanged, IVsUpdateSolutionEvents, IVsSolutionEvents
    {
        private string _filterString;
        private bool _hideNopInstructions;
        private ICollectionView _instructionsView;
        private byte[] _lastBuildMd5Hash;
        private ObservableCollection<MsilMethodEntity> _methods;
        private ICollectionView _methodsView;
        private MsilInstruction _selectedInstruction;
        private MsilMethodEntity _selectedMethod;

        public bool HideNopInstructions
        {
            get { return _hideNopInstructions; }
            set
            {
                if (value == _hideNopInstructions) {
                    return;
                }
                _hideNopInstructions = value;
                this._instructionsView.Refresh();
                OnPropertyChanged();
            }
        }

        public MsilInstruction SelectedInstruction
        {
            get { return _selectedInstruction; }
            set
            {
                if (Equals(value, _selectedInstruction)) {
                    return;
                }
                _selectedInstruction = value;
                OnPropertyChanged();
            }
        }

        public string FilterString
        {
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

        public ObservableCollection<MsilMethodEntity> Methods
        {
            get { return _methods; }
            set
            {
                if (value == null || Equals(value, _methods)) {
                    return;
                }
                _methods = new ObservableCollection<MsilMethodEntity>(value);
                OnPropertyChanged();
                try {
                    if (this.SelectedMethod == null) {
                        return;
                    }
                    string selectedName = this.SelectedMethod.MethodData.FullName;
                    this.SelectedMethod =
                        value.FirstOrDefault(x => x.MethodData.FullName == selectedName);
                } catch {
                    // ignored
                }
            }
        }

        public MsilMethodEntity SelectedMethod
        {
            get { return _selectedMethod; }
            set
            {
                if (value == null || value.Equals(_selectedMethod)) {
                    return;
                }
                _selectedMethod = value;
                OnPropertyChanged();
                this.UpdateInstructionsFilter();
            }
        }

        public MyControlVM()
        {
            uint solutionUpdateCookie;
            uint solutionCookie;
            Common.Instance.Build.AdviseUpdateSolutionEvents(this, out solutionUpdateCookie);
            Common.Instance.Solution.AdviseSolutionEvents(this, out solutionCookie);
            Common.Instance.SolutionUpdateCookie = solutionUpdateCookie;
            Common.Instance.SolutionCookie = solutionCookie;
            this.Methods = new ObservableCollection<MsilMethodEntity>();
            this.UpdateMethodsFilter();
            this.FilterString = "";
            Debug.WriteLine(MsilInstructionsDescription.InstructionDescriptions.Max(x => x.Key.Length));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public int OnAfterCloseSolution(object pUnkReserved)
        {
            this.Methods.Clear(); // empty collection
            return VSConstants.S_OK;
        }

        public int UpdateSolution_Done(int fSucceeded, int fModified, int fCancelCommand)
        {
            if (! MyToolWindow.IsVisible || fSucceeded != 1) {
                return VSConstants.S_OK;
            }
            Debug.Write("Compiled, generating IL code...");
            string assemblyFile = Helpers.GetOutputAssemblyFileName();
            try {
                var hash = Helpers.ComputeMd5(assemblyFile);
                // if assembly was not changed
                if (_lastBuildMd5Hash != null && _lastBuildMd5Hash.SequenceEqual(hash)) {
                    return VSConstants.S_OK;
                }
                var msilReader = new MsilReader(assemblyFile);

                var methodsEnumerable = msilReader.EnumerateMethods();
                this.Methods = new ObservableCollection<MsilMethodEntity>(methodsEnumerable);
                _lastBuildMd5Hash = hash;
                this.UpdateMethodsFilter();
            } catch {
                this.Methods = new ObservableCollection<MsilMethodEntity>();
            }
            Debug.WriteLine("Done");
            return VSConstants.S_OK;
        }

        private void UpdateInstructionsFilter()
        {
            this._instructionsView =
                CollectionViewSource.GetDefaultView(this.SelectedMethod.Instructions);
            this._instructionsView.Filter = o => {
                if (! this.HideNopInstructions) {
                    return true;
                }
                var obj = (MsilInstruction)o;
                return obj.OpCode.Code != Code.Nop;
            };
        }

        private void UpdateMethodsFilter()
        {
            this._methodsView = CollectionViewSource.GetDefaultView(this.Methods);
            this._methodsView.Filter = o => {
                if (String.IsNullOrEmpty(this.FilterString)) {
                    return true;
                }
                var obj = (MsilMethodEntity)o;
                return obj.MethodData.FullName.ToLower().Contains(this.FilterString.ToLower());
            };
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #region Unused handlers

        public int UpdateSolution_Begin(ref int pfCancelUpdate)
        {
            return VSConstants.S_OK;
        }

        public int UpdateSolution_StartUpdate(ref int pfCancelUpdate)
        {
            return VSConstants.S_OK;
        }

        public int UpdateSolution_Cancel()
        {
            return VSConstants.S_OK;
        }

        public int OnActiveProjectCfgChange(IVsHierarchy pIVsHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeCloseSolution(object pUnkReserved)
        {
            return VSConstants.S_OK;
        }

        #endregion
    }
}