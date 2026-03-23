# 🩸 IPHEM - Sistema Integral SaaS para Bancos de Sangre

![.NET](https://img.shields.io/badge/.NET-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-316192?style=for-the-badge&logo=postgresql&logoColor=white)
![Bootstrap](https://img.shields.io/badge/Bootstrap-563D7C?style=for-the-badge&logo=bootstrap&logoColor=white)
![Llama 3](https://img.shields.io/badge/AI-Llama_3.1-04242C?style=for-the-badge)

## 📝 Descripción del Proyecto
Plataforma SaaS (Software as a Service) desarrollada como Proyecto Final de Grado (Tecnicatura en Desarrollo de Software). Diseñada específicamente para modernizar y digitalizar el ciclo completo de atención del **Instituto Provincial de Hemoterapia (IPHEM)** de San Juan, Argentina.

El sistema elimina la dependencia del papel, agiliza los tiempos de espera y dota a la gerencia de herramientas de análisis de datos impulsadas por Inteligencia Artificial.

## ✨ Características Principales

### 1. Portal Público y CMS (Gestor de Contenidos)
* Interfaz pública responsiva orientada al ciudadano.
* Panel administrativo para publicar noticias, alertas de colectas urgentes y gestionar banners en tiempo real, sin intervención de IT.

### 2. CRM Médico y Autogestión del Donante
* **Enrutamiento Inteligente:** Redirección automática basada en roles (Claims Identity).
* **Gestión de Turnos:** Asignación dinámica de horarios validando cupos diarios y exclusión de fines de semana.
* **Triage Digital (Declaración Jurada):** Cuestionario clínico pre-donación con validación de firma electrónica del laboratorista.

### 3. Seguridad y Trazabilidad (Nivel Corporativo)
* **Túnel Seguro de Archivos:** Los análisis serológicos en PDF se almacenan fuera de la raíz pública y se sirven a través de un controlador que valida criptográficamente la identidad del usuario logueado, previniendo ataques de enumeración.
* **Caja Negra / Change Data Capture (CDC):** Interceptor a nivel de Entity Framework que registra en formato JSON el "antes y después" de cada modificación crítica en la base de datos para auditoría legal.
* **Hashing Criptográfico:** Contraseñas protegidas mediante BCrypt.

### 4. Analítica con Inteligencia Artificial
* Integración asíncrona con la API de **GroqCloud (Llama 3.1)**.
* Generación instantánea de reportes ejecutivos y campañas de marketing basándose exclusivamente en datos demográficos reales calculados en el backend (aplicación estricta de *Guardrails* para evitar alucinaciones).
* Exportación de informes oficiales a PDF mediante HTML5 Canvas y html2pdf.

## ⚙️ Arquitectura y Tecnologías
* **Backend:** ASP.NET Core MVC (Patrón Modelo-Vista-Controlador).
* **Capa de Datos:** Entity Framework Core (ORM) + PostgreSQL.
* **Frontend:** Vistas Razor (`.cshtml`), Bootstrap 5, Vanilla JS, Chart.js.
* **Autenticación:** Cookie Authentication + Roles (RBAC).

---
*Desarrollado por Rodrigo Tomás López Becerra.*
