using Avalonia.Controls;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Logic;
using BlenderManager.ViewModels;
using System.Threading.Tasks;
using System.Net.Http.Handlers;
using System.Net.Http;
using System.IO;
namespace BlenderManager{

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel(this);
        }
    }
    
    public class MainWindowViewModel : INotifyPropertyChanged{
        Window mainWindow;
        ///<summary>
        ///Initialisation of the main window. We want to load our config file here.
        ///</summary>
        public MainWindowViewModel(Window mainWindow){
            this.mainWindow = mainWindow;
        }

        public void RefreshVersions(){
            var v = l.Versions;
            v.Sort((a,b) => b.versionString.CompareTo(a.versionString));
            Versions.Clear();
            foreach(Version ver in v){
                Versions.Add(new VersionViewModel(ver));
            }
        }

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
        public string InstallDirText{
            get=>l.installFolder==null?"No install folder is set!":"Your blender installation folder is "+l.installFolder;
            set{
                l.installFolder=value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(InstallDirButtonText)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(InstallDirText)));
            }
        }
        public string InstallDirButtonText{
            get=>l.installFolder==null?"Choose a folder":"Change folder";
        }
        public long? _totalBytes;
        public long _currentBytes;
        public long? TotalBytes{
            get=>_totalBytes;
            set{
                _totalBytes=value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalBytes)));
            }
        }
        public long CurrentBytes{
            get=>_currentBytes;
            set{
                _currentBytes=value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentBytes)));
            }
        }
        bool _downloading;
        public bool Downloading{
            get=>_downloading;
            set{
                _downloading=value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Downloading)));
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
        public string? _webVersionSelected;
        public string? _webSystemSelected;
        public string? WebVersionSelected{
            get=>_webVersionSelected;
            set{
                if (value!=null)_listWithVersion = l.GetListFromVersion(value);
                else _listWithVersion = new();
                _webVersionSelected=value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ListWithVersion)));
            }
        }
        public string? WebSystemSelected{
            get=>_webSystemSelected;
            set{
                _webSystemSelected=value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WebVersionSelected)));
            }
        }

        public List<string> ListWithVersion{
            get=>_listWithVersion;
            set=>PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ListWithVersion)));
        }

        public void ReloadVersionsFromWebsite(){
            _versionList = l.GetVersionListFromWeb();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VersionsWebsite)));
        }
        ///<summary>
        ///Function that allows to change the installation directory by opening a dialog.
        ///</summary>
        public async void SelectNewDir(){
            var ofd = new OpenFolderDialog();
            var dir = await ofd.ShowAsync(this.mainWindow);
            if (dir == null)return;
            InstallDirText=dir;
            RefreshVersions();
        }

        ///<summary>
        ///Downloads a version from the blender releases page.
        ///This also extracts from an archive (if applicable).
        ///Should be called when clicking on a button as it depends on the version selected in the UI.
        ///</summary>
        public async Task DownloadVersion(){
            Downloading = true;
            try{
                var handler = new HttpClientHandler();
                var ph=new ProgressMessageHandler(handler);
                ph.HttpReceiveProgress += (_, args) => {
                    if (args.TotalBytes!=null)TotalBytes = args.TotalBytes; 
                    else TotalBytes=0;
                    CurrentBytes=args.BytesTransferred;
                };
                var client = new HttpClient(ph);
                var bytes = await client.GetByteArrayAsync("https://download.blender.org/release/Blender"+WebVersionSelected+"/"+WebSystemSelected);
                var outpath=l.installFolder + Path.DirectorySeparatorChar + "blender-"+WebSystemSelected;
                File.WriteAllBytes(outpath, bytes);
                l.Extract(outpath);
                VersionsInstalled = l.Versions;
            } catch{
                Downloading = false;
            }
            Downloading=false;
            RefreshVersions();
            
        }
        public void ButtonClicked() {
            l.installFolder = "/home/linuxcat/blender";
            var v = l.Versions;
            v.Sort((a,b) => a.versionString.CompareTo(b.versionString));
            for(int i=Versions.Count-1; i>=0; i--){
                Versions.Remove(Versions[i]);
            }
            if (v!=null){
                foreach(Version l in v){
                    Versions.Add(new VersionViewModel(l));
                }
            }
            _versionList = l.GetVersionListFromWeb();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VersionsInstalled)));
        }
    }
}