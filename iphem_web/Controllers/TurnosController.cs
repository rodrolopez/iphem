using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using iphem_web.Models;
using BCrypt;

namespace iphem_web.Controllers;

public class TurnosController : Controller
{
    private readonly IphemDbContext _context;

    public TurnosController(IphemDbContext context)
    {
        _context = context;
    }

    // GET: Turnos/Create
    public IActionResult Create()
    {
        ViewData["IdTipoTurno"] = new SelectList(_context.Tiposturnos, "Id", "Nombre");
        return View();
    }

    // POST: Turnos/Create
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create([Bind("Id,Fechahora,Dnisolicitante,Nombresolicitante,Apellidosolicitante,Gruposanguineo,Departamento,Emailsolicitante,Telefonosolicitante,Pacientedestino,Idtipoturno,Observaciones")] Turno turno)
{
    ModelState.Remove("IdestadoturnoNavigation");
    ModelState.Remove("IdtipoturnoNavigation");
    ModelState.Remove("UsuarioNavigation");

    if (ModelState.IsValid)
    {
        // 1. Datos automáticos del turno
        turno.FechaCreacion = DateTime.Now;
        turno.Idestadoturno = 1; // 1 es "Pendiente"

        // ==============================================================
        // 2. LÓGICA DE LA CUENTA FANTASMA (CRM - Versión a prueba de balas)
        // ==============================================================
        if (!string.IsNullOrEmpty(turno.Dnisolicitante))
        {
            var dniLimpio = turno.Dnisolicitante.Replace(".", "").Trim();
            var emailLimpio = turno.Emailsolicitante?.Trim().ToLower();

            // Buscamos si el usuario ya existe por DNI **O** por Email (para atrapar tus pruebas viejas)
            var usuarioExistente = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Dni == dniLimpio || (!string.IsNullOrEmpty(emailLimpio) && u.Email == emailLimpio));

            if (usuarioExistente != null)
            {
                // ¡Es un usuario que ya existe! Lo vinculamos
                turno.IdUsuario = usuarioExistente.Id;

                bool requiereUpdate = false;

                // AUTO-CURACIÓN: Si era una prueba vieja que tenía Email pero NO tenía DNI, se lo agregamos
                if (string.IsNullOrEmpty(usuarioExistente.Dni))
                {
                    usuarioExistente.Dni = dniLimpio;
                    usuarioExistente.Passwordhash = BCrypt.Net.BCrypt.HashPassword(dniLimpio); // Le seteamos la clave nueva segura
                    requiereUpdate = true;
                }

                // AUTO-CURACIÓN: Si tenía DNI pero no tenía Email, se lo sumamos
                if (string.IsNullOrEmpty(usuarioExistente.Email) && !string.IsNullOrEmpty(emailLimpio))
                {
                    // Chequeamos que por alguna razón insólita no esté tomado
                    bool emailOcupado = await _context.Usuarios.AnyAsync(u => u.Id != usuarioExistente.Id && u.Email == emailLimpio);
                    if (!emailOcupado)
                    {
                        usuarioExistente.Email = emailLimpio;
                        requiereUpdate = true;
                    }
                }

                if (requiereUpdate)
                {
                    _context.Usuarios.Update(usuarioExistente);
                }
            }
            else
            {
                // ¡Es un donante 100% nuevo!
                var rolDonante = await _context.Roles.FirstOrDefaultAsync(r => r.Nombre == "Donante");
                int idRol = rolDonante != null ? rolDonante.Id : 3;

                // Prevención extrema: Si por un milagro el email sigue ocupado, lo dejamos nulo para que la base no explote
                bool emailOcupado = !string.IsNullOrEmpty(emailLimpio) && await _context.Usuarios.AnyAsync(u => u.Email == emailLimpio);
                string? emailFinal = emailOcupado ? null : emailLimpio;

                var nuevoUsuario = new Usuario
                {
                    Dni = dniLimpio,
                    Email = emailFinal,
                    Nombrecompleto = $"{turno.Nombresolicitante.Trim()} {turno.Apellidosolicitante.Trim()}",
                    Passwordhash = BCrypt.Net.BCrypt.HashPassword(dniLimpio),
                    Idrol = idRol,
                    Fechaalta = DateTime.Now
                };

                _context.Usuarios.Add(nuevoUsuario);
                await _context.SaveChangesAsync();

                turno.IdUsuario = nuevoUsuario.Id;
            }
        }

        // ==============================================================

        // 3. Guardamos el turno
        _context.Turnos.Add(turno);
        await _context.SaveChangesAsync(); // ACÁ Postgres le asigna un número de ID al turno

        // 4. ¡LA SOLUCIÓN! Redirigimos a tu vista pero pasándole el ID recién creado
        return RedirectToAction("Confirmacion", new { id = turno.Id });
    }

