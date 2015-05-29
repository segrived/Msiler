using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace Quart.Msiler
{
    [Guid("0c127690-de92-4d02-a743-634bb922145c")]
    public class MyToolWindow : ToolWindowPane
    {
        public MyToolWindow() : base(null)
        {
            this.Caption = Resources.ToolWindowTitle;
            this.BitmapResourceID = 301;
            this.BitmapIndex = 1;
            base.Content = new MyControl();
        }
    }
}
