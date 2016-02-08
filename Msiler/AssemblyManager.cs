using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Msiler.AssemblyParser;
using Msiler.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace Msiler
{
    public delegate void MethodListChangedHandler(object sender, MethodsListEventArgs e);

    public class MethodsListEventArgs : EventArgs
    {
        public List<AssemblyMethod> Methods { get; private set; }

        public MethodsListEventArgs(List<AssemblyMethod> methods) {
            this.Methods = methods;
        }
    }

    public class AssemblyManager : IVsUpdateSolutionEvents, IVsSolutionEvents
    {
        public IEnumerable<AssemblyMethod> Methods { get; private set; }

        AssemblyReader _assemblyReader;
        DateTime _previousAssemblyWriteTime;

        public AssemblyManager() {
            uint solutionUpdateCookie;
            uint solutionCookie;

            Common.Instance.BuildManager.AdviseUpdateSolutionEvents(this, out solutionUpdateCookie);
            Common.Instance.Solution.AdviseSolutionEvents(this, out solutionCookie);

            Common.Instance.SolutionUpdateCookie = solutionUpdateCookie;
            Common.Instance.SolutionCookie = solutionCookie;

            Common.Instance.ListingGenerationOptions.Applied += (s, e) => {
                _previousAssemblyWriteTime = default(DateTime);
            };
        }

        public int OnAfterCloseSolution(object pUnkReserved) {
            OnMethodListChanged(new List<AssemblyMethod>());
            this._previousAssemblyWriteTime = default(DateTime);
            return VSConstants.S_OK;
        }

        public int UpdateSolution_Begin(ref int pfCancelUpdate) {
            if (this._assemblyReader != null) {
                this._assemblyReader.Dispose();
            }
            return VSConstants.S_OK;
        }

        public int UpdateSolution_Done(int fSucceeded, int fModified, int fCancelCommand) {
            if (Common.Instance.GeneralOptions.UpdateListingOnlyIfVisible && !MsilerToolWindow.IsVisible) {
                return VSConstants.S_OK;
            }
            if (fSucceeded != 1) {
                return VSConstants.S_OK;
            }
            string assemblyFile = DTEHelpers.GetOutputAssemblyFileName();
            try {
                var assemblyWriteTime = new FileInfo(assemblyFile).LastWriteTime;
                // if assembly was not changed
                if (_previousAssemblyWriteTime == assemblyWriteTime) {
                    return VSConstants.S_OK;
                }
                var options = new AssemblyParserOptions {
                    ProcessPDB = Common.Instance.ListingGenerationOptions.ProcessPDBFiles
                };
                this._assemblyReader = new AssemblyReader(assemblyFile, options);
                _previousAssemblyWriteTime = assemblyWriteTime;
                var genOpt = Common.Instance.GeneralOptions;
                OnMethodListChanged(this._assemblyReader.Methods);
            } catch (Exception) {
                OnMethodListChanged(new List<AssemblyMethod>());
            }
            return VSConstants.S_OK;
        }

        #region Unused handlers
        public int UpdateSolution_StartUpdate(ref int pfCancelUpdate)
            => VSConstants.S_OK;
        public int UpdateSolution_Cancel()
            => VSConstants.S_OK;
        public int OnActiveProjectCfgChange(IVsHierarchy pIVsHierarchy)
            => VSConstants.S_OK;
        public int OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
            => VSConstants.S_OK;
        public int OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
            => VSConstants.S_OK;
        public int OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
            => VSConstants.S_OK;
        public int OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
            => VSConstants.S_OK;
        public int OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
            => VSConstants.S_OK;
        public int OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
            => VSConstants.S_OK;
        public int OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
            => VSConstants.S_OK;
        public int OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
            => VSConstants.S_OK;
        public int OnBeforeCloseSolution(object pUnkReserved)
            => VSConstants.S_OK;
        #endregion

        public event MethodListChangedHandler MethodListChanged;

        public void OnMethodListChanged(List<AssemblyMethod> methodList) {
            MethodListChanged?.Invoke(this, new MethodsListEventArgs(methodList));
        }
    }
}
