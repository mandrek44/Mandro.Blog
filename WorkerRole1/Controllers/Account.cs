using System.Security.Claims;

using Mandro.Owin.SimpleMVC;

using Microsoft.Owin.Security;

namespace Mandro.Blog.Worker.Controllers
{
    public class Account
    {
        public void GetLogin()
        {

        }

        [Authorize]
        public void GetInfo()
        {
            
        }

        public dynamic PostLogin(dynamic loginDetails)
        {
            if (loginDetails.UserName != "Mandro" || loginDetails.Password != "957gNH4wEAe5ZpO2FGdG")
            {
                return null;
            }

            var claims = new[] { new Claim(ClaimTypes.Name, loginDetails.UserName) };
            var id = new ClaimsIdentity(claims, "Cookie");

            var context = loginDetails.Context.Authentication as IAuthenticationManager;
            context.SignIn(id);

            return Redirect.To((Home controller) => controller.GetIndex);
        }
    }
}