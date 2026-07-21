using Eaf.Middleware.Auditing;
using Shouldly;
using Xunit;

namespace Eaf.ProjectName.Tests.Auditing
{
    // ReSharper disable once InconsistentNaming
    public class NamespaceStripper_Tests : ProjectNameTestBase
    {
        private readonly INamespaceStripper _namespaceStripper;

        public NamespaceStripper_Tests()
        {
            _namespaceStripper = Resolve<INamespaceStripper>();
        }

        [Fact]
        public void Should_Stripe_Namespace()
        {
            var controllerName = _namespaceStripper.StripNameSpace("Eaf.ProjectName.Web.Controllers.HomeController");
            controllerName.ShouldBe("HomeController");
        }

        [Theory]
        [InlineData("Eaf.ProjectName.Auditing.GenericEntityService`1[[Eaf.ProjectName.Storage.BinaryObject, Eaf.ProjectName.Core, Version=1.10.1.0, Culture=neutral, PublicKeyToken=null]]", "GenericEntityService<BinaryObject>")]
        [InlineData("CompanyName.ProductName.Services.Base.EntityService`6[[CompanyName.ProductName.Entity.Book, CompanyName.ProductName.Core, Version=1.10.1.0, Culture=neutral, PublicKeyToken=null],[CompanyName.ProductName.Services.Dto.Book.CreateInput, N...", "EntityService<Book, CreateInput>")]
        [InlineData("Eaf.ProjectName.Auditing.XEntityService`1[Eaf.ProjectName.Auditing.AService`5[[Eaf.ProjectName.Storage.BinaryObject, Eaf.ProjectName.Core, Version=1.10.1.0, Culture=neutral, PublicKeyToken=null],[Eaf.ProjectName.Storage.TestObject, Eaf.ProjectName.Core, Version=1.10.1.0, Culture=neutral, PublicKeyToken=null],]]", "XEntityService<AService<BinaryObject, TestObject>>")]
        public void Should_Stripe_Generic_Namespace(string serviceName, string result)
        {
            var genericServiceName = _namespaceStripper.StripNameSpace(serviceName);
            genericServiceName.ShouldBe(result);
        }
    }
}