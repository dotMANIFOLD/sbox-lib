using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Editor;
using MANIFOLD.AnimGraph;
using MANIFOLD.Editor;
using Sandbox;
using Application = Editor.Application;

namespace MANIFOLD.Animation {
    public static class ModelAnimExtract {
        [Event("asset.contextmenu")]
        private static void OnModelAssetContext(AssetContextMenu evt) {
            var modelAssets = evt.SelectedList.Where(x => x.AssetType == AssetType.Model).ToArray();
            if (!modelAssets.Any()) return;

            var subMenu = evt.Menu.FindOrCreateMenu("Animation");
            subMenu.Icon = "animation";

            if (modelAssets.Length == 1) {
                subMenu.AddOption("Extract single animation", "unarchive", () => {
                    ShowSingleAnimPrompt(modelAssets[0].Asset);
                });
                subMenu.AddOption("Extract single bone mask", "unarchive", () => {
                    ShowBoneMaskPrompt(modelAssets[0].Asset);
                });
            }
            subMenu.AddOption("Extract all animations", "unarchive", () => {
                ExtractManyAnimations(modelAssets);
            });
            subMenu.AddOption("Create graph resources", "list_alt", () => {
                ExtractGraphResources(modelAssets[0].Asset);
            });
            evt.Menu.AddSeparator();
        }

        public static void ShowSingleAnimPrompt(Asset asset) {
            ShowSelectPrompt("Extract single animation", "Select an animation to extract", asset.LoadResource<Model>().AnimationNames, (result) => {
                ExtractSingleAnimation(asset, result);
            });
        }

        public static async Task ShowBoneMaskPrompt(Asset asset) {
            var execResult = await VTools.Execute(new VTools.ExecutionInfo() {
                command = "model list mask",
                arguments = [ asset.RelativePath, "--format", "json" ],
                autoCloseDialog = true
            });
            
            var allMasks = Json.Deserialize<string[]>(execResult.logs[^2]);
            ShowSelectPrompt("Extract single bone mask", "Select a bone mask to extract", allMasks, (result) => {
                ExtractSingleBoneMask(asset, result);
            });
        }

        public static async Task ExtractSingleBoneMask(Asset asset, string mask) {
            await VTools.Execute(new VTools.ExecutionInfo() {
                command = "model extract mask",
                arguments = [asset.RelativePath, mask, "--ext", BoneMask.EXTENSION]
            });
        }
        
        public static async Task ExtractSingleAnimation(Asset asset, string animation) {
            await VTools.Execute(new VTools.ExecutionInfo() {
                command = "model extract anim",
                arguments = [ asset.RelativePath, animation, "--ext", AnimationClip.EXTENSION ]
            });
        }

        public static async Task ExtractGraphResources(Asset asset) {
            await VTools.Execute(new VTools.ExecutionInfo() {
                command = "model extract resources",
                arguments = [ asset.RelativePath, "--ext", AnimGraphResources.EXTENSION ],
            });
        }
        
        public static async Task ExtractManyAnimations(IEnumerable<AssetEntry> list) {
            foreach (var entry in list) {
                await ExtractAnimations(entry.Asset);
            }
        }
        
        public static async Task ExtractAnimations(Asset asset) {
            await VTools.Execute(new VTools.ExecutionInfo() {
                command = "anim extract",
                arguments = [ asset.GetCompiledFile(false) ]
            });
        }

        private static void ShowSelectPrompt(string name, string message, IEnumerable<string> entries, Action<string> onConfirm) {
            var dialog = new Dialog();
            dialog.Window.Title = name;
            // dialog.Window.SetWindowIcon("animation");
            dialog.Window.Size = new Vector2(400, 140);
            
            dialog.Layout = Layout.Column();
            dialog.Layout.Margin = 16;
            dialog.Layout.Spacing = 4;

            var label = dialog.Layout.Add(new Label(message));
            label.SetSizeMode(SizeMode.Default, SizeMode.Expand);
            
            var comboBox = dialog.Layout.Add(new ComboBox());
            foreach (var entry in entries) {
                comboBox.AddItem(entry);
            }

            var row = dialog.Layout.AddRow();
            row.Spacing = 4;
            row.AddStretchCell();
            row.Add(new Button.Primary("Extract") { Clicked = () => {
                    onConfirm?.Invoke(comboBox.CurrentText);
                    dialog.Close();
                }
            });
            row.Add(new Button("Cancel") { Clicked = () => dialog.Close() });
            
            dialog.Show();
        }
    }
}
