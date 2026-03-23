using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using iphem_web.Models;
using Microsoft.EntityFrameworkCore;

namespace iphem_web.Controllers;

public class AccesoController : Controller
{
    private readonly IphemDbContext _context;

    public AccesoController(IphemDbContext context)
    {
        _context = context;
    }

    // GET: Acceso/Login
    public IActionResult Login()
    {
        if (User.Identity!.IsAuthenticated)
        {
            // Si ya está logueado, vemos qué rol tiene para mandarlo a su casa
            if (User.IsInRole("Administrador")) return RedirectToAction("Index", "Admin");
            return RedirectToAction("Index", "Donante"); // Futuro panel del donante
        }
        return View();
    }

    // POST: Acceso/Login
    [HttpPost]
    public async Task<IActionResult> Login(string identificador, string password)
    {
        if (string.IsNullOrEmpty(identificador) || string.IsNullOrEmpty(password))
        {
            ViewBag.Error = "Por favor, complete todos los campos.";
            return View();
        }

        // 1. Limpieza Inteligente
        string paramLimpio = identificador.Trim().ToLower();
        if (!paramLimpio.Contains("@"))
        {
            // Solo le sacamos los puntos si NO es un correo electrónico (ej: DNI)
            paramLimpio = paramLimpio.Replace(".", "");
        }

        // ¡A la contraseña NO le sacamos los puntos!
        string passLimpio = password.Trim();

        // 2. Buscamos por DNI o Email en la base de datos
        var usuario = await _context.Usuarios
            .Include(u => u.IdrolNavigation)
            .FirstOrDefaultAsync(u => u.Dni == paramLimpio || (u.Email != null && u.Email.ToLower() == paramLimpio));

        if (usuario != null)
        {
            bool esValida = false;
            try
            {
                // Verificación segura con BCrypt
                esValida = BCrypt.Net.BCrypt.Verify(passLimpio, usuario.Passwordhash);
            }
            catch(Exception)
            {
                // Auto-curación: Si la clave en la base de datos está en texto plano (como admin123)
                if (usuario.Passwordhash == passLimpio)
                {
                    esValida = true; // Lo dejamos pasar

                    // Y le encriptamos la clave para el futuro
                    usuario.Passwordhash = BCrypt.Net.BCrypt.HashPassword(passLimpio);
                    _context.Usuarios.Update(usuario);
                    await _context.SaveChangesAsync();
                }
            }

            if (esValida)
            {
                string nombreRol = usuario.IdrolNavigation?.Nombre ?? "Donante";

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, usuario.Nombrecompleto),
                    new Claim(ClaimTypes.Email, usuario.Email ?? ""),
                    new Claim(ClaimTypes.Role, nombreRol),
                    new Claim("IdUsuario", usuario.Id.ToString())
                };

                if (!string.IsNullOrEmpty(usuario.Dni))
                {
                    claims.Add(new Claim("DNI", usuario.Dni));
                }

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity));

                // Redirección inteligente según el rol del usuario
                if (nombreRol == "Administrador")
                {
                    return RedirectToAction("Index", "Admin");
                }
                else if (nombreRol == "Medico")
                {
                    return RedirectToAction("Index", "Medico");
                }
                else
                {
                    return RedirectToAction("Index", "Donante");
                }
            }
        }

        ViewBag.Error = "DNI, Email o contraseña incorrectos.";
        return View();
    }

    // GET: Acceso/Logout
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login", "Acceso");
    }
}