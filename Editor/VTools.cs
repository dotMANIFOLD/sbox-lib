using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Editor;
using Sandbox;
using FileSystem = Editor.FileSystem;

namespace MANIFOLD {
    public static class VTools {
        public const string GITHUB_LINK = "https://github.com/dotMANIFOLD/sbox-vtools";
        public const string DOWNLOAD_LINK = GITHUB_LINK + "/releases/latest/download/vtools.zip";
        
        public const string VTOOLS_FOLDER = "vtools/";
        public const string EXECUTABLE = "vtools.exe";
        public const string SETTINGS = "settings.json";

        public const string WORK_FOLDER = "vtools_work/";

        public static string ExecutablePath => FileSystem.ProjectTemporary.GetFullPath(VTOOLS_FOLDER + EXECUTABLE);
        public static string VToolsFolder => FileSystem.ProjectTemporary.GetFullPath(VTOOLS_FOLDER);
        public static string WorkFolder => FileSystem.ProjectTemporary.GetFullPath(WORK_FOLDER);

        public static async Task Execute(string command, params string[] args) {
            if (!ExecutableExists()) {
                bool askResult = await ShowConfirmation($"VTools are missing. Do you want to download VTools?\nVTools is open source and hosted on GitHub.\n{GITHUB_LINK}\n\nIt will be downloaded from\n{DOWNLOAD_LINK}");
                if (!askResult) {
                    Log.Info("VTools: Download declined. Cancelling command.");
                    return;
                }
                
                bool downloadResult = await DownloadFiles();
                if (!downloadResult) {
                    Log.Info("VTools: Download cancelled.");
                    return;
                }
            }
            
            if (!SettingsExists()) {
                await RunInit();
            }
            var list = new List<string>(args.Length + 3);
            list.Add(command);
            list.AddRange(args);
            list.Add("--work-folder");
            list.Add(WorkFolder);
            
            using var handle = Progress.Start($"VTools: {command}");
            await ExecuteInternal(Progress.GetCancel(), list.ToArray());
        }
        
        private static async Task RunInit() {
            FileSystem.ProjectTemporary.CreateDirectory(WORK_FOLDER);
            
            using var handle = Progress.Start("Initialize VTools");
            await ExecuteInternal(CancellationToken.None, "init", Project.Current.GetAssetsPath(), "-f", FileSystem.ProjectTemporary.GetFullPath(WORK_FOLDER));
        }
        
        private static async Task ExecuteInternal(CancellationToken token, params string[] args) {
            ProcessStartInfo info = new ProcessStartInfo(ExecutablePath, args);
            info.CreateNoWindow = true;
            info.RedirectStandardOutput = true;
            info.RedirectStandardError = true;
            
            using var process = Process.Start(info);
            if (!process.HasExited) {
                await process.WaitForExitAsync(token);
                
                if (token.IsCancellationRequested) {
                    Log.Info("VTools execution cancelled.");
                    return;
                }
            }
            
            var errorStr = await process.StandardError.ReadToEndAsync();
            if (!string.IsNullOrWhiteSpace(errorStr)) {
                Log.Error($"VTools error occured.\nArgs: \"{string.Join(' ', args)}\"\nError: {errorStr}");
            }
            
            // var str = await process.StandardOutput.ReadToEndAsync();
            // Log.Info($"VTools. Args: {string.Join(" ", args)}, Result: {str}");
        }
        
        // CHECKS
        public static bool ExecutableExists() {
            return FileSystem.ProjectTemporary.FileExists(VTOOLS_FOLDER + EXECUTABLE);
        }
        
        public static bool SettingsExists() {
            return FileSystem.ProjectTemporary.FileExists(WORK_FOLDER + SETTINGS);
        }
        
        // POPUP
        private static async ValueTask<bool> ShowConfirmation(string question) {
            var success = new TaskCompletionSource();
            var fail = new TaskCompletionSource();
            
            PopupWindow.AskConfirm(() => success.SetResult(), () => fail.SetResult(), question);
            
            await Task.WhenAny(success.Task, fail.Task);
            return success.Task.IsCompletedSuccessfully;
        }

        private static async ValueTask<bool> DownloadFiles() {
            FileSystem.ProjectTemporary.CreateDirectory(VTOOLS_FOLDER);
            
            using var handle = Progress.Start("Downloading VTools");
            var token = Progress.GetCancel();
            var bytes = await Http.RequestBytesAsync(DOWNLOAD_LINK, cancellationToken: token);

            if (token.IsCancellationRequested) {
                return false;
            }
            
            using var stream = new MemoryStream(bytes);
            using var zip = new ZipArchive(stream);
            zip.ExtractToDirectory(VToolsFolder);

            return true;
        }
    }
}
