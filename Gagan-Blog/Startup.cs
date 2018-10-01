using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Gagan_Blog.Startup))]
namespace Gagan_Blog
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
