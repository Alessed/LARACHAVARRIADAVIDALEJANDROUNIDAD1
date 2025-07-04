using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MathematicalRhythm.Models;

namespace MathematicalRhythm.Models
{
    public class Recuperacion
    {
        [Key]
        public int IdRecuperacion { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public Usuario Usuario { get; set; }

        public Guid Token { get; set; } = Guid.NewGuid();

        public DateTime FechaSolicitud { get; set; } = DateTime.Now;

        public bool Usado { get; set; } = false;
    }
}
