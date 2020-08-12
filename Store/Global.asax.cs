using Store.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Store
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        // Create a method for handling authentication requests
        protected void Application_AuthenticateRequest()
        {
            // Check if the user is authorized
            if (User == null)
                return;

            string userName = Context.User.Identity.Name;

            string[] roles = null;

            using (Db db = new Db())
            {
                // Filling the array with roles
                UserDTO dto = db.Users.FirstOrDefault(x => x.Username == userName);

                if (dto == null)
                    return;

                roles = db.UserRoles.Where(x => x.UserId == dto.Id)
                    .Select(x => x.Role.Name).ToArray();
            }

            // Create the IPrincipal interface object
            IIdentity userIdentity = new GenericIdentity(userName);
            IPrincipal newUserObject = new GenericPrincipal(userIdentity, roles);

            // Declare and initialize with data Context.User
            Context.User = newUserObject;


        }
    }
}
