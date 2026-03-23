using System;
using System.Collections.Generic;

namespace iphem_web.Models;

public partial class Estadosturno
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<Turno> Turnos { get; set; } = new List<Turno>();
}
