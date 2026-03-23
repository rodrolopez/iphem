using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using iphem_web.Models;
using Microsoft.AspNetCore.Hosting; // Agregamos esto para los archivos

namespace iphem_web.Controllers;

[Authorize(Roles = "Medico")]
public class MedicoController : Controller
{
    private readonly IphemDbContext _context;
    private readonly IWebHostEnvironment _env; // Herramienta para rutas de archivos

    // Actualizamos el constructor para inyectar el IWebHostEnvironment
    public MedicoController(IphemDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    // GET: Medico/Index (El Dashboard del Médico)
    public async Task<IActionResult> Index()
    {
        // Contamos cuántos triages lo están esperando para mostrarle una alerta rápida
        var pendientes = await _context.Turnos
            .Include(t => t.TriageNavigation)
            .Include(t => t.IdestadoturnoNavigation)
            .Where(t => t.TriageNavigation != null && t.IdestadoturnoNavigation.Nombre == "Pendiente")
            .CountAsync();

        ViewBag.TriagesPendientes = pendientes;
        return View();
    }

    // GET: Medico/TriagesPendientes
    public async Task<IActionResult> TriagesPendientes()
    {
        var turnosParaEvaluar = await _context.Turnos
            .Include(t => t.UsuarioNavigation)
            .Include(t => t.TriageNavigation)
            .Include(t => t.IdestadoturnoNavigation)
            .Where(t => t.TriageNavigation != null && t.IdestadoturnoNavigation.Nombre == "Pendiente")
            .OrderBy(t => t.Fechahora)
            .ToListAsync();

        return View(turnosParaEvaluar);
    }

    // GET: Medico/VerTriage/5
    public async Task<IActionResult> VerTriage(int id)
    {
        var turno = await _context.Turnos
            .Include(t => t.UsuarioNavigation)
            .Include(t => t.TriageNavigation)
            .Include(t => t.IdestadoturnoNavigation)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (turno == null || turno.TriageNavigation == null)
        {
            TempData["Error"] = "El turno no existe o no tiene Triage.";
            return RedirectToAction(nameof(TriagesPendientes));
        }

        return View(turno);
    }

    // POST: Medico/EvaluarTriage
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EvaluarTriage(int id, string accion, string? motivoRechazo)
    {
        var turno = await _context.Turnos.FindAsync(id);
        if (turno == null) return NotFound();

        if (accion == "Aprobar")
        {
            var estadoConfirmado = await _context.Estadosturnos.FirstOrDefaultAsync(e => e.Nombre == "Confirmado");
            if (estadoConfirmado != null) turno.Idestadoturno = estadoConfirmado.Id;
            TempData["MensajeExito"] = "Triage aprobado. El turno ahora está Confirmado.";
        }
        else if (accion == "Rechazar")
        {
            var estadoCancelado = await _context.Estadosturnos.FirstOrDefaultAsync(e => e.Nombre == "Cancelado");
            if (estadoCancelado != null) turno.Idestadoturno = estadoCancelado.Id;

            if (!string.IsNullOrEmpty(motivoRechazo))
            {
                turno.Observaciones = motivoRechazo;
            }

            TempData["MensajeError"] = "Triage rechazado. El turno fue cancelado y el donante será notificado.";
        }

        // Capturamos el email o nombre del Laboratorista que está logueado haciendo esta acción
        string firmaClinica = User.Identity?.Name
                              ?? User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
                              ?? "Laboratorista Titular";

        // Se lo asignamos al turno
        turno.FirmaLaboratorista = firmaClinica;

        _context.Turnos.Update(turno);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(TriagesPendientes));
    }
    // ==============================================================
    // CARGA DE RESULTADOS CLÍNICOS
    // ==============================================================

    // 1. Bandeja de turnos listos para cargar resultados (Solo Confirmados)
    public async Task<IActionResult> CargaResultados()
    {
        var turnosConfirmados = await _context.Turnos
            .Include(t => t.UsuarioNavigation)
            .Include(t => t.IdestadoturnoNavigation)
            .Where(t => t.IdestadoturnoNavigation.Nombre == "Confirmado" && t.RutaPdfResultado == null)
            .OrderByDescending(t => t.Fechahora)
            .ToListAsync();

        return View(turnosConfirmados);
    }

    // 2. GET: Formulario para subir el archivo
    public async Task<IActionResult> SubirResultado(int id)
    {
        var turno = await _context.Turnos
            .Include(t => t.UsuarioNavigation)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (turno == null) return NotFound();

        return View(turno);
    }

    // 3. POST: Procesa el archivo y lo guarda
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubirResultado(int id, string notasLaboratorio, IFormFile archivoPdf)
    {
        var turno = await _context.Turnos.FindAsync(id);
        if (turno == null) return NotFound();

        // Validamos que efectivamente haya subido un archivo y que sea PDF
        if (archivoPdf != null && archivoPdf.Length > 0)
        {
            if (archivoPdf.ContentType != "application/pdf")
            {
                TempData["MensajeError"] = "El archivo debe ser un documento PDF.";
                return RedirectToAction(nameof(SubirResultado), new { id = turno.Id });
            }

            // Creamos la carpeta "resultados" adentro de wwwroot si no existe
            string uploadsFolder = Path.Combine(_env.WebRootPath, "resultados");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Generamos un nombre único para el archivo (Ej: Resultado_DNI_Fecha.pdf)
            string uniqueFileName = $"Resultado_{turno.Dnisolicitante}_{DateTime.Now.ToString("yyyyMMddHHmmss")}.pdf";
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Copiamos el archivo al servidor (Tu máquina Ubuntu por ahora)
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await archivoPdf.CopyToAsync(fileStream);
            }

            // Guardamos el nombre del archivo y las notas en la base de datos
            turno.RutaPdfResultado = uniqueFileName;
            turno.NotasLaboratorio = notasLaboratorio;

            // Capturamos el email o nombre del Laboratorista que está logueado haciendo esta acción
            string firmaClinica = User.Identity?.Name
                                  ?? User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
                                  ?? "Laboratorista Titular";

            // Se lo asignamos al turno
            turno.FirmaLaboratorista = firmaClinica;

            _context.Turnos.Update(turno);
            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = "Resultado cargado exitosamente. El donante ya puede visualizarlo.";
            return RedirectToAction(nameof(CargaResultados));
        }

        TempData["MensajeError"] = "Por favor, seleccione un archivo válido.";
        return RedirectToAction(nameof(SubirResultado), new { id = turno.Id });
    }
}