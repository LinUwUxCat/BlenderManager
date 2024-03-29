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
        FileSystemWatcher watcher;
        LogicSys l;
        ///<summary>
        ///Initialisation of the main window. We want to load our config file here.
        ///</summary>
        public MainWindowViewModel(Window mainWindow){
            this.mainWindow = mainWindow;
            this.mainWindow.CanResize=false;
            watcher = new FileSystemWatcher();
            watcher.NotifyFilter = NotifyFilters.Attributes
                                 | NotifyFilters.CreationTime
                                 | NotifyFilters.DirectoryName
                                 | NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.Security
                                 | NotifyFilters.FileName
                                 | NotifyFilters.Size;
            watcher.Created += refreshVersionsWhen;
            watcher.Deleted += refreshVersionsWhen;
            watcher.Renamed += refreshVersionsWhen;

            watcher.EnableRaisingEvents=true;

            l = new LogicSys();
            if(File.Exists(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "BlenderManager.conf"))){
                l.installFolder = File.ReadAllText(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "BlenderManager.conf"));
            }
            RefreshVersions();
            
        }
        private void refreshVersionsWhen(object sender, FileSystemEventArgs e){
            RefreshVersions();
        }

        public void RefreshVersions(){
            Versions.Clear();
            foreach(Version ver in l.Versions){
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

        public static ObservableCollection<VersionViewModel> Versions {get;} = new();

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
        public bool _systemDropDownEnabled;
        public bool _downloadButtonEnabled;
        public string? WebVersionSelected{
            get=>_webVersionSelected;
            set{
                SystemDropDownEnabled=value!=null;
                if (value!=null){
                    _listWithVersion = l.GetListFromVersion(value);
                } else{
                    _listWithVersion = new();
                } 
                _webVersionSelected=value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ListWithVersion)));
            }
        }
        public string? WebSystemSelected{
            get=>_webSystemSelected;
            set{
                DownloadButtonEnabled=value!=null;
                _webSystemSelected=value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WebVersionSelected)));
            }
        }
        public bool SystemDropDownEnabled{
            get=>_systemDropDownEnabled;
            set{
                _systemDropDownEnabled=value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SystemDropDownEnabled)));
            }
        }
        public bool DownloadButtonEnabled{
            get=>_downloadButtonEnabled;
            set{
                _downloadButtonEnabled=value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DownloadButtonEnabled)));
            }
        }

        public List<string> ListWithVersion{
            get=>_listWithVersion;
            set=>PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ListWithVersion)));
        }

        public void ReloadVersionsFromWebsite(){
            DownloadButtonEnabled=false;
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
            watcher.Path=dir;
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
    }
}
