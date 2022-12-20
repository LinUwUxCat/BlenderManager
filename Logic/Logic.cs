using System.IO;
using System.Collections.Generic;
using System;
using System.Linq;
namespace Logic;

public class Version{
    public string versionString;
    public string path;
    public string system;

    public Version(string versionString, string system, string path){
        this.versionString = versionString;
        this.system = system;
        this.path = path;
    }
}

class LogicSys{
    
    string? folder=null;
    public string? installFolder{
        get=>folder;
        set{
            if (!Directory.Exists(value)){
                folder=null;
            } else folder=value;
        }
    }

    /// <summary>
    /// Lists all versions of blender installed into installFolder.
    /// </summary>
    public List<Version> Versions{
        get{
            if (installFolder==null)return new List<Version>(); //Throw something here maybe?
            var r=new List<Version>();
            DirectoryInfo dir = new DirectoryInfo(installFolder);
            foreach(DirectoryInfo d in dir.GetDirectories()){
                var l = d.Name.Split('-');
                //Blender naming scheme follows:
                //blender-M.mp-operatingsystem-architecture for blender up to 2.83, where M is major version (2), m is minor version (04 to 83), and p is patch (either none, a, b, c, or rcX)
                //blender-M.m.p-operatingsystem-architecture from blender 2.90, where M is major version (2 or 3), m is minor version (90 to 93 or single digit), and p is patch (single digit)
                //note that the naming scheme is so different on everything (e.g. blender-2.80rc1-linux-glibc217-x86_64) that i'm not even gonna try to display anything more than the version and OS.
                //Also, versions 1.X are unsupported for now.
                var vs = "0.0.0";
                var system = "unknown";
                var path = d.FullName;
                if (l[0].ToLower() == "blender" && l.Length>1 && isABlenderFolder(d, l[1])){
                    vs = l[1];
                    if (l.Length == 2){
                        r.Add(new Version(vs, system, path));
                    } else {
                        system = l.Length>3&&l[2]=="release"?l[3]:l[2]; //skip "release"
                        r.Add(new Version(vs, system, path));
                    }
                }
            }
            return r;

        }
    }

    /// <summary>
    /// I'd rather not explain this function.
    /// </summary>
    private bool isABlenderFolder(DirectoryInfo d, string version){
        return Directory.GetFiles(d.FullName).Contains(d.FullName + "/blender") && Array.Exists(Directory.GetDirectories(d.FullName), x=>version.StartsWith(x.Split(Path.DirectorySeparatorChar)[x.Split(Path.DirectorySeparatorChar).Length-1]));
    }
}
