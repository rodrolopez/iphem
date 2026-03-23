using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace iphem_web.Models;

public partial class Usuario
{
    public int Id { get; set; }

    // Agregamos el DNI para que puedan iniciar sesión
    [MaxLength(20)]
    public string? Dni { get; set; }

    public string Nombrecompleto { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Passwordhash { get; set; } = null!;

    public int Idrol { get; set; }

    public DateTime? Fechaalta { get; set; }

    public virtual Role IdrolNavigation { get; set; } = null!;

    public virtual ICollection<Noticia> Noticia { get; set; } = new List<Noticia>();
}
