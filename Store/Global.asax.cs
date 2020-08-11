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

        // Создаем метод обработки запросов аутентификации
        protected void Application_AuthenticateRequest()
        {
            // Провкряем, авторизован ли пользователь
            if (User == null)
                return;

            // Получаем имя пользователя
            string userName = Context.User.Identity.Name;

            // Обьявляем массив ролей
            string[] roles = null;

            using (Db db = new Db())
            {
                // Заполняем массив ролями
                UserDTO dto = db.Users.FirstOrDefault(x => x.Username == userName);

                if (dto == null)
                    return;

                roles = db.UserRoles.Where(x => x.UserId == dto.Id)
                    .Select(x => x.Role.Name).ToArray();
            }

            // Создаем обьект интерфейса IPrincipal
            IIdentity userIdentity = new GenericIdentity(userName);
            IPrincipal newUserObject = new GenericPrincipal(userIdentity, roles);

            // Обьявляем и инициализируем данными Context.User
            Context.User = newUserObject;


        }
    }
}
