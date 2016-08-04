using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(CBLSummerBugTracker08042016.Startup))]
namespace CBLSummerBugTracker08042016
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
