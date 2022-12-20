using Avalonia.Controls;
using Avalonia.Media.Imaging;
using System.ComponentModel;
using System.Collections.Generic;
using ReactiveUI;
using Logic;

namespace BlenderManager.ViewModels;
public class VersionViewModel : ViewModelBase{
    private readonly Version _version;
    private Bitmap? _icon;

    public VersionViewModel(Version version){
        _version = version;
    }
    
    public string VersionString => _version.versionString;

    public Bitmap? Icon{
        get=>_icon;
        private set => this.RaiseAndSetIfChanged(ref _icon, value);
    }
}