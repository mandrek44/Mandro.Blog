using System.Security.Claims;

using Mandro.Owin.SimpleMVC;

using Microsoft.Owin.Security;
using Microsoft.WindowsAzure;

namespace Mandro.Blog.Worker.Controllers
{
    public class Account
    {
        public void GetLogin()
        {
        }

        [Authorize]
        public dynamic GetInfo(dynamic environment)
        {
            return null;
        }

        public dynamic PostLogin(dynamic loginDetails)
        {
            var adminPass = CloudConfigurationManager.GetSetting("AdminPass").Trim();

            if (loginDetails.UserName != "Mandro" || string.IsNullOrEmpty(loginDetails.Password) || loginDetails.Password != adminPass)
            {
                return Redirect.To((Account controller) => controller.GetInfo);
            }

            var claims = new[] { new Claim(ClaimTypes.Name, loginDetails.UserName) };
            var id = new ClaimsIdentity(claims, "Cookie");

            var context = loginDetails.Context.Authentication as IAuthenticationManager;
            context.SignIn(id);

            return Redirect.To((Home controller) => controller.GetIndex);
        }
    }
}