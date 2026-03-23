using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using iphem_web.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;

//PARA LA IA
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace iphem_web.Controllers;

[Authorize]
public class AdminController : Controller
{
    private readonly IphemDbContext _context;

    public AdminController(IphemDbContext context)
    {
        _context = context;
    }

    // GET: /Admin
    // Tablero de Control (Dashboard)
    public async Task<IActionResult> Index()
    {
        // 1. Contamos los turnos PENDIENTES (Asumiendo que ID 1 es Pendiente)
        ViewBag.Pendientes = await _context.Turnos
            .Where(t => t.Idestadoturno == 1)
            .CountAsync();

        // 2. Contamos los turnos CONFIRMADOS (Asumiendo que ID 2 es Confirmado)
        ViewBag.Confirmados = await _context.Turnos
            .Where(t => t.Idestadoturno == 2)
            .CountAsync();

        // 3. Contamos los turnos de HOY
        var hoy = DateTime.Today;
        var manana = hoy.AddDays(1);
        
        ViewBag.TurnosHoy = await _context.Turnos
            .Where(t => t.Fechahora >= hoy && t.Fechahora < manana)
            .CountAsync();

        return View();
    }

    // GET: Admin/Turnos
    public async Task<IActionResult> Turnos(string? busqueda, string? filtro, string? orden)
    {
        ViewData["BusquedaActual"] = busqueda;

        var query = _context.Turnos
            .Include(t => t.IdestadoturnoNavigation)
            .Include(t => t.IdtipoturnoNavigation)
            .AsQueryable();

        // 1. BUSCADOR
        if (!string.IsNullOrEmpty(busqueda))
        {
            query = query.Where(t => t.Dnisolicitante.Contains(busqueda) ||
                                     t.Apellidosolicitante.ToLower().Contains(busqueda.ToLower()));
        }

        // 2. FILTRO (Turnos de Hoy)
        if (filtro == "hoy")
        {
            var fechaHoy = DateTime.Today;
            query = query.Where(t => t.Fechahora.Date == fechaHoy);
            ViewBag.FiltroActivo = "Turnos del día de hoy";
        }

        // 3. ORDENAMIENTO
        if (orden == "fecha_turno")
        {
            query = query.OrderBy(t => t.Fechahora); // Cronológico
            ViewBag.OrdenActual = "fecha_turno";
        }
        else
        {
            query = query.OrderByDescending(t => t.FechaCreacion); // Llegada reciente (Default)
            ViewBag.OrdenActual = "recientes";
        }

        var turnos = await query.ToListAsync();
        return View(turnos);
    }

    /* METODO GET VIEJO POR SI SE ROMPE EL NUEVO
    public async Task<IActionResult> Turnos(string busqueda)
    {
        // 1. Empezamos la consulta base (sin traer los datos todavía)
        var consulta = _context.Turnos
            .Include(t => t.IdestadoturnoNavigation)
            .Include(t => t.IdtipoturnoNavigation)
            .AsQueryable(); // Importante para poder agregar filtros dinámicos

        // 2. Si el usuario escribió algo, filtramos
        if (!string.IsNullOrEmpty(busqueda))
        {
            // CORRECCIÓN AQUÍ:
            // En lugar de .ToString(), usamos Convert.ToString() o el truco de concatenar ""
            // Esto le dice a Postgres: "Tratame este número como texto para poder buscar adentro"
            
            consulta = consulta.Where(t => 
                (t.Dnisolicitante + "").Contains(busqueda) || 
                t.Apellidosolicitante.ToLower().Contains(busqueda.ToLower()));
        }

        // 3. Ordenamos y ejecutamos
        var listaFinal = await consulta
            .OrderByDescending(t => t.Fechahora)
            .ToListAsync();

        // Guardamos lo que buscó para mostrarlo en la cajita de texto (que no se borre)
        ViewData["BusquedaActual"] = busqueda;
            
        return View(listaFinal);
    }*/
    
    // GET: Admin/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var turno = await _context.Turnos.FindAsync(id);
        if (turno == null) return NotFound();

