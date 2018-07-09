namespace Tollrech.Tests.Test.Data.MockedClassCreateFixTests
{
    public class Class1 : TestBase
    {
        private MockedClass mockedClass;

        void Some()
        {
            mockedClass = new MockedClass({caret});
        }

        public void Test()
        {
            DateTime d;
            var productId = "productId";
            var resourc1 = "resourc1";
            var resourc2 = "resourc2";
            var resources = new[] { resourc1, resourc2 };
            var date1 = new DateTime(2010, 10, 10);
            var date2 = new DateTime(2011, 11, 11);
            mockedClass.Do(productId, resources, date1, date2);
        }
    }

    public class MockedClass
    {
        private readonly IQueryExecutorFactory _queryExecutorFactory;
        private readonly IArrayed[] _arrayeds;
        private readonly IClass _clas;

        public MockedClass(IQueryExecutorFactory queryExecutorFactory, decimal unmockable1, IArrayed[] arrayeds, IClass clas, IClass clas2, IArrayed[] arrayeds2, int unmockable2)
        {
            _queryExecutorFactory = queryExecutorFactory;
            _arrayeds = arrayeds;
            _clas = clas;
        }

        public void Do(string productId, string[] resources, DateTime date1, DateTime date2)
        {

        }
    }

    public class TestBase : UnitTestBase
    {
        protected IQueryExecutorFactory QueryExecutorFactory;
    }

    public class UnitTestBase
    {
        protected T NewMock<T>()
        {
            return default(T);
        }
    }

    public interface IQueryExecutorFactory
    {

    }

    public interface IArrayed
    {

    }

    public interface IClass
    {

    }

    public class Class : IClass
    {
    }
}