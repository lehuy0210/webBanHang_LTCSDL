using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(webBH.Startup))]
namespace webBH
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
