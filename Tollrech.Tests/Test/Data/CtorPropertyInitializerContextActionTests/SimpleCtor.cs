using System;

namespace Tollrech.Tests.Test.Data.CtorPropertyInitializerContextActionTests
{
    public class SimpleCtor
    {
        public void F()
        {
            var s = new PropertyClass(){caret}
            {
                Guid = Guid.NewGuid(),
            };
        }
    }

    public class PropertyClass
    {
        public int Int { get; set; }
        public Guid Guid { get; set; }
    }
}