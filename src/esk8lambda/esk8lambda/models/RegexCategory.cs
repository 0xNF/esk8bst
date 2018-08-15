using System.Text.RegularExpressions;

namespace esk8lambda.models {
    class RegexCategory<T> {
        public readonly Regex Regex;
        public readonly T Tag;

        public RegexCategory(Regex pattern, T tag) {
            this.Regex = pattern;
            this.Tag = tag;
        }
    }

}
