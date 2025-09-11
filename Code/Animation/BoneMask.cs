using System.Collections.Generic;
using Sandbox;

namespace MANIFOLD.Animation {
	[AssetType(Name = "Bone Mask", Category = ModuleData.CATEGORY, Extension = EXTENSION)]
	public class BoneMask : GameResource {
		public const string EXTENSION = ModuleData.EXT_PREFIX + "mask";
		
		/// <summary>
		/// Model for the skeleton of this mask. Can be null.
		/// </summary>
		public Model Model { get; set; }
		public Dictionary<string, float> Weights { get; set; }

		protected override Bitmap CreateAssetTypeIcon(int width, int height) {
			return CreateSimpleAssetTypeIcon("hdr_strong", width, height, ModuleData.BG_COLOR);
		}
	}
}
