using System.Linq;
using System.Text.RegularExpressions;

namespace RegexDialog
{
    internal class RegexGroupResult : RegexResult
    {
        public RegexGroupResult(Regex regex, Group group, int groupNb, string fileName = "", int selectionIndex = 0) : base(regex, group, groupNb, fileName, selectionIndex)
        {
            int i = 0;

            Children = group.Captures
                .Cast<Capture>()
                .ToList()
                .ConvertAll(delegate (Capture c)
                {
                    RegexResult result = new RegexCaptureResult(regex, c, i, fileName, selectionIndex)
                    {
                        Parent = this
                    };

                    i++;

                    return result;
                });

            IsExpanded = Config.Instance.MatchesShowLevel > 2;
        }

        public override string Name
        {
            get
            {
                string result = "";

                try
                {
                    if (RegexElement != null)
                    {
                        Group group = (Group)RegexElement;
                        result = ElementType + " " + Regex.GetGroupNames()[RegexElementNb] + (group.Success ? " [" + group.Index.ToString() + "," + group.Length.ToString() + "]: " : " - Not Found -");
                    }
                }
                catch
                { }

                return result;
            }
        }
            
        public string GroupName
        {
            get
            {
                return Regex.GetGroupNames()[RegexElementNb];
            }
        }

        public bool Success
        {
            get
            {
                if (RegexElement != null)
                {
                    Group group = (Group)RegexElement;
                    return group.Success;
                }
                else
                {
                    return false;
                }
            }
        }

        public override void RefreshExpands()
        {
            IsExpanded = Config.Instance.MatchesShowLevel > 2;
        }
    }
}
