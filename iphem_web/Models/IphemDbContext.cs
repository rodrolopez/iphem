using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace iphem_web.Models;

public partial class IphemDbContext : DbContext
{
    public IphemDbContext()
    {
    }

    private readonly IHttpContextAccessor _httpContextAccessor;

    // Le agregamos el IHttpContextAccessor al constructor
    public IphemDbContext(DbContextOptions<IphemDbContext> options, IHttpContextAccessor httpContextAccessor = null)
        : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public virtual DbSet<Estadosturno> Estadosturnos { get; set; }

    public virtual DbSet<Noticia> Noticias { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Tiposturno> Tiposturnos { get; set; }

    public virtual DbSet<Turno> Turnos { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }
    
    public virtual DbSet<Carrusel> Carrusel { get; set; }

    public virtual DbSet<ConfiguracionTurno> ConfiguracionTurnos { get; set; }

    public virtual DbSet<Auditoria> Auditorias { get; set; }

    public virtual DbSet<Triage> Triages { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Database=iphem_db;Username=postgres;Password=postgres");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Estadosturno>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("estadosturno_pkey");

            entity.ToTable("estadosturno");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Noticia>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("noticias_pkey");

            entity.ToTable("noticias");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Cuerpo).HasColumnName("cuerpo");
            entity.Property(e => e.Epigrafe)
                .HasMaxLength(200)
                .HasColumnName("epigrafe");
            entity.Property(e => e.Fechapublicacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("fechapublicacion");
            entity.Property(e => e.Idusuariocreador).HasColumnName("idusuariocreador");
            entity.Property(e => e.Imagenurl)
                .HasMaxLength(255)
                .HasColumnName("imagenurl");
            entity.Property(e => e.Piedefoto)
                .HasMaxLength(200)
                .HasColumnName("piedefoto");
            entity.Property(e => e.Subtitulo)
                .HasMaxLength(200)
                .HasColumnName("subtitulo");
            entity.Property(e => e.Titulo)
                .HasMaxLength(200)
                .HasColumnName("titulo");

            entity.HasOne(d => d.IdusuariocreadorNavigation).WithMany(p => p.Noticia)
                .HasForeignKey(d => d.Idusuariocreador)
                .HasConstraintName("noticias_idusuariocreador_fkey");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("roles_pkey");

            entity.ToTable("roles");

            entity.HasIndex(e => e.Nombre, "roles_nombre_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Tiposturno>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tiposturno_pkey");

            entity.ToTable("tiposturno");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Turno>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("turnos_pkey");

            entity.ToTable("turnos");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Apellidosolicitante)
                .HasMaxLength(100)
                .HasColumnName("apellidosolicitante");
            entity.Property(e => e.Dnisolicitante)
                .HasMaxLength(20)
                .HasColumnName("dnisolicitante");
            entity.Property(e => e.Emailsolicitante)
                .HasMaxLength(100)
                .HasColumnName("emailsolicitante");
            entity.Property(e => e.Fechahora)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("fechahora");
            entity.Property(e => e.Idestadoturno).HasColumnName("idestadoturno");
            entity.Property(e => e.Idtipoturno).HasColumnName("idtipoturno");
            entity.Property(e => e.Nombresolicitante)
                .HasMaxLength(100)
                .HasColumnName("nombresolicitante");
            entity.Property(e => e.Observaciones).HasColumnName("observaciones");
            entity.Property(e => e.Pacientedestino)
                .HasMaxLength(100)
                .HasColumnName("pacientedestino");
            entity.Property(e => e.Telefonosolicitante)
                .HasMaxLength(50)
                .HasColumnName("telefonosolicitante");

            entity.HasOne(d => d.IdestadoturnoNavigation).WithMany(p => p.Turnos)
                .HasForeignKey(d => d.Idestadoturno)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("turnos_idestadoturno_fkey");

            entity.HasOne(d => d.IdtipoturnoNavigation).WithMany(p => p.Turnos)
                .HasForeignKey(d => d.Idtipoturno)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("turnos_idtipoturno_fkey");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("usuarios_pkey");

            entity.ToTable("usuarios");

            entity.HasIndex(e => e.Email, "usuarios_email_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.Fechaalta)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("fechaalta");
            entity.Property(e => e.Idrol).HasColumnName("idrol");
            entity.Property(e => e.Nombrecompleto)
                .HasMaxLength(100)
                .HasColumnName("nombrecompleto");
            entity.Property(e => e.Passwordhash)
                .HasMaxLength(255)
                .HasColumnName("passwordhash");

            entity.HasOne(d => d.IdrolNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.Idrol)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("usuarios_idrol_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entradasAuditoria = new List<Auditoria>();

        // ChangeTracker es el motor de Entity Framework que sabe exactamente qué cambió en memoria
        foreach (var entry in ChangeTracker.Entries())
        {
            // Ignoramos si no hubo cambios, o si el cambio es en la misma tabla de Auditoría (para evitar bucles)
            if (entry.Entity is Auditoria || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                continue;

            // Intentamos leer el email o el nombre del usuario logueado. Si no hay nadie (ej: un proceso automático), ponemos "Sistema"
            string usuarioActual = _httpContextAccessor?.HttpContext?.User?.Identity?.Name
                                   ?? _httpContextAccessor?.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value
                                   ?? "Sistema (Sin Autenticar)";

            var auditoria = new Auditoria
            {
                Tabla = entry.Metadata.GetTableName() ?? "Desconocida",
                FechaHora = DateTime.UtcNow,
                Usuario = usuarioActual // <--- ¡Acá usamos el usuario real!
            };

            var valoresAntiguos = new Dictionary<string, object?>();
            var valoresNuevos = new Dictionary<string, object?>();

            // Revisamos propiedad por propiedad (ej: Nombre, DNI, Estado)
            foreach (var property in entry.Properties)
            {
                string propertyName = property.Metadata.Name;

                switch (entry.State)
                {
                    case EntityState.Added:
                        auditoria.Accion = "CREAR";
                        valoresNuevos[propertyName] = property.CurrentValue;
                        break;

                    case EntityState.Deleted:
                        auditoria.Accion = "ELIMINAR";
                        valoresAntiguos[propertyName] = property.OriginalValue;
                        break;

                    case EntityState.Modified:
                        if (property.IsModified)
                        {
                            auditoria.Accion = "MODIFICAR";
                            valoresAntiguos[propertyName] = property.OriginalValue;
                            valoresNuevos[propertyName] = property.CurrentValue;
                        }
                        break;
                }
            }

            // Convertimos los diccionarios a texto JSON solo si tienen datos
            auditoria.ValoresAntiguos = valoresAntiguos.Count == 0 ? null : System.Text.Json.JsonSerializer.Serialize(valoresAntiguos);
            auditoria.ValoresNuevos = valoresNuevos.Count == 0 ? null : System.Text.Json.JsonSerializer.Serialize(valoresNuevos);

            entradasAuditoria.Add(auditoria);
        }

        // Si detectamos cambios, agregamos nuestra bitácora antes de guardar
        if (entradasAuditoria.Any())
        {
            Auditorias.AddRange(entradasAuditoria);
        }

        // Finalmente, ejecutamos el guardado real en PostgreSQL
        return await base.SaveChangesAsync(cancellationToken);
    }
}
