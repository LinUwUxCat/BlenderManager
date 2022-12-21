using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Logic;
using BlenderManager.ViewModels;
namespace BlenderManager{

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }
    }
    
    public class MainWindowViewModel : INotifyPropertyChanged{
        public event PropertyChangedEventHandler? PropertyChanged;
        public string TitleText{
            get => "Blender Manager";
        }
        public string ButtonText{
            get => "Show Versions";
            set{
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ButtonText)));
            }
        }
        
       
        LogicSys l = new LogicSys();
        List<string> _versionList = new();
        string installDir = "";
        string versionListText = "";
        public string InstallDir{
            get=>l.installFolder==null?"":l.installFolder;
            set{
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(InstallDir)));
            }
        }
        public string VersionList{
            get=>versionListText;
            set{
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VersionList)));
            }
        }

        public ObservableCollection<VersionViewModel> Versions {get;} = new();

        public List<Version> VersionsInstalled{
            get=>l.Versions;
            set=>PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VersionsInstalled)));
        }

        public List<string> VersionsWebsite{
            get=>l.GetVersionListFromWeb();
            set=>PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VersionsWebsite)));
        }
        public List<string> _listWithVersion=new();
        public string? WebVersionSelected{
            set{
                if (value!=null)_listWithVersion = l.GetListFromVersion(value);
                else _listWithVersion = new();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ListWithVersion)));
            }
        }

        public List<string> ListWithVersion{
            get=>_listWithVersion;
            set=>PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ListWithVersion)));
        }
        public void AddVersion(){
            //TODO : tomorrow
        }

        public void ReloadVersionsFromWebsite(){
            _versionList = l.GetVersionListFromWeb();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VersionsWebsite)));
        }
        public void ButtonClicked() {
            l.installFolder = "/home/linuxcat/blender";
            var v = l.Versions;
            v.Sort((a,b) => a.versionString.CompareTo(b.versionString));
            versionListText ="";
            for(int i=Versions.Count-1; i>=0; i--){
                Versions.Remove(Versions[i]);
            }
            if (v==null) versionListText = ""; 
            else {
                foreach(Version l in v){
                    versionListText+=l.versionString+"\n";
                    Versions.Add(new VersionViewModel(l));
                }
            }
            _versionList = l.GetVersionListFromWeb();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VersionsInstalled)));
            VersionList = versionListText;
        }
    }
}