using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MathematicalRhythm.Models
{
    public class Usuario
    {
        public int IdUsuario { get; set; }

        
        public string Nombre { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        
        [Required]
        public string Contrasena { get; set; }


        public DateTime FechaRegistro { get; set; }

        public bool Estado { get; set; }


        public string? TokenRecuperacion { get; set; }
        public DateTime? FechaExpiracionToken { get; set; }


        // Relaciones
       
    }
}
