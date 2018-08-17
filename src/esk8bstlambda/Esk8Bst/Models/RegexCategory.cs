using System.Text.RegularExpressions;

namespace Esk8Bst.Models {
    public class RegexCategory<T> {
        public readonly Regex Regex;
        public readonly T Tag;

        public RegexCategory(Regex pattern, T tag) {
            this.Regex = pattern;
            this.Tag = tag;
        }
    }


}
