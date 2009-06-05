//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.IO;
//using System.Diagnostics;
//using CodeRun.Util;

//namespace CodeRun.Utils.CommandLine
//{
//  public class CommandLine
//  {
//    public CommandLine()
//    {
//      Log = Logger.Console;// System.Console.Out;
//      UseSystemCurrentDirectory = true;
//    }
//    public CommandLine(string virtualCurrentDirectory)
//    {
//      Log = Logger.Console;
//      UseSystemCurrentDirectory = false;
//      Cd(virtualCurrentDirectory);
//    }

//    public ILogger Log{get;set;}

//    public string MapPath(string path)
//    {
//      return MapPath(CurrentDirectory, path);
//    }

//    public static string MapPath(string currentDir, string path)
//    {
//      if (currentDir.IsNullOrEmpty())
//        currentDir = Directory.GetCurrentDirectory();
//      if (path.IsNullOrEmpty())
//        return currentDir;
//      path = Environment.ExpandEnvironmentVariables(path);
//      if (Path.IsPathRooted(path))
//        return path;
//      else
//        return Path.GetFullPath(Path.Combine(currentDir, path));
//    }

//    public void Cd(string dir)
//    {
//      CurrentDirectory = MapPath(dir);
//    }

//    public bool UseSystemCurrentDirectory { get; set; }

//    string _CurrentDirectory;
//    public string CurrentDirectory
//    {
//      get
//      {
//        if (UseSystemCurrentDirectory || _CurrentDirectory.IsNullOrEmpty())
//        {
//          return Directory.GetCurrentDirectory();
//        }
//        return _CurrentDirectory;
//      }
//      set
//      {
//        if (UseSystemCurrentDirectory)
//        {
//          Directory.SetCurrentDirectory(value);
//        }
//        _CurrentDirectory = MapPath(value);
//      }
//    }

//    static char[] WildcardChars = new char[] { '*', '?' };

//    public string[] DirAbsolute(string drivePathFileMask)
//    {
//      var mask = new FileMask(drivePathFileMask);
//      var dir = MapPath(mask.Directory);
//      if (!Directory.Exists(dir))
//        return new string[0];
//      var files = Directory.GetFiles(dir, mask.Pattern, mask.SearchOption);
//      return files;
//    }

//    public string[] DirRelativeTo(string homeDir, string drivePathFileMask)
//    {
//      var mask = new FileMask(Combine(homeDir, drivePathFileMask));
//      if (!Directory.Exists(mask.Directory))
//        return new string[0];
//      var files = Directory.GetFiles(mask.Directory, mask.Pattern, mask.SearchOption);
//      return PathHelper.CreateRelativePaths(homeDir, files);
//    }

//    public string[] Dir(params string[] drivePathFileMasks)
//    {
//      if (drivePathFileMasks != null)
//      {
//        IEnumerable<string> files = Enumerable.Empty<string>();
//        foreach (var arg in drivePathFileMasks)
//        {
//          files = files.Concat(DirRelativeTo(CurrentDirectory, arg));
//        }
//        return files.ToArray();
//      }
//      return DirRelativeTo(CurrentDirectory, null);
//    }

//    public string[] Dir(Func<string, bool> pred, bool includeSubDirs)
//    {
//      var files = Directory.GetFiles(CurrentDirectory, null, includeSubDirs ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
//      return files.Where(pred).ToArray();
//    }

//    public void Copy(string mask, string targetDir)
//    {
//      Copy(mask, targetDir, false);
//    }

//    public void Copy(string mask, IEnumerable<string> targetDirs, bool overwrite)
//    {
//      foreach (var targetDir in targetDirs)
//        Copy(mask, targetDir, overwrite);
//    }

//    public void Copy(string mask, string targetDir, bool overwrite)
//    {
//      targetDir = MapPath(targetDir);
//      if (!Directory.Exists(targetDir))
//        throw new Exception("Directory " + targetDir + " does not exist.");
//      if (VirtualMode)
//      {
//        Inform("copy {0} {1}", mask, targetDir);
//      }
//      else
//      {
//        var mask2 = new FileMask(mask);
//        var files = Dir(mask);
//        foreach (var file in files)
//        {
//          var relFile = MakeRelative(MapPath(file), mask2.Directory);
//          var targetFile = Combine(targetDir, relFile);
//          Inform("Copying {0} {1}", file, targetFile);
//          var dir = Path.GetDirectoryName(targetFile);
//          try
//          {
//            VerifyDirectoryExists(dir);
//            File.Copy(MapPath(file), MapPath(targetFile), overwrite);
//          }
//          catch (Exception e)
//          {
//            Warn("Error copying file. {0}", e);
//          }
//        }
//      }
//    }

