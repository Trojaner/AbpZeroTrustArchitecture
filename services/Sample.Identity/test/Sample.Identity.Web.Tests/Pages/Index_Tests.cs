using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace Sample.Identity.Pages;

public class Index_Tests : IdentityWebTestBase
{
    [Fact]
    public async Task Welcome_Page()
    {
        var response = await GetResponseAsStringAsync("/");
        response.ShouldNotBeNull();
    }
}
