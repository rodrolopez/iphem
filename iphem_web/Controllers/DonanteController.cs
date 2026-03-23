using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using iphem_web.Models;
using Microsoft.AspNetCore.Hosting; // Agregamos esto

namespace iphem_web.Controllers;

[Authorize(Roles = "Donante")]
public class DonanteController : Controller
{
    private readonly IphemDbContext _context;
    private readonly IWebHostEnvironment _env; // Herramienta de archivos

    // Actualizamos el constructor
    public DonanteController(IphemDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    // El Dashboard del Donante
    public async Task<IActionResult> Index()
    {
        // 1. Obtenemos el ID del usuario que está logueado desde sus Claims
        var userIdClaim = User.FindFirst("IdUsuario")?.Value;
        if (userIdClaim == null) return RedirectToAction("Login", "Acceso");

        int userId = int.Parse(userIdClaim);

        // 2. Buscamos TODOS sus turnos (los de hoy, los viejos y los futuros)
        var misTurnos = await _context.Turnos
            .Include(t => t.IdestadoturnoNavigation)
            .Include(t => t.IdtipoturnoNavigation)
            .Include(t => t.TriageNavigation)
            .Where(t => t.IdUsuario == userId)
            .OrderByDescending(t => t.Fechahora)
            .ToListAsync();

        return View(misTurnos);
    }
    // POST: Donante/Cancelar
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(int id)
    {
        // Obtenemos el ID del usuario logueado
        var userIdClaim = User.FindFirst("IdUsuario")?.Value;
        if (userIdClaim == null) return RedirectToAction("Login", "Acceso");
        int userId = int.Parse(userIdClaim);

        // Buscamos el turno, asegurándonos de que sea de ESTE usuario
        var turno = await _context.Turnos.FirstOrDefaultAsync(t => t.Id == id && t.IdUsuario == userId);

        if (turno != null)
        {
            // Buscamos cuál es el ID del estado "Cancelado" en tu base de datos
            var estadoCancelado = await _context.Estadosturnos.FirstOrDefaultAsync(e => e.Nombre == "Cancelado");

            if (estadoCancelado != null)
            {
                turno.Idestadoturno = estadoCancelado.Id;
                _context.Turnos.Update(turno);
                await _context.SaveChangesAsync();
            }
        }

        // Lo devolvemos a su panel
        return RedirectToAction(nameof(Index));
    }

    // GET: Donante/CambiarClave
    public IActionResult CambiarClave()
    {
        return View();
    }

    // POST: Donante/CambiarClave
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarClave(string claveActual, string nuevaClave, string confirmarClave)
    {
        // 1. Validamos que las nuevas coincidan
        if (nuevaClave != confirmarClave)
        {
            ViewBag.Error = "Las contraseñas nuevas no coinciden. Verificalas e intentá de nuevo.";
            return View();
        }

        // 2. Buscamos al usuario logueado
        var userIdClaim = User.FindFirst("IdUsuario")?.Value;
        if (userIdClaim == null) return RedirectToAction("Login", "Acceso");
        int userId = int.Parse(userIdClaim);

        var usuario = await _context.Usuarios.FindAsync(userId);
        if (usuario == null) return NotFound();

        // 3. Verificamos que sepa su contraseña actual (usando BCrypt)
        if (!BCrypt.Net.BCrypt.Verify(claveActual, usuario.Passwordhash))
        {
            ViewBag.Error = "La contraseña actual ingresada es incorrecta.";
            return View();
        }

        // 4. Todo en orden: Hasheamos la nueva y guardamos
        usuario.Passwordhash = BCrypt.Net.BCrypt.HashPassword(nuevaClave);
        _context.Usuarios.Update(usuario);
        await _context.SaveChangesAsync();

        // Mandamos un mensajito de éxito al Index usando TempData
        TempData["MensajeExito"] = "Tu contraseña fue actualizada correctamente. Ya podés usarla en tu próximo ingreso.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Donante/CompletarTriage/5
    public async Task<IActionResult> CompletarTriage(int id)
    {
        var userIdClaim = User.FindFirst("IdUsuario")?.Value;
        if (userIdClaim == null) return RedirectToAction("Login", "Acceso");
        int userId = int.Parse(userIdClaim);

        // Verificamos que el turno sea de este usuario
        var turno = await _context.Turnos
            .Include(t => t.TriageNavigation)
            .FirstOrDefaultAsync(t => t.Id == id && t.IdUsuario == userId);

        if (turno == null) return NotFound();

        // Si ya lo completó, lo pateamos de vuelta al inicio
        if (turno.TriageNavigation != null)
        {
            TempData["MensajeInfo"] = "Ya completaste la Declaración Jurada para este turno.";
            return RedirectToAction(nameof(Index));
        }

        // Le mandamos un Triage en blanco pero ya asociado a su Turno
        var modelo = new Triage { IdTurno = turno.Id };
        return View(modelo);
    }

    // POST: Donante/CompletarTriage
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompletarTriage([Bind("IdTurno,PesoMayor50Kg,SentirseBienHoy,TatuajesPiercings12Meses,Cirugias12Meses,TomoAntibioticos7Dias,EmbarazoOLactancia,AceptoDeclaracionJurada")] Triage triage)
    {
        // Ignoramos la propiedad de navegación para que no rompa la validación
        ModelState.Remove("TurnoNavigation");

        if (ModelState.IsValid)
        {
            // Sellamos la hora exacta en la que lo firmó digitalmente
            triage.FechaCompletado = DateTime.Now;

            _context.Triages.Add(triage);
            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = "¡Cuestionario de salud completado con éxito! Tu proceso en el IPHEM será mucho más rápido.";
            return RedirectToAction(nameof(Index));
        }

        return View(triage);
    }

    //=======================================
    // CARGA DE ANALISIS

    // GET: Donante/DescargarResultado/5
    public async Task<IActionResult> DescargarResultado(int id)
    {
        // 1. Identificamos quién lo pide
        var userIdClaim = User.FindFirst("IdUsuario")?.Value;
        if (userIdClaim == null) return RedirectToAction("Login", "Acceso");
        int userId = int.Parse(userIdClaim);

        // 2. Buscamos el turno, asegurándonos de que sea de ÉSTE usuario
        var turno = await _context.Turnos.FirstOrDefaultAsync(t => t.Id == id && t.IdUsuario == userId);

        if (turno == null || string.IsNullOrEmpty(turno.RutaPdfResultado))
        {
            TempData["MensajeInfo"] = "El resultado no está disponible o no te pertenece.";
            return RedirectToAction(nameof(Index));
        }

        // 3. Armamos la ruta física del archivo
        string filepath = Path.Combine(_env.WebRootPath, "resultados", turno.RutaPdfResultado);

        if (!System.IO.File.Exists(filepath))
        {
            TempData["MensajeInfo"] = "El archivo físico no se encontró en el servidor.";
            return RedirectToAction(nameof(Index));
        }

        // 4. Si todo está bien, le mandamos el PDF para que lo descargue
        byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(filepath);
        return File(fileBytes, "application/pdf", turno.RutaPdfResultado);
    }
}