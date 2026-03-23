using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace iphem_web.Models;

public partial class Noticia
{
    public int Id { get; set; }

    public string Titulo { get; set; } = null!;

    public string? Subtitulo { get; set; }

    public string? Epigrafe { get; set; }

    public string Cuerpo { get; set; } = null!;
    
    [Column("imagenpath")]
    public string? ImagenPath { get; set; }

    public string? Imagenurl { get; set; }

    public string? Piedefoto { get; set; }

    public DateTime? Fechapublicacion { get; set; }

    public int? Idusuariocreador { get; set; }

    public virtual Usuario? IdusuariocreadorNavigation { get; set; }

}