//    public string MakeRelative(string file, string dir)
//    {
//      return new DirectoryInfo(dir ?? CurrentDirectory).CreateRelativePathTo(new FileInfo(file));
//    }

//    public string MakeRelative(string file)
//    {
//      return MakeRelative(file, CurrentDirectory);
//    }

//    public string Combine(string path1, string path2, params string[] paths)
//    {
//      var s = InternalPathCombine(path1, path2);
//      if (paths != null && paths.Length > 0)
//      {
//        foreach (var path in paths)
//        {
//          s = InternalPathCombine(s, path);
//        }
//      }
//      return s;
//    }

//    private string InternalPathCombine(string path1, string path2)
//    {
//      if (path1 == null)
//        return path2;
//      if (path2 == null)
//        return path1;
//      return Path.Combine(path1, path2);
//    }

//    public int Execute(string file, string args)
//    {
//      Inform("{0} {1}", file, args);
//      if (!VirtualMode)
//      {
//        return ProcessHelper.ExecuteAndWaitForExit(file, args, null, Execute_DataReceived, Execute_DataReceived);
//      }
//      return 0;
//    }

//    void Execute_DataReceived(object sender, DataReceivedEventArgs e)
//    {
//      if (e.Data == null)
//        return;
//      Inform(e.Data);
//    }


//    public void Inform(string format, params object[] args)
//    {
//      //Trace.WriteLine(String.Format(format, args));
//      Log.Inform(format, args);
//    }

//    public void Warn(string format, params object[] args)
//    {
//      //Trace.WriteLine("Warning: "+String.Format(format, args));
//      Log.Warn(format, args);
//    }

//    public void VerifyDirectoryExists(string dir)
//    {
//      dir = MapPath(dir);
//      if (VirtualMode)
//      {
//        Inform("VerifyDirectoryExists {0}", dir);
//      }
//      else
//      {
//        if (!Directory.Exists(dir))
//          Directory.CreateDirectory(dir);
//      }
//    }

//    public bool VirtualMode { get; set; }

//    public void CopyFileToDirectory(string sourceFile, string dir)
//    {
//      var targetFile = Combine(dir, Path.GetFileName(sourceFile));
//      CopyFileToFile(sourceFile, targetFile);
//    }

//    public void CopyFileToFile(string sourceFile, string targetFile, bool overwrite)
//    {
//      sourceFile = MapPath(sourceFile);
//      targetFile = MapPath(targetFile);
//      File.Copy(sourceFile, targetFile, overwrite);
//    }
//    public void CopyFileToFile(string sourceFile, string targetFile)
//    {
//      CopyFileToFile(sourceFile, targetFile, false);
//    }


//    public string ReadAllText(string file)
//    {
//      return File.ReadAllText(MapPath(file));
//    }

//    public bool FileExists(string file)
//    {
//      return File.Exists(MapPath(file));
//    }

//  }

//  struct FileMask
//  {
//    static char[] WildcardChars = new char[] { '*', '?' };

//    bool NoDir;
//    public FileMask(string s)
//    {
//      if (s.IsNullOrEmpty())
//      {
//        Directory = String.Empty;// System.IO.Directory.GetCurrentDirectory();
//        Pattern = "*";//
//        NoDir = true;
//      }
//      else
//      {
//        var lio = s.LastIndexOf('\\');
//        if (lio > -1)
//        {
//          if (s.IndexOfAny(WildcardChars) >= 0)
//          {
//            var pair = s.SplitAt(lio, true);
//            Directory = pair.First;
//            Pattern = pair.Second;
//          }
//          else
//          {
//            Directory = s;
//            Pattern = "*";
//          }
//          NoDir = false;
//        }
//        else
//        {
//          Directory = String.Empty;
//          Pattern = s;
//          NoDir = true;
//        }
//      }
//      if (Pattern.StartsWith("**"))
//      {
//        Pattern = Pattern.Substring(1);
//        SearchOption = SearchOption.AllDirectories;
//      }
//      else
//        SearchOption = SearchOption.TopDirectoryOnly;
//    }

//    public SearchOption SearchOption;
//    public string Directory;
//    public string Pattern;

//    public override string ToString()
//    {
//      if (NoDir || Directory == null)
//        return Pattern ?? String.Empty;
//      if (Pattern == null)
//        return Directory ?? String.Empty;
//      return Path.Combine(Directory, Pattern);
//    }
//  }

//}
