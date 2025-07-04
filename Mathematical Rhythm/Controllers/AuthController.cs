using Microsoft.AspNetCore.Mvc;
using MathematicalRhythm.Models;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using MathematicalRhythm.Models.ViewModels;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace ProyectoMathematicalRhythm.Controllers
{
    //sesion
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public class AuthController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        private readonly RecaptchaService _recaptchaService;

        public AuthController(
            IConfiguration configuration,
            RecaptchaService recaptchaService)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
            _recaptchaService = recaptchaService;
        }

        // GET: /Auth/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Auth/Login
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errores = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                var mensajeErrores = string.Join(", ", errores);
                TempData["Error"] = $"Campos inválidos: {mensajeErrores}";
                return View(model);
            }

            // Verificación de reCAPTCHA
            var recaptchaToken = Request.Form["g-recaptcha-response"];
            var isValidRecaptcha = await _recaptchaService.VerifyToken(recaptchaToken);

            if (!isValidRecaptcha)
            {
                TempData["Error"] = "Por favor, verifica que no eres un robot.";
                return View(model);
            }

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT IdUsuario, Nombre FROM Usuarios WHERE Email = @Email AND Contrasena = @Contrasena";
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Email", model.Email);
                cmd.Parameters.AddWithValue("@Contrasena", model.Contrasena);

                connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    HttpContext.Session.SetInt32("UsuarioId", reader.GetInt32(0));
                    HttpContext.Session.SetString("NombreUsuario", reader.GetString(1));
                    return RedirectToAction("Index", "VistaUsuario");
                }
                else
                {
                    TempData["Error"] = "Email o contraseña incorrectos.";
                    return View(model);
                }
            }
        }

        // GET: /Auth/Registro
        [HttpGet]
        public IActionResult Registro()
        {
            return View();
        }

        // POST: /Auth/Registro
        [HttpPost]
        public async Task<IActionResult> Registro(Usuario usuario)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                TempData["Error"] = $"Errores: {string.Join(", ", errors.Select(e => e.ErrorMessage))}";
                return View(usuario);
            }

            // Verificación de reCAPTCHA
            var recaptchaToken = Request.Form["g-recaptcha-response"];
            var isValidRecaptcha = await _recaptchaService.VerifyToken(recaptchaToken);

            if (!isValidRecaptcha)
            {
                TempData["Error"] = "Por favor, verifica que no eres un robot.";
                return View(usuario);
            }

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string checkQuery = "SELECT COUNT(*) FROM Usuarios WHERE Email = @Email";
                SqlCommand checkCmd = new SqlCommand(checkQuery, connection);
                checkCmd.Parameters.AddWithValue("@Email", usuario.Email);

                connection.Open();
                int count = (int)checkCmd.ExecuteScalar();

                if (count > 0)
                {
                    TempData["Error"] = "El email ya está registrado.";
                    return View(usuario);
                }

                string insertQuery = @"INSERT INTO Usuarios (Nombre, Email, Contrasena) 
                                    VALUES (@Nombre, @Email, @Contrasena)";
                SqlCommand insertCmd = new SqlCommand(insertQuery, connection);
                insertCmd.Parameters.AddWithValue("@Nombre", usuario.Nombre);
                insertCmd.Parameters.AddWithValue("@Email", usuario.Email);
                insertCmd.Parameters.AddWithValue("@Contrasena", usuario.Contrasena);

                insertCmd.ExecuteNonQuery();
            }

            TempData["Mensaje"] = "Registro exitoso. Ya puedes iniciar sesión.";
            return RedirectToAction("Login");
        }




        // GET: /Auth/Logout

        [HttpGet] // o [HttpPost] si usas un formulario
        public IActionResult Logout()
        {
            // 1. Destruir la sesión
            HttpContext.Session.Clear();

            // 2. Eliminar la cookie de sesión (¡nombre debe coincidir con Program.cs!)
            Response.Cookies.Delete(
                "TuApp.Session",
                new CookieOptions
                {
                    Secure = true,
                    HttpOnly = true,
                    SameSite = SameSiteMode.Strict
                }
            );

            // 3. Redirigir al Login (NO a "Logout")
            return RedirectToAction("Login", "Auth"); // Asegúrate que "Auth" sea tu controlador de login
        }
    }
}