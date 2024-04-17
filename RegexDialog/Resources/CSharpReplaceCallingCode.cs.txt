            CSharpReplaceContainer container = new CSharpReplaceContainer();

            string fileName = "-";

            input = container.Before(input, fileName);

            int index = -1;
            // To make a replace
            string replace = regex.Replace(input, match =>
            {
                index++;
                return container.Replace(match, index, fileName, index, 0);
            });

            replace = container.After(replace, fileName);

            Console.WriteLine(replace);