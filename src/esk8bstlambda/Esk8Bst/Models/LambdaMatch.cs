using System.Collections.Generic;

namespace Esk8Bst.Models {
    /// <summary>
    /// A match object for this Lambda
    /// Contains a Matching criteria, plus a list of Posts that matched it
    /// </summary>
    public class LambdaMatch {
        public Match FbMatch { get; }
        public List<BSTComment> Posts { get; } = new List<BSTComment>();

        public LambdaMatch(Match m) {
            this.FbMatch = m;
        }
    }
}
