namespace Tollrech.Tests.Test.Data.MockedClassCreateFixTests
{
    public class Simple : Base
    {
      public void Method()
      {
        new MockingClass({caret});
      }
    }

    public abstract class Base
    {
        protected void NewMock<T>() { }
    }

    public class MockingClass
    {
        public MockingClass(IMockedInterface1 m1, IMockedInterface2 m2) { }
    }

    public interface IMockedInterface1 { }

    public interface IMockedInterface2 { }
}