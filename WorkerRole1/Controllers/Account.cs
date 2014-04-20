using System.Security.Claims;

using Microsoft.Owin;
using Microsoft.Owin.Security;

namespace Mandro.Blog.Worker.Controllers
{
    public class Account
    {
        public void GetLogin()
        {

        }

        public void PostLogin(dynamic loginDetails)
        {
            if (loginDetails.UserName != "Mandro" || loginDetails.Password != "957gNH4wEAe5ZpO2FGdG")
            {
                return;
            }

            var claims = new[] { new Claim(ClaimTypes.Name, loginDetails.UserName) };
            var id = new ClaimsIdentity(claims, "Cookie");

            var context = loginDetails.Context.Authentication as IAuthenticationManager;
            context.SignIn(id);
        }
    }
}