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
            DataContext = new MainWindowViewModel();
        }
    }
    
    public class MainWindowViewModel : INotifyPropertyChanged{

        ///<summary>
        ///Initialisation of the main window. We want to load our config file here.
        ///</summary>
        public MainWindowViewModel(){
            l.installFolder = "/home/linuxcat/blender";
            var v = l.Versions;
            v.Sort((a,b) => a.versionString.CompareTo(b.versionString));
            foreach(Version l in v){
                Versions.Add(new VersionViewModel(l));
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
        string installDir = "";
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
        public string InstallDir{
            get=>l.installFolder==null?"":l.installFolder;
            set{
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(InstallDir)));
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
        ///Downloads a version from the blender releases page.
        ///This also extracts from an archive (if applicable).
        ///Should be called when clicking on a button as it depends on the version selected in the UI.
        ///</summary>
        public async Task DownloadVersion(){
            var handler = new HttpClientHandler();
            var ph=new ProgressMessageHandler(handler);
            ph.HttpReceiveProgress += (_, args) => {
                if (args.TotalBytes!=null)TotalBytes = args.TotalBytes; 
                else TotalBytes=0;
                CurrentBytes=args.BytesTransferred;
            };
            var client = new HttpClient(ph);
            var bytes = await client.GetByteArrayAsync("https://download.blender.org/release/Blender"+WebVersionSelected+"/"+WebSystemSelected);
            File.WriteAllBytes(l.installFolder + Path.DirectorySeparatorChar + "blender-"+WebSystemSelected, bytes);
            
        }
        public void ButtonClicked() {
            l.Extract("blender-2.40-windows.zip");
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