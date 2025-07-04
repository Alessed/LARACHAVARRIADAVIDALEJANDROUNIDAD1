using System;
using System.Linq;
using System.Threading.Tasks;
using MathematicalRhythm.Helpers;
using MathematicalRhythm.Models;
using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MathematicalRhythm.Data;

namespace MathematicalRhythm.Controllers
{
    public class RecuperarController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RecuperarController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Recuperar/Recuperar
        public IActionResult Recuperar()
        {
            return View();
        }

        // POST: /Recuperar/Recuperar
        [HttpPost]
        public async Task<IActionResult> Recuperar(string correo)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == correo);
            if (usuario == null)
            {
                ViewBag.Mensaje = "No se encontró el correo.";
                return View();
            }

            var token = TokenHelper.GenerarToken();
            usuario.TokenRecuperacion = token;
            usuario.FechaExpiracionToken = DateTime.Now.AddHours(1);
            await _context.SaveChangesAsync();

            await EnviarCorreoRecuperacion(usuario, token);

            // Guardar el email en TempData para usarlo en la siguiente vista
            TempData["EmailRecuperacion"] = usuario.Email;
            return RedirectToAction("IngresarToken");
        }

        // GET: /Recuperar/IngresarToken
        public IActionResult IngresarToken()
        {
            var email = TempData["EmailRecuperacion"]?.ToString();
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Recuperar");
            }

            ViewBag.Email = email;
            return View();
        }

        // POST: /Recuperar/IngresarToken
        [HttpPost]
        public async Task<IActionResult> IngresarToken(string email, string token)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u =>
                u.Email == email &&
                u.TokenRecuperacion == token &&
                u.FechaExpiracionToken > DateTime.Now);

            if (usuario == null)
            {
                ViewBag.Error = "Token inválido o expirado";
                ViewBag.Email = email;
                return View();
            }

            // Guardar el token en TempData para usarlo en el RestablecerController
            TempData["TokenValido"] = token;
            TempData["EmailUsuario"] = email;

            return RedirectToAction("Restablecer", "Restablecer");
        }

        private async Task EnviarCorreoRecuperacion(Usuario usuario, string token)
        {
            var mensaje = new MimeMessage();
            mensaje.From.Add(new MailboxAddress("Mathematical Rhythm", "tu_correo@gmail.com"));
            mensaje.To.Add(new MailboxAddress(usuario.Nombre, usuario.Email));
            mensaje.Subject = "Recupera tu contraseña";

            mensaje.Body = new TextPart("html")
            {
                Text = $@"
                <p>Hola {usuario.Nombre},</p>
                <p>Hemos recibido una solicitud para restablecer tu contraseña.</p>
                <p>Tu código de verificación es: <strong>{token}</strong></p>
                <p>Este código expirará en 1 hora.</p>
                <p>Si no solicitaste este cambio, por favor ignora este mensaje.</p>"
            };

            using (var smtp = new SmtpClient())
            {
                await smtp.ConnectAsync("smtp.gmail.com", 587, false);
                await smtp.AuthenticateAsync("proyectoalex16@gmail.com", "anyiktvcvclrfore");
                await smtp.SendAsync(mensaje);
                await smtp.DisconnectAsync(true);
            }
        }
    }
}