﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tollrech.Tests.Test.Data.LogIntroducerTests
{
    public class Simple
    {
        public void Some()
        {
            Some2("I want to log this{caret:To:CamelCase}");
        }

        public void Some2(string s)
        {

        }
    }
}
