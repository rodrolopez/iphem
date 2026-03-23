using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace iphem_web.Models;

public class Auditoria
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Usuario { get; set; } = null!; // Quién hizo el cambio

    [Required]
    public string Accion { get; set; } = null!; // CREAR, MODIFICAR, ELIMINAR

    [Required]
    public string Tabla { get; set; } = null!; // En qué tabla impactó

    // Guardamos los datos en formato JSON para poder ver exactamente qué cambió
    public string? ValoresAntiguos { get; set; }
    public string? ValoresNuevos { get; set; }

    public DateTime FechaHora { get; set; } = DateTime.UtcNow;
}