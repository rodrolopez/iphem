using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace iphem_web.Models;

public partial class Turno
{
    public int Id { get; set; }

    public DateTime Fechahora { get; set; }

    public string Dnisolicitante { get; set; } = null!;

    public string Nombresolicitante { get; set; } = null!;

    public string Apellidosolicitante { get; set; } = null!;

    [Column("gruposanguineo")]
    public string? GrupoSanguineo { get; set; }

    [Column("departamento")]
    public string? Departamento { get; set; }

    public string? Emailsolicitante { get; set; }

    public string? Telefonosolicitante { get; set; }

    public string? Pacientedestino { get; set; }

    public int Idtipoturno { get; set; }

    public int Idestadoturno { get; set; }

    public string? Observaciones { get; set; }

    // Nueva propiedad para la firma clínica
    public string? FirmaLaboratorista { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    public virtual Estadosturno IdestadoturnoNavigation { get; set; } = null!;

    public virtual Tiposturno IdtipoturnoNavigation { get; set; } = null!;

    // Relación 1 a 1: Un turno tiene un solo cuestionario de Triage
    public virtual Triage? TriageNavigation { get; set; }

    // ==========================================
    // RESULTADOS CLÍNICOS
    // ==========================================
    [MaxLength(255)]
    public string? RutaPdfResultado { get; set; }

    public string? NotasLaboratorio { get; set; }

    // Vínculo con el Usuario (CRM) - Es nullable (?) para compatibilidad con turnos viejos
    public int? IdUsuario { get; set; }

    [ForeignKey("IdUsuario")]
    public virtual Usuario? UsuarioNavigation { get; set; }
}
