using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace iphem_web.Models;

public class Triage
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int IdTurno { get; set; }

    public DateTime FechaCompletado { get; set; }

    // --- PREGUNTAS CLAVE DEL IPHEM ---
    [Display(Name = "¿Pesás más de 50 kg?")]
    public bool PesoMayor50Kg { get; set; }

    [Display(Name = "¿Te sentís en buen estado de salud hoy?")]
    public bool SentirseBienHoy { get; set; }

    [Display(Name = "¿Te hiciste tatuajes o piercings en los últimos 12 meses?")]
    public bool TatuajesPiercings12Meses { get; set; }

    [Display(Name = "¿Tuviste cirugías en los últimos 12 meses?")]
    public bool Cirugias12Meses { get; set; }

    [Display(Name = "¿Tomaste antibióticos en los últimos 7 días?")]
    public bool TomoAntibioticos7Dias { get; set; }

    [Display(Name = "¿Estás embarazada o en período de lactancia? (Solo mujeres)")]
    public bool EmbarazoOLactancia { get; set; }

    // --- FIRMA DIGITAL / CONSENTIMIENTO ---
    [Required]
    public bool AceptoDeclaracionJurada { get; set; }

    // Relación con el Turno
    [ForeignKey("IdTurno")]
    public virtual Turno TurnoNavigation { get; set; } = null!;
}