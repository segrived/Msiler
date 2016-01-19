using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Quart.Msiler
{
    [Guid("0c127690-de92-4d02-a743-634bb922145c")]
    public sealed class MsilerToolWindow : ToolWindowPane, IVsWindowFrameNotify
    {
        public static bool IsVisible { get; private set; }

        public MsilerToolWindow() : base(null) {
            this.Caption = Resources.ToolWindowTitle;
            this.BitmapResourceID = 301;
            this.BitmapIndex = 1;
            Content = new MyControl();
            IsVisible = false;
        }

        public int OnShow(int fShow) {
            if (fShow == (int)__FRAMESHOW.FRAMESHOW_WinShown) {
                IsVisible = true;
            } else if (fShow == (int)__FRAMESHOW.FRAMESHOW_WinHidden) {
                IsVisible = false;
            }

            return VSConstants.S_OK;
        }

        public int OnMove() {
            return VSConstants.S_OK;
        }

        public int OnSize() {
            return VSConstants.S_OK;
        }

        public int OnDockableChange(int fDockable) {
            return VSConstants.S_OK;
        }
    }
}