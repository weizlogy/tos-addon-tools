using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using wzipfrenamer.Properties;

namespace wzipfrenamer {
  /// <summary>
  /// IPFファイルの生成を監視して特定のフォルダに移動させる
  /// フォルダ構成の制限あり
  /// 
  /// basedir -- 監視対象
  ///   └ addonName
  ///       ├ addon_d.ipf
  ///       │   ├ xxx.lua
  ///       │   └ xxx.xml
  ///       └ xxx.ipf  -- ipftool等で生成
  ///   ...
  ///   └ addonName
  /// </summary>
  class Program {
    static void Main(string[] args) {
      Console.OutputEncoding = Encoding.UTF8;
      // パラメータチェック
      if (String.IsNullOrEmpty(Settings.Default.AddonFolder)) {
        Console.WriteLine("ERROR: Open wzipfrenamer.exe.config and setting AddonFolder value.");
        return;
      }
      if (String.IsNullOrEmpty(Settings.Default.UnicodeChar)) {
        Console.WriteLine("ERROR: Open wzipfrenamer.exe.config and setting UnicodeChar value.");
        return;
      }
      if (args.Length < 1) {
        Console.WriteLine("Usage: wzipfrenamer.exe watchDir");
        return;
      }

      var watchDir = args[0];
      Console.WriteLine("Settings: ");
      Console.WriteLine("  AddonFolder = " + Settings.Default.AddonFolder);
      Console.WriteLine("  UnicodeChar = " + BitConverter.ToString(Encoding.UTF8.GetBytes(Settings.Default.UnicodeChar)));
      Console.WriteLine("  WaitSecT... = " + Settings.Default.WaitSecToFileWritten + " (sec)");
      Console.WriteLine("Parametter: ");
      Console.WriteLine("     WatchDir = " + watchDir);
      Console.WriteLine("Watch start. Press any key and enter to quit. -> " + watchDir);

      try {
        // 監視開始
        var myWatcher = new IpfFileWatcher().Run(watchDir, (sender, ev) => {
          var target = ev.FullPath;
          Console.WriteLine(String.Format("{0} is {1}", target, ev.ChangeType));
          var renamer = new IpfFileRenamer(target);
          var renamed = renamer.Rename();
          if (!renamed) {
            return;
          }
          var copied = renamer.Copy();
          if (!copied) {
            return;
          }
        });

        Console.ReadLine();

        // 監視終了
        myWatcher.Stop();

      } catch (Exception ex) {
        Console.WriteLine(ex);
      }

      Console.WriteLine("Successfully finished.");
    }

    /// <summary>
    /// IPFファイルの生成監視
    /// </summary>
    class IpfFileWatcher {
      FileSystemWatcher watcher;

      public IpfFileWatcher() {
        this.watcher = new FileSystemWatcher();
      }

      [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
      public IpfFileWatcher Run(string pWatchDir, Action<object, FileSystemEventArgs> pCreateAction) {
        this.watcher.Path = pWatchDir;
        this.watcher.Filter = "*.ipf";
        this.watcher.IncludeSubdirectories = true;
        this.watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime;
        this.watcher.Created += new FileSystemEventHandler(pCreateAction);
        this.watcher.EnableRaisingEvents = true;
        return this;
      }

      public IpfFileWatcher Stop() {
        this.watcher.EnableRaisingEvents = false;
        return this;
      }
    }

    /// <summary>
    /// IPFファイルのリネームとコピー
    /// </summary>
    class IpfFileRenamer {
      string target;

      protected string AddonName {
        get {
          return Path.GetDirectoryName(this.target).Split(Path.DirectorySeparatorChar).Last();
        }
      }

      public IpfFileRenamer(string pTargetFullPath) {
        this.target = pTargetFullPath;
      }

      public bool Rename() {
        // 最大N秒書き込み待ち デフォルト3
        var duration = 3;
        try {
          duration = Convert.ToInt32(Settings.Default.WaitSecToFileWritten);
        } catch { }

        for (var attempt = 0; attempt < duration; attempt++) {
          try {
            using (var stream = new StreamReader(this.target)) {
              break;
            }
          } catch (Exception) {
            System.Threading.Thread.Sleep(1000);
          }
        }

        // 上書き更新
        var dest = String.Format("{0}/{1}{2}.ipf", Path.GetDirectoryName(this.target), Settings.Default.UnicodeChar, this.AddonName);
        try {
          if (File.Exists(dest)) {
            File.Delete(dest);
          }
          File.Move(this.target, dest);
        } catch (Exception ex) {
          Console.WriteLine("ERROR: Unable to rename ipf.");
          Console.WriteLine(ex);
          return false;
        }

        this.target = dest;

        Console.WriteLine("Successfully renamed.");

        return true;
      }

      public bool Copy() {
        var dest = String.Format("{0}/{1}{2}.ipf", Settings.Default.AddonFolder, Settings.Default.UnicodeChar, this.AddonName);

        try {
          File.Copy(this.target, dest, true);
        } catch (Exception ex) {
          Console.WriteLine("ERROR: Unable to copy ipf.");
          Console.WriteLine(ex);
          return false;
        }

        Console.WriteLine("Successfully copied.");

        return true;
      }
    }
  }
}
