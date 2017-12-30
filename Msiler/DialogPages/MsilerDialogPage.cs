using System;
using Microsoft.VisualStudio.Shell;

namespace Msiler.DialogPages
{
    public delegate void ApplySettingsHandler(object sender, EventArgs e);

    [System.ComponentModel.DesignerCategory("")]
    public class MsilerDialogPage : DialogPage
    {
        protected override void OnApply(PageApplyEventArgs e)
        {
            this.Applied?.Invoke(this, e);
            base.OnApply(e);
        }

        public event ApplySettingsHandler Applied;
    }
}
