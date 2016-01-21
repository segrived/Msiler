using Quart.Msiler.Annotations;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Quart.Msiler
{
    public class AboutWindowVM : INotifyPropertyChanged
    {
        public AboutWindowVM() {
            var manifest = VsixManifest.GetManifest();
            this.VersionInformation = $"Msiler v.{manifest.Version}";
        }

        private string _versionInformation;
        public string VersionInformation {
            get { return this._versionInformation; }
            set
            {
                this._versionInformation = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            var handler = PropertyChanged;
            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
