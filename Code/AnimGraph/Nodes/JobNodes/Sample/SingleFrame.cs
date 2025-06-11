using System.Text.Json.Serialization;

namespace MANIFOLD.AnimGraph.Nodes {
    [Category(JobCategories.SAMPLING)]
    [ExposeToAnimGraph]
    public class SingleFrame : JobNode {
        [Animation]
        public string Animation { get; set; }
        public int Frame { get; set; }

        [JsonIgnore]
        public override string DisplayName => "Single Frame";
        [JsonIgnore]
        public override Color AccentColor => JobCategories.SAMPLING_COLOR;
        
        public override IBaseAnimJob CreateJob() {
            throw new System.NotImplementedException();
        }
    }
}
