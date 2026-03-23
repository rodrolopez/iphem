using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Necesario para la BD
using iphem_web.Models;
using System.Diagnostics;

namespace iphem_web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IphemDbContext _context; // Agregamos el contexto

    // Inyectamos la base de datos en el constructor
    public HomeController(ILogger<HomeController> logger, IphemDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        // --- PARTE 1: LO QUE YA TENÍAS (Noticias) ---
        // Esto alimenta la grilla de novedades de abajo.
        var ultimasNoticias = await _context.Noticias
            .OrderByDescending(n => n.Fechapublicacion) // Ojo: asegurate si es 'Fechapublicacion' o 'FechaPublicacion' en tu modelo
            .Take(6)
            .ToListAsync();

        // --- PARTE 2: LO NUEVO (Carrusel) ---
        // Esto busca las fotos activas en la tabla nueva.
        var itemsCarrusel = await _context.Carrusel
            .Where(c => c.Activo == true)
            .ToListAsync();

        // --- PARTE 3: EMPAQUETADO ---
        // Las noticias van como MODELO principal (return View(ultimasNoticias))
        // El carrusel va en la "mochila" ViewBag para usarlo arriba.
        ViewBag.Carrusel = itemsCarrusel;

        return View(ultimasNoticias);
    }

    // GET: /Home/Institucional
    public IActionResult Institucional()
    {
        return View();
    }
    
    public IActionResult Privacy()
    {
        return View();
    }
    
    // 1. Ver una noticia completa
    public async Task<IActionResult> Noticia(int? id)
    {
        if (id == null) return NotFound();

        var noticia = await _context.Noticias
            .Include(n => n.IdusuariocreadorNavigation) // Por si querés mostrar el autor
            .FirstOrDefaultAsync(m => m.Id == id);

        if (noticia == null) return NotFound();

        return View(noticia);
    }

    // 2. Página de Requisitos (Estática)
    public IActionResult Requisitos()
    {
        return View();
    }

    // 3. Página de Dónde Donar (Estática)
    public IActionResult DondeDonar()
    {
        return View();
    }

    // 4. Ver TODAS las noticias (Listado completo)
    public async Task<IActionResult> Novedades()
    {
        var noticias = await _context.Noticias
            .OrderByDescending(n => n.Fechapublicacion)
            .ToListAsync();
        return View(noticias);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}