    // Si el formulario tenía errores, volvemos a cargar los selectores (combobox)
    ViewData["Idestadoturno"] = new SelectList(_context.Estadosturnos, "Id", "Nombre", turno.Idestadoturno);
    ViewData["Idtipoturno"] = new SelectList(_context.Tiposturnos, "Id", "Nombre", turno.Idtipoturno);
    return View(turno);
}

    // GET: Turnos/Confirmacion/5
    public async Task<IActionResult> Confirmacion(int? id)
    {
        if (id == null) return RedirectToAction("Index", "Home");

        var turno = await _context.Turnos
            .Include(t => t.IdtipoturnoNavigation)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (turno == null) return NotFound();

        return View(turno);
    }

    // --- ACÁ EMPIEZA EL MÉTODO NUEVO QUE AGREGAMOS ---

    // GET: /Turnos/GetHorariosDisponibles
    [HttpGet]
    public async Task<IActionResult> GetHorariosDisponibles(DateTime fecha)
    {
        var config = await _context.ConfiguracionTurnos.FirstOrDefaultAsync();
        if (config == null)
        {
            config = new ConfiguracionTurno { HoraInicio = new TimeSpan(8, 0, 0), CantidadTurnos = 20, IntervaloMinutos = 10 };
        }

        // Definimos el inicio y fin del día para que Postgres lo entienda fácil
        var inicioDia = fecha.Date;
        var finDia = inicioDia.AddDays(1);

        // Traemos todos los turnos de ese día a la memoria
        var turnosDelDia = await _context.Turnos
            .Where(t => t.Fechahora >= inicioDia && t.Fechahora < finDia)
            .ToListAsync();

        // Recién en memoria, les sacamos la hora
        var turnosOcupados = turnosDelDia
            .Select(t => t.Fechahora.TimeOfDay)
            .ToList();

        var horariosDisponibles = new List<string>();
        TimeSpan horaActual = config.HoraInicio;

        for (int i = 0; i < config.CantidadTurnos; i++)
        {
            if (!turnosOcupados.Contains(horaActual))
            {
                horariosDisponibles.Add(horaActual.ToString(@"hh\:mm"));
            }

            horaActual = horaActual.Add(TimeSpan.FromMinutes(config.IntervaloMinutos));
        }

        return Json(horariosDisponibles);
    }

    // GET: Turnos
    // Recibe el filtro (ej: "hoy") y el orden deseado
    public async Task<IActionResult> Index(string? filtro, string? orden)
    {
        var query = _context.Turnos
            .Include(t => t.IdestadoturnoNavigation)
            .Include(t => t.IdtipoturnoNavigation)
            .AsQueryable();

        // 1. LÓGICA DEL FILTRO (Si viene de la tarjeta del Dashboard)
        if (filtro == "hoy")
        {
            var fechaHoy = DateTime.Today;
            // Filtramos solo los turnos cuya fecha coincida con hoy
            query = query.Where(t => t.Fechahora.Date == fechaHoy);
            ViewBag.FiltroActivo = "Viendo únicamente los turnos para el día de hoy.";
        }

        // 2. LÓGICA DE ORDENAMIENTO
        if (orden == "fecha_turno")
        {
            // Orden cronológico: ideal para ver cómo viene la agenda de la semana
            query = query.OrderBy(t => t.Fechahora);
            ViewBag.OrdenActual = "fecha_turno";
        }
        else
        {
            // DEFAULT (Bandeja de entrada): Los últimos turnos creados aparecen primero.
            query = query.OrderByDescending(t => t.FechaCreacion);
            ViewBag.OrdenActual = "recientes";
        }

        var turnos = await query.ToListAsync();
        return View(turnos);
    }
}