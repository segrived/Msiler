using System;
using Microsoft.VisualStudio.Shell;

namespace Msiler.DialogPages
{
    public delegate void ApplySettingsHandler(object sender, EventArgs e);

    [System.ComponentModel.DesignerCategory("")]
    public class MsilerDialogPage : DialogPage
    {
        protected override void OnApply(PageApplyEventArgs e) {
            OnApplied(e);
            base.OnApply(e);
        }

        public event ApplySettingsHandler Applied;

        protected virtual void OnApplied(EventArgs e) {
            if (Applied != null)
                Applied(this, e);
        }
    }
}
