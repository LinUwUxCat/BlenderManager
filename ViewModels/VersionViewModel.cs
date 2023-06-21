using Avalonia.Controls;
using Avalonia.Media.Imaging;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using ReactiveUI;
using Logic;

namespace BlenderManager.ViewModels;
public class VersionViewModel : ReactiveObject, INotifyPropertyChanged{
    private readonly Version _version;
    private Bitmap? _icon;
    public VersionViewModel(Version version){
        _version = version;
    }
    public string _launchText = "Launch";
    public string LaunchText {
        get=>_launchText;
        private set => this.RaiseAndSetIfChanged(ref _launchText, value);
    }

    public string VersionString => _version.versionString;
    public string FolderPath{
        get{
            var l = _version.path.Split(Path.DirectorySeparatorChar);
            return l[l.Length-1];
        }
    }
    public Bitmap? Icon{
        get{
            var assetPath = Path.GetDirectoryName(System.AppDomain.CurrentDomain.BaseDirectory)+Path.DirectorySeparatorChar+"Assets"+Path.DirectorySeparatorChar;
            if(!Path.Exists(assetPath))return null;
            if (Path.Exists(assetPath+"icon-"+_version.system+".png")){
                return new(assetPath+"icon-"+_version.system+".png");
            } else return new(assetPath+"icon-unknown.png");
        }
        private set => this.RaiseAndSetIfChanged(ref _icon, value);
    }


    public void LaunchVersion(){
        var extension = _version.system == "windows"?".exe":"";
        try{
            System.Diagnostics.Process.Start(_version.path+Path.DirectorySeparatorChar+"blender"+extension);
        } catch {
            try {
               System.Diagnostics.Process.Start("chmod", "+x \"" + _version.path+Path.DirectorySeparatorChar+"blender"+extension+"\"");
               System.Diagnostics.Process.Start(_version.path+Path.DirectorySeparatorChar+"blender"+extension);
            } catch (System.Exception e) {
                System.Console.WriteLine("Could not start the process! Error : "+e);
            }
        }
    }

    public void RemoveVersion(){
        try{
            Directory.Delete(_version.path, true);
            MainWindowViewModel.Versions.Remove(this);
        } catch (System.Exception e){
            System.Console.WriteLine(e);
        }
    }

}