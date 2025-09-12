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

namespace MANIFOLD.Editor {
    public static class VTools {
        public class ExecutionInfo {
            public string command;
            public IReadOnlyList<string> arguments;
            public CancellationToken token = CancellationToken.None;
            
            public bool showDialog = true;
            public bool autoCloseDialog;
        }
        
        public class ExecutionResult {
            public List<string> logs = new List<string>();
            public List<string> errors = new List<string>();
            
            public bool HadErrors => errors.Count > 0;
        }
        
        public const string GITHUB_LINK = "https://github.com/dotMANIFOLD/sbox-vtools";
        public const string DOWNLOAD_LINK = GITHUB_LINK + "/releases/latest/download/vtools.zip";
        
        public const string VTOOLS_FOLDER = "vtools/";
        public const string EXECUTABLE = "vtools.exe";
        public const string SETTINGS = "settings.json";

        public const string WORK_FOLDER = "vtools_work/";

        public static string ExecutablePath => FileSystem.ProjectTemporary.GetFullPath(VTOOLS_FOLDER + EXECUTABLE);
        public static string VToolsFolder => FileSystem.ProjectTemporary.GetFullPath(VTOOLS_FOLDER);
        public static string WorkFolder => FileSystem.ProjectTemporary.GetFullPath(WORK_FOLDER);

        public static async ValueTask<ExecutionResult> Execute(ExecutionInfo info) {
            if (!ExecutableExists()) {
                bool askResult = await ShowConfirmation($"VTools are missing. Do you want to download VTools?\nVTools is open source and hosted on GitHub.\n{GITHUB_LINK}\n\nIt will be downloaded from\n{DOWNLOAD_LINK}");
                if (!askResult) {
                    Log.Info("VTools: Download declined. Cancelling command.");
                    return null;
                }
                
                bool downloadResult = await DownloadFiles();
                if (!downloadResult) {
                    Log.Info("VTools: Download cancelled.");
                    return null;
                }
            }
            
            if (!SettingsExists()) {
                await RunInit();
            }
            var realArgs = new List<string>(info.arguments.Count + 3);
            realArgs.Add(info.command);
            realArgs.AddRange(info.arguments);
            realArgs.Add("--work-folder");
            realArgs.Add(WorkFolder);
            
            // using var handle = Progress.Start($"VTools: {command}");
            return await ExecuteInternal(realArgs, info);
        }
        
        private static async ValueTask<ExecutionResult> RunInit() {
            FileSystem.ProjectTemporary.CreateDirectory(WORK_FOLDER);
            
            // using var handle = Progress.Start("Initialize VTools");
            return await ExecuteInternal([ "init", Project.Current.GetAssetsPath(), "-f", FileSystem.ProjectTemporary.GetFullPath(WORK_FOLDER) ], new ExecutionInfo() {
                autoCloseDialog = true
            });
        }
        
        private static async ValueTask<ExecutionResult> ExecuteInternal(IEnumerable<string> args, ExecutionInfo execInfo) {
            ProcessStartInfo procInfo = new ProcessStartInfo(ExecutablePath, args);
            procInfo.CreateNoWindow = true;
            procInfo.RedirectStandardOutput = true;
            procInfo.RedirectStandardError = true;

            using var process = new Process();
            process.StartInfo = procInfo;

            ExecutionResult execResult = new ExecutionResult();
            process.OutputDataReceived += (_, evt) => {
                execResult.logs.Add(evt.Data);
            };
            process.ErrorDataReceived += (_, evt) => {
                if (string.IsNullOrWhiteSpace(evt.Data)) return;
                execResult.errors.Add(evt.Data);
            };
            
            ProcessDialog dialog = null;
            if (execInfo.showDialog) {
                dialog = new ProcessDialog();
                dialog.onCancel = () => {
                    if (!process.HasExited) {
                        process.Kill();
                    }
                    dialog.Failed();
                };
                process.OutputDataReceived += (_, evt) => {
                    dialog.AddInfo(evt.Data);
                };
                process.ErrorDataReceived += (_, evt) => {
                    dialog.AddError(evt.Data);
                };
                dialog.Show();
            }
            
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            
            if (!process.HasExited) {
                await process.WaitForExitAsync(execInfo.token);
            }
            if (dialog != null) {
                dialog.Finished();
                if (execInfo.autoCloseDialog) {
                    dialog.Close();
                }
            }

            if (execResult.HadErrors) {
                Log.Error($"VTools error occured.\nArgs: \"{string.Join(' ', args)}\"\nError: {execResult.errors[^1]}");
                dialog?.Failed();
            }

            return execResult;
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
