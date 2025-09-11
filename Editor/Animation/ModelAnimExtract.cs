using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Editor;
using Sandbox;

namespace MANIFOLD.Animation {
    public class ModelAnimExtract {
        [Event("asset.contextmenu")]
        private static void OnModelAssetContext(AssetContextMenu evt) {
            if (evt.SelectedList.Any(x => x.AssetType != AssetType.Model)) return;

            var subMenu = evt.Menu.FindOrCreateMenu("Animation");
            subMenu.Icon = "animation";
            
            subMenu.AddOption("Extract single animation", "animation");
            subMenu.AddOption("Create graph resources", "list_alt");
            subMenu.AddOption("Extract animations", "directions_run", () => {
                ExtractManyAnimations(evt.SelectedList);
            });
            evt.Menu.AddSeparator();
        }

        public static async Task ExtractManyAnimations(IEnumerable<AssetEntry> list) {
            foreach (var entry in list) {
                await ExtractAnimations(entry.Asset);
            }
        }
        
        public static async Task ExtractAnimations(Asset asset) {
            await VTools.Execute("anim extract", asset.GetCompiledFile());
        }
    }
}
