using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tollrech.Tests.Test.Data.LogIntroducerTests
{
    public class Simple
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Simple));

        public void Some()
        {
            "i want to log this"{caret}
        }
    }
}
