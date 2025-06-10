using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Editor;
using Sandbox;
using FileSystem = Editor.FileSystem;

namespace MANIFOLD {
    public static class VTools {
        public const string GITHUB_LINK = "https://github.com/dotMANIFOLD/sbox-vtools";
        public const string DOWNLOAD_LINK = "";
        
        public const string FOLDER_PATH = "vtools/";
        public const string EXECUTABLE = "vtools.exe";
        public const string SETTINGS = "settings.json";

        public const string WORK_FOLDER = "vtools_work/";

        public static string ExecutablePath => FileSystem.ProjectTemporary.GetFullPath(FOLDER_PATH + EXECUTABLE);
        public static string WorkFolder => FileSystem.ProjectTemporary.GetFullPath(WORK_FOLDER);

        public static async Task Execute(string command, params string[] args) {
            if (!ExecutableExists()) {
                bool result = await ShowConfirmation($"VTools are missing. Do you want to download VTools?\nVTools is open source and hosted on GitHub.\n{GITHUB_LINK}");
                if (result) {
                    // TODO: add download
                    Log.Warning("Download not implemented yet.");
                }
                return;
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
            return FileSystem.ProjectTemporary.FileExists(FOLDER_PATH + EXECUTABLE);
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
    }
}
