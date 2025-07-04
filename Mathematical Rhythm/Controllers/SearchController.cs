using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace MathematicalRhythm.Controllers  // Asegúrate de usar tu namespace correcto
{
    public class SearchController : Controller
    {
        [HttpGet]
        public IActionResult Search(string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                {
                    return Json(new { success = false, message = "Término demasiado corto" });
                }

                // Datos mock mejorados
                var results = new
                {
                    Songs = new List<object>
                    {
                        new { Id = 1, Name = $"{query} (Remix)", Artist = "DJ Local", Duration = "3:45" },
                        new { Id = 2, Name = $"{query} Original Mix", Artist = "Artist Test", Duration = "5:20" }
                    },
                    Artists = new List<object>
                    {
                        new { Id = 1, Name = $"{query} Producer", Genre = "Techno" }
                    },
                    Albums = new List<object>
                    {
                        new { Id = 1, Name = $"{query} Album", Artist = "Various Artists", Year = DateTime.Now.Year }
                    }
                };

                return Json(new { success = true, data = results });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}