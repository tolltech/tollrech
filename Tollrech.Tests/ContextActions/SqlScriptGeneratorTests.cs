﻿using Tollrech.EFClass;

namespace Tollrech.Tests.ContextActions
{
    public class SqlScriptGeneratorTests : ContextActionBaseTests<SqlScriptGeneratorContextAction>
    {
        protected override string ExtraPath => "SqlScriptGeneratorTests";
        protected override string RelativeTestDataPath => "SqlScriptGeneratorTests";
        public override void TestAbstract(string fileName)
        {
            throw new System.NotImplementedException();
        }
    }
}