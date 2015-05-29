using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Mono.Cecil;
using Quart.Msiler.Annotations;

namespace Quart.Msiler
{
    public class MyControlVM : INotifyPropertyChanged, IVsUpdateSolutionEvents
    {
        private MsilMethodEntity _selectedMethod;
        private ObservableCollection<MsilMethodEntity> _methods;
        private ICollectionView _methodsView;
        private string _filterString;

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
                    this.SelectedMethod = value.FirstOrDefault(x => x.MethodData.FullName == selectedName);
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
            }
        }

        public MyControlVM()
        {
            uint cookie;
            Common.Instance.Build.AdviseUpdateSolutionEvents(this, out cookie);
            Common.Instance.SolutionCookie = cookie;
            this.Methods = new ObservableCollection<MsilMethodEntity>();
            this._methodsView = CollectionViewSource.GetDefaultView(this.Methods);
            this._methodsView.Filter = o => {
                var obj = (MsilMethodEntity)o;
                return obj.MethodData.FullName.Contains(this.FilterString);
            };
            this.FilterString = "";
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public int UpdateSolution_Done(int fSucceeded, int fModified, int fCancelCommand)
        {
            if (fSucceeded == 1) {
                Debug.Write("Compiled, generating IL code...");
                string assemblyFile = Helpers.GetOutputAssemblyFileName();
                var msilReader = new MsilReader(assemblyFile);

                this.Methods = new ObservableCollection<MsilMethodEntity>(msilReader.EnumerateMethods());
                this._methodsView = CollectionViewSource.GetDefaultView(this.Methods);
                this._methodsView.Filter = o => {
                    var obj = (MsilMethodEntity)o;
                    return obj.MethodData.FullName.Contains(this.FilterString);
                };

                Debug.WriteLine("Done");
            }
            return VSConstants.S_OK;
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
        #endregion
    }
}
