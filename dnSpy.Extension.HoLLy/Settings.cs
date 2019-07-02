using System.ComponentModel.Composition;
using dnSpy.Contracts.MVVM;

namespace HoLLy.dnSpy.Extension
{
    [Export(typeof(Settings))]
    internal class Settings : ViewModelBase
    {
        public StorageLocation SourceMapStorageLocation
        {
            get => sourceMapStorageLocation;
            set {
                if (sourceMapStorageLocation == value) return;
                sourceMapStorageLocation = value;
                OnPropertyChanged(nameof(SourceMapStorageLocation));
            }
        }

        private StorageLocation sourceMapStorageLocation = StorageLocation.AssemblyLocation;
    }

    public enum StorageLocation
    {
        None,
        AssemblyLocation,
        // AppData,
    }
}
