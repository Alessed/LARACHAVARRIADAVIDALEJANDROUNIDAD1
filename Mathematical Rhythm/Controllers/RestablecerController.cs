using System;
using System.Threading.Tasks;
using MathematicalRhythm.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MathematicalRhythm.Data;

namespace MathematicalRhythm.Controllers
{
    public class RestablecerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RestablecerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Restablecer/Restablecer
        public IActionResult Restablecer()
        {
            // Obtener el token de TempData
            var token = TempData["TokenValido"]?.ToString();
            var email = TempData["EmailUsuario"]?.ToString();

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Recuperar", "Recuperar");
            }

            // Guardar nuevamente para el POST
            TempData.Keep("TokenValido");
            TempData.Keep("EmailUsuario");

            return View("~/Views/Recuperar/Restablecer.cshtml");
        }



        // POST: /Restablecer/Restablecer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restablecer(
     string nuevaContrasena,
     string confirmarContrasena)
        {
            try
            {
                var token = TempData["TokenValido"]?.ToString();
                var email = TempData["EmailUsuario"]?.ToString();

                if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Sesión inválida. Por favor inicie el proceso nuevamente."
                    });
                }

                if (nuevaContrasena != confirmarContrasena)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Las contraseñas no coinciden"
                    });
                }

                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u =>
                        u.Email == email &&
                        u.TokenRecuperacion == token &&
                        u.FechaExpiracionToken > DateTime.Now);

                if (usuario == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Token inválido o expirado"
                    });
                }

                // Actualizar contraseña (usa hashing en producción)
                usuario.Contrasena = nuevaContrasena;
                usuario.TokenRecuperacion = null;
                usuario.FechaExpiracionToken = null;

                _context.Update(usuario);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    redirectUrl = Url.Action("Login", "Auth")
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Error interno: " + ex.Message
                });
            }
        }
    }
    }
