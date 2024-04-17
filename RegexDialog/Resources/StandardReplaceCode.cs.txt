            // To make a replace
            string replace = regex.Replace(input, replacement);
            Console.WriteLine(replace);

            //To get all matches
            MatchCollection matches = regex.Matches(input);
            Console.WriteLine(string.Join("\r\n", matches.Cast<Match>().Select(match => match.Value)));