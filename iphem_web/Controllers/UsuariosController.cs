using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using iphem_web.Models;
using Microsoft.AspNetCore.Authorization;

namespace iphem_web.Controllers;

[Authorize] // Solo admins pueden entrar acá
public class UsuariosController : Controller
{
    private readonly IphemDbContext _context;

    public UsuariosController(IphemDbContext context)
    {
        _context = context;
    }

    // GET: Usuarios
    // Lista de usuarios del sistema
    public async Task<IActionResult> Index()
    {
        return View(await _context.Usuarios.Include(u => u.IdrolNavigation).ToListAsync());
    }

    // GET: Usuarios/Create
    public IActionResult Create()
    {
        // Necesitamos cargar los roles (Admin, Médico, etc.) para el desplegable
        ViewData["Idrol"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Roles, "Id", "Nombre");
        return View();
    }

    // POST: Usuarios/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Nombrecompleto,Email,Passwordhash,Idrol")] Usuario usuario)
    {
        // Validamos que el email no exista ya
        if (_context.Usuarios.Any(u => u.Email == usuario.Email))
        {
            ModelState.AddModelError("Email", "Este correo ya está registrado.");
        }

        // Removemos validaciones de navegación
        ModelState.Remove("IdrolNavigation");

        if (ModelState.IsValid)
        {
            // --- LA MAGIA OCURRE ACÁ ---
            // Tomamos lo que escribió (ej: "pepe123") y lo reemplazamos por el hash
            usuario.Passwordhash = BCrypt.Net.BCrypt.HashPassword(usuario.Passwordhash);
            
            usuario.Fechaalta = DateTime.Now; // Fecha automática

            _context.Add(usuario);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Si falló, recargamos la lista de roles
        ViewData["Idrol"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Roles, "Id", "Nombre", usuario.Idrol);
        return View(usuario);
    }
}