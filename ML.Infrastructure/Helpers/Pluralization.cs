using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Pluralize.NET.Core;

namespace ML.Infrastructure.Helpers
{
    public static class Pluralization
    {
        private static Pluralizer pluralizer = null;

        public static string Pluralize(string noun)
        {
            return Pluralizer.Pluralize(noun);
        }

        private static Pluralizer Pluralizer
        {
            get
            {
                if (pluralizer == null)
                    pluralizer = new Pluralizer();
                return pluralizer;
            }
        }
    }
}
