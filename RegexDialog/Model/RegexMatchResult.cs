using System.Linq;
using System.Text.RegularExpressions;

namespace RegexDialog
{
    internal class RegexMatchResult : RegexResult
    {
        public RegexMatchResult(Regex regex, Match match, int matchNb, string fileName = "", int selectionIndex = 0) : base(regex, match, matchNb, fileName, selectionIndex)
        {
            int i = 0;

            Children = match.Groups
                .Cast<Group>()
                .ToList()
                .ConvertAll(group =>
                {
                    RegexResult result = new RegexGroupResult(regex, group, i, fileName, selectionIndex)
                    {
                        Parent = this
                    };

                    i++;

                    return result;
                });

            if (Children.Count > 0)
                Children.RemoveAt(0);

            IsExpanded = Config.Instance.MatchesShowLevel > 1;
        }

        public override void RefreshExpands()
        {
            if (Config.Instance.MatchesShowLevel > 1)
            {
                Children.ForEach(child => child.RefreshExpands());

                IsExpanded = true;
            }
            else
            {
                IsExpanded = false;
            }
        }
    }
}
