using System.Windows.Controls;

namespace Quart.Msiler
{
    public partial class MyControl : UserControl
    {
        private MyControlVM _viewModel;

        public MyControl()
        {
            InitializeComponent();
            this._viewModel = new MyControlVM();
            this.DataContext = this._viewModel;
        }

    }
}