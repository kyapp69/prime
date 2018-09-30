﻿using System.Collections.Generic;

namespace Prime.Core
{
    public class ConsoleEntries : List<ConsoleEntry>
    {
        public void Add(int width, string text)
        {
            Add(new ConsoleEntry(width, text));
        }

        public void Add(string text)
        {
            Add(new ConsoleEntry(text));
        }
    }
}