        // Cargamos las listas para los desplegables
        // LO IMPORTANTE: Ahora cargamos "Idestadoturno" para que puedas cambiarlo
        ViewData["Idestadoturno"] = new SelectList(_context.Estadosturnos, "Id", "Nombre", turno.Idestadoturno);
        ViewData["Idtipoturno"] = new SelectList(_context.Tiposturnos, "Id", "Nombre", turno.Idtipoturno);
        
        return View(turno);
    }

    // POST: Admin/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Fechahora,Nombresolicitante,Apellidosolicitante,Dnisolicitante,Telefonosolicitante,Emailsolicitante,Idtipoturno,Idestadoturno,Observaciones,Pacientedestino")] Turno turno)
    {
        if (id != turno.Id) return NotFound();

        // Quitamos las validaciones de navegación para que no molesten
        ModelState.Remove("IdestadoturnoNavigation");
        ModelState.Remove("IdtipoturnoNavigation");

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(turno);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Turnos.Any(e => e.Id == id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Turnos)); // Volvemos a la lista
        }
        
        // Si falló, recargamos los desplegables
        ViewData["Idestadoturno"] = new SelectList(_context.Estadosturnos, "Id", "Nombre", turno.Idestadoturno);
        ViewData["Idtipoturno"] = new SelectList(_context.Tiposturnos, "Id", "Nombre", turno.Idtipoturno);
        return View(turno);
    }
    
    // --- SECCIÓN NOTICIAS ---

    // GET: Admin/Noticias
    // Listado de noticias para el administrador
    public async Task<IActionResult> Noticias()
    {
        return View(await _context.Noticias.OrderByDescending(n => n.Fechapublicacion).ToListAsync());
    }

    // GET: Admin/NuevaNoticia
    // Muestra el formulario vacío
    public IActionResult NuevaNoticia()
    {
        return View();
    }

    // POST: Admin/NuevaNoticia
    // Recibe los datos y crea la noticia real
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> NuevaNoticia([Bind("Titulo,Epigrafe,Cuerpo")] Noticia noticia, IFormFile? archivoImagen)
    {
        // Completamos los datos automáticos
        noticia.Fechapublicacion = DateTime.Now;
        noticia.Idusuariocreador = 1; // Por ahora hardcodeamos el Admin 1 (luego lo arreglaremos con el Login)

        // Limpiamos validaciones de relaciones que no usamos ahora
        ModelState.Remove("IdusuariocreadorNavigation");

        if (ModelState.IsValid)
        {
            // --- LÓGICA DE IMAGEN ---
            if (archivoImagen != null && archivoImagen.Length > 0)
            {
                // 1. Definir dónde se guarda
                // Usamos Path.Combine para que funcione en Linux y Windows
                string carpetasImagenes = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "noticias");

                // Crear carpeta si no existe
                if (!Directory.Exists(carpetasImagenes)) Directory.CreateDirectory(carpetasImagenes);

                // 2. Generar nombre único (Ej: a5f1-22b1-foto.jpg)
                string nombreUnico = Guid.NewGuid().ToString() + Path.GetExtension(archivoImagen.FileName);

                // 3. Ruta completa en el disco
                string rutaCompleta = Path.Combine(carpetasImagenes, nombreUnico);

                // 4. Guardar el archivo físicamente
                using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                {
                    await archivoImagen.CopyToAsync(stream);
                }

                // 5. Guardar SOLO EL NOMBRE en la base de datos
                noticia.ImagenPath = nombreUnico;
            }
            // ------------------------

            _context.Add(noticia);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Noticias));
        }
        return View(noticia);
    }
    
    // GET: Admin/BorrarNoticia/5
    public async Task<IActionResult> BorrarNoticia(int? id)
    {
        if (id == null) return NotFound();
        var noticia = await _context.Noticias.FindAsync(id);
        if (noticia == null) return NotFound();

        // Borramos directo (para hacerlo rápido)
        _context.Noticias.Remove(noticia);
        await _context.SaveChangesAsync();
        
        return RedirectToAction(nameof(Noticias));
    }
    
    // GET: Admin/EditarNoticia/5
    public async Task<IActionResult> EditarNoticia(int? id)
    {
        if (id == null) return NotFound();
        var noticia = await _context.Noticias.FindAsync(id);
        if (noticia == null) return NotFound();
        return View(noticia);
    }

    // POST: Admin/EditarNoticia/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarNoticia(int id, [Bind("Id,Titulo,Epigrafe,Cuerpo,ImagenPath,FechaPublicacion,IdUsuarioCreador")] Noticia noticia, IFormFile? archivoImagen)
    {
        if (id != noticia.Id) return NotFound();

        ModelState.Remove("IdusuariocreadorNavigation");

        if (ModelState.IsValid)
        {
            try
            {
                // Lógica de Imagen:
                // Si subió una nueva, la procesamos. Si no, dejamos la que viene en noticia.ImagenPath (gracias al input hidden)
                if (archivoImagen != null && archivoImagen.Length > 0)
                {
                    string carpetasImagenes = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "noticias");
                    if (!Directory.Exists(carpetasImagenes)) Directory.CreateDirectory(carpetasImagenes);

                    string nombreUnico = Guid.NewGuid().ToString() + Path.GetExtension(archivoImagen.FileName);
                    string rutaCompleta = Path.Combine(carpetasImagenes, nombreUnico);

                    using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                    {
                        await archivoImagen.CopyToAsync(stream);
                    }

                    // Borrar la imagen vieja del disco si querés ahorrar espacio (Opcional)
                    // ...

                    noticia.ImagenPath = nombreUnico; // Asignamos la nueva
                }
                
                _context.Update(noticia);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Noticias.Any(e => e.Id == id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Noticias));
        }
        return View(noticia);
    }
    
    // --- SECCIÓN CARRUSEL ---

    // GET: Admin/Carrusel
    public async Task<IActionResult> Carrusel()
    {
        return View(await _context.Carrusel.ToListAsync());
    }

    // GET: Admin/NuevoItemCarrusel
    public IActionResult NuevoItemCarrusel()
    {
        return View();
    }

    // POST: Admin/NuevoItemCarrusel
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> NuevoItemCarrusel(Carrusel item, IFormFile archivoImagen)
    {
        // La foto es obligatoria acá
        if (archivoImagen == null || archivoImagen.Length == 0)
        {
            ModelState.AddModelError("ImagenPath", "Debes subir una imagen para el carrusel.");
            return View(item);
        }

        // Guardamos la foto (misma lógica que noticias pero carpeta 'carrusel')
        string carpetasImagenes = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "carrusel");
        if (!Directory.Exists(carpetasImagenes)) Directory.CreateDirectory(carpetasImagenes);

        string nombreUnico = Guid.NewGuid().ToString() + Path.GetExtension(archivoImagen.FileName);
        string rutaCompleta = Path.Combine(carpetasImagenes, nombreUnico);

        using (var stream = new FileStream(rutaCompleta, FileMode.Create))
        {
            await archivoImagen.CopyToAsync(stream);
        }

        item.ImagenPath = nombreUnico;
        item.Activo = true;

        _context.Add(item);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Carrusel));
    }
    
    // DELETE: Admin/BorrarItemCarrusel/5
    public async Task<IActionResult> BorrarItemCarrusel(int id)
    {
        var item = await _context.Carrusel.FindAsync(id);
        if (item != null)
        {
            _context.Carrusel.Remove(item);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Carrusel));
    }
    
    // GET: Admin/EditarItemCarrusel/5
    public async Task<IActionResult> EditarItemCarrusel(int? id)
    {
        if (id == null) return NotFound();
        var item = await _context.Carrusel.FindAsync(id);
        if (item == null) return NotFound();
        return View(item);
    }

    // POST: Admin/EditarItemCarrusel/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarItemCarrusel(int id, Carrusel item, IFormFile? archivoImagen)
    {
        if (id != item.Id) return NotFound();

        // Validamos manualmente lo básico
        if (ModelState.IsValid)
        {
            try
            {
                // 1. Si sube imagen nueva, la reemplazamos
                if (archivoImagen != null && archivoImagen.Length > 0)
                {
                    string carpetasImagenes = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "carrusel");
                    string nombreUnico = Guid.NewGuid().ToString() + Path.GetExtension(archivoImagen.FileName);
                    string rutaCompleta = Path.Combine(carpetasImagenes, nombreUnico);

                    using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                    {
                        await archivoImagen.CopyToAsync(stream);
                    }
                    item.ImagenPath = nombreUnico;
                }
                
                // 2. Actualizamos la BD
                _context.Update(item);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Carrusel.Any(e => e.Id == id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Carrusel));
        }
        return View(item);
    }

    // GET: Admin/ConfiguracionTurnos
    public async Task<IActionResult> ConfiguracionTurnos()
    {
        // Buscamos la primera (y única) fila de configuración
        var config = await _context.ConfiguracionTurnos.FirstOrDefaultAsync();

        // Prevención de errores: si la tabla está vacía, creamos una por defecto
        if (config == null)
        {
            config = new ConfiguracionTurno {
                HoraInicio = new TimeSpan(8, 0, 0),
                CantidadTurnos = 20,
                IntervaloMinutos = 10
            };
            _context.Add(config);
            await _context.SaveChangesAsync();
        }

        return View(config);
    }

    // --- SECCIÓN CONFIGURACION TURNOS ---

    // POST: Admin/ConfiguracionTurnos
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfiguracionTurnos(ConfiguracionTurno config)
    {
        if (ModelState.IsValid)
        {
            _context.Update(config);
            await _context.SaveChangesAsync();

            // Usamos TempData para mostrar un mensajito de éxito en el Dashboard después
            TempData["Mensaje"] = "Configuración de turnos actualizada correctamente.";
            return RedirectToAction(nameof(Index)); // Volvemos al Dashboard
        }
        return View(config);
    }

    // --- SECCIÓN REPORTES ---

    // GET: Admin/Reportes
    public async Task<IActionResult> Reportes()
    {
        // 1. Agrupamos por Grupo Sanguíneo (ignoramos a los que eligieron "Prefiero no decirlo")
        var datosSangre = await _context.Turnos
            .Where(t => !string.IsNullOrEmpty(t.GrupoSanguineo))
            .GroupBy(t => t.GrupoSanguineo)
            .Select(g => new { Grupo = g.Key, Cantidad = g.Count() })
            .ToListAsync();

        // 2. Agrupamos por Departamento (ordenados de mayor a menor)
        var datosDepto = await _context.Turnos
            .Where(t => !string.IsNullOrEmpty(t.Departamento))
            .GroupBy(t => t.Departamento)
            .Select(g => new { Departamento = g.Key, Cantidad = g.Count() })
            .OrderByDescending(x => x.Cantidad)
            .ToListAsync();

        // Convertimos los datos a formato JSON para que JavaScript (Chart.js) los entienda como listas (Arrays)
        ViewBag.LabelsSangre = System.Text.Json.JsonSerializer.Serialize(datosSangre.Select(d => d.Grupo));
        ViewBag.ValoresSangre = System.Text.Json.JsonSerializer.Serialize(datosSangre.Select(d => d.Cantidad));

        ViewBag.LabelsDepto = System.Text.Json.JsonSerializer.Serialize(datosDepto.Select(d => d.Departamento));
        ViewBag.ValoresDepto = System.Text.Json.JsonSerializer.Serialize(datosDepto.Select(d => d.Cantidad));

        return View();
    }

    //INTELIGENCIA ARTIFICIAL

    // POST: /Admin/GenerarReporteIA
    [HttpPost]
    public async Task<IActionResult> GenerarReporteIA([FromBody] PeticionIA peticion)
    {
        // 1. Calculamos el TOTAL exacto en C# para que la IA no invente
        int totalDonantes = await _context.Turnos.CountAsync();

        var datosSangre = await _context.Turnos
            .Where(t => !string.IsNullOrEmpty(t.GrupoSanguineo))
            .GroupBy(t => t.GrupoSanguineo)
            .Select(g => $"{g.Key}: {g.Count()} donantes")
            .ToListAsync();

        var datosDepto = await _context.Turnos
            .Where(t => !string.IsNullOrEmpty(t.Departamento))
            .GroupBy(t => t.Departamento)
            .Select(g => $"{g.Key}: {g.Count()} donantes")
            .ToListAsync();

        string resumenSangre = string.Join(", ", datosSangre);
        string resumenDepto = string.Join(", ", datosDepto);

        if (totalDonantes == 0)
        {
            return Json(new { exito = false, mensaje = "No hay datos suficientes en la base." });
        }

        // 2. --- EL NUEVO CONTEXTO ESTRICTO (GUARDRAILS) ---
        string contexto = $@"DATOS ESTRICTOS DEL IPHEM SAN JUAN:
            - Total de donantes registrados: {totalDonantes}.
            - Por grupo sanguíneo: {resumenSangre}.
            - Por departamento: {resumenDepto}.
            REGLA ABSOLUTA: Usá ÚNICAMENTE estos números para tus cálculos. Tenés estrictamente prohibido inventar datos demográficos, asumir poblaciones de ciudades o agregar información externa. Si te piden un porcentaje, calculalo sobre el Total de donantes registrados ({totalDonantes}). ";

        string promptUsuario = "";

        if (!string.IsNullOrWhiteSpace(peticion.PreguntaLibre))
        {
            promptUsuario = contexto + $"PREGUNTA DEL ADMINISTRADOR: {peticion.PreguntaLibre}. Respondé de forma directa y profesional.";
        }
        else
        {
            switch (peticion.TipoReporte)
            {
                case "marketing":
                    promptUsuario = contexto + "Actuá como experto en marketing de salud. Escribí 3 ideas breves para campañas de donación enfocadas en los grupos o departamentos con menos donantes.";
                    break;
                case "demografico":
                    promptUsuario = contexto + "Actuá como sociólogo. Hacé un análisis rápido de la distribución de estos donantes por departamento en San Juan.";
                    break;
                default:
                    promptUsuario = contexto + "Escribí un reporte ejecutivo de 2 párrafos resumiendo estos datos y dando una recomendación general.";
                    break;
            }
        }

        // 3. --- EL FRENO DE MANO FINAL ---
        promptUsuario += "\nREGLA FINAL: Terminá tu respuesta inmediatamente después de dar la información. NO agregues subtítulos, NO agregues nuevas preguntas, ni texto extra de relleno.";

        // 4. --- CONEXIÓN CON GROQ (LA NUEVA MAGIA) ---
        try
        {
            string apiKey = "API_KEY";
            using var client = new System.Net.Http.HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            // Groq usa la misma estructura de mensajes que OpenAI
            var requestBody = new {
                model = "llama-3.1-8b-instant", // Modelo súper rápido y capaz
                messages = new[]
                {
                    new { role = "system", content = "Eres un analista de datos experto trabajando para el Ministerio de Salud (IPHEM). Responde siempre en español, con un tono muy formal e institucional. No uses formato Markdown complejo." },
                    new { role = "user", content = promptUsuario }
                },
                temperature = 0.5 // Mantenemos tu temperatura baja
            };

            var content = new System.Net.Http.StringContent(System.Text.Json.JsonSerializer.Serialize(requestBody), System.Text.Encoding.UTF8, "application/json");

            // Le pegamos a la API de Groq en vez de a tu máquina local
            var response = await client.PostAsync("https://api.groq.com/openai/v1/chat/completions", content);

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                using var jsonDoc = System.Text.Json.JsonDocument.Parse(responseString);

                // La ruta para extraer el texto en Groq es distinta a la de Ollama
                string textoGenerado = jsonDoc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content").GetString();

                return Json(new { exito = true, mensaje = textoGenerado });
            }

            var errorInfo = await response.Content.ReadAsStringAsync();
            return Json(new { exito = false, mensaje = "El servicio de Groq rechazó la solicitud: " + errorInfo });
        }
        catch (Exception ex)
        {
            return Json(new { exito = false, mensaje = "Error tecnico: " + ex.Message });
        }
    }

    //--AUDITORIA--

    // GET: Admin/Auditoria
    public async Task<IActionResult> Auditoria()
    {
        // Traemos los últimos 100 movimientos ordenados desde el más nuevo al más viejo
        var registros = await _context.Auditorias
            .OrderByDescending(a => a.FechaHora)
            .Take(100)
            .ToListAsync();

        return View(registros);
    }

    // QUE VA A RECIBIR LA IA: PREGUNTA LIBRE O ESTABLECIDA
    public class PeticionIA
    {
        public string? TipoReporte { get; set; }
        public string? PreguntaLibre { get; set; }
    }
}