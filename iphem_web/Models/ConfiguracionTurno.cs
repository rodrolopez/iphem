using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace iphem_web.Models;

[Table("configuracionturnos")]
public partial class ConfiguracionTurno
{
    [Column("id")]
    public int Id { get; set; }

    [Column("horainicio")]
    public TimeSpan HoraInicio { get; set; }

    [Column("cantidadturnos")]
    public int CantidadTurnos { get; set; }

    [Column("intervalominutos")]
    public int IntervaloMinutos { get; set; }
}