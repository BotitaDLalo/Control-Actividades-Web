using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ControlActividades.Models;
using ControlActividades.Models.db;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace ControlActividades.Controllers
{
    // Controller to create a development test alumno user. REMOVE in production.
    public class DevController : Controller
    {
        [AllowAnonymous]
        public async Task<ActionResult> CreateTestAlumno()
        {
            // Restrict to local requests for safety
            if (!Request.IsLocal)
            {
                return new HttpStatusCodeResult(403, "Forbidden");
            }

            var db = new ApplicationDbContext();

            // create stores/managers using the application's DbContext
            var userStore = new UserStore<ApplicationUser>(db);
            var userManager = new UserManager<ApplicationUser>(userStore);
            var roleStore = new RoleStore<IdentityRole>(db);
            var roleManager = new RoleManager<IdentityRole>(roleStore);

            string email = "test.alumno@example.com";
            string password = "P@ssw0rd!";

            // create role if missing
            if (!roleManager.RoleExists("Alumno"))
            {
                roleManager.Create(new IdentityRole("Alumno"));
            }

            // create user if missing
            var user = await userManager.FindByNameAsync(email);
            if (user == null)
            {
                user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
                var res = await userManager.CreateAsync(user, password);
                if (!res.Succeeded)
                {
                    return Content("Failed to create user: " + string.Join(", ", res.Errors));
                }
            }

            // assign role
            if (!userManager.IsInRole(user.Id, "Alumno"))
            {
                userManager.AddToRole(user.Id, "Alumno");
            }

            // create tbAlumnos row if missing (use Models.db namespace)
            var existsAlumno = db.tbAlumnos.Any(a => a.UserId == user.Id);
            if (!existsAlumno)
            {
                var alumno = new tbAlumnos
                {
                    ApellidoPaterno = "ApellidoP",
                    ApellidoMaterno = "ApellidoM",
                    Nombre = "Test",
                    UserId = user.Id
                };
                db.tbAlumnos.Add(alumno);
                db.SaveChanges();
            }

            return Content($"Test alumno ensured: {email} / {password} -- delete this endpoint after use.");
        }
    }
}
