using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace iphem_web.Models;

[Table("carrusel")]
public partial class Carrusel
{
    [Column("id")]
    public int Id { get; set; }
    
    [Column(name: "titulo")]
    public string? Titulo { get; set; }
    
    [Column(name:"subtitulo")]
    public string? Subtitulo { get; set; }

    [Column("imagenpath")] // Acordate del truco de postgres
    public string ImagenPath { get; set; } = null!;
    
    [Column("enlace")]
    public string? Enlace { get; set; }
    
    [Column("activo")]
    public bool Activo { get; set; }
}