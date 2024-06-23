using Developers.Models.ViewModels;
using Developers.Repositories.Interfaces;
using Developers.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace Developers.Controllers;

public class ClassroomsController : Controller
{
    private readonly IUnitWork _unitWork;
    [BindProperty]
    public ClassroomVM classroomVM { get; set; }
    public ClassroomsController(IUnitWork unitWork)
    {
        _unitWork = unitWork;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Create()
    {
        ClassroomVM classroomVM = new ClassroomVM()
        {
            Classroom = new Models.Classroom(){ SessionDate= DateTime.Now },            
            CourseList = _unitWork.Classroom.ObtenerTodosDropdownLista("Course"),
            TrainerList = _unitWork.Classroom.ObtenerTodosDropdownLista("Trainer")
        };

        return View(classroomVM);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ClassroomVM classroomVM)
    {
        if (classroomVM is null) return NotFound();
        if (ModelState.IsValid)
        {
            await _unitWork.Classroom.AgregarAsync(classroomVM.Classroom);
            await _unitWork.GuardarAsync();
            TempData[DS.Successfull] = "Sesión creada correctamente.";
            return RedirectToAction("Index");
        }
        classroomVM.CourseList = _unitWork.Classroom.ObtenerTodosDropdownLista("Course");
        classroomVM.TrainerList = _unitWork.Classroom.ObtenerTodosDropdownLista("Trainer");
        TempData[DS.Error] = "Error al guardar la sesión, intente de nuevo.";
        return View(classroomVM);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int? id)
    {
        if(id is null) return NotFound();
        classroomVM = new ClassroomVM();
        classroomVM.Classroom = await _unitWork.Classroom.ObtenerPrimeroAsync(filter: c => c.ClassroomId == id, includeProperties:"Trainer,Course");
        classroomVM.Enrollments = await _unitWork.Enrollment.ObtenerTodosAsync(filter: e => e.ClassroomId == id, includeProperties:"Student");

        return View(classroomVM);
    }

    [HttpPost]
    public async Task<IActionResult> Details(int classroomId, int studentId)
    {
        classroomVM = new ClassroomVM();
        classroomVM.Classroom = await _unitWork.Classroom.ObtenerPrimeroAsync(filter: c => c.ClassroomId == classroomId) ;
        var enrollment = await _unitWork.Enrollment.ObtenerPrimeroAsync(e => e.ClassroomId == classroomId && e.StudentId == studentId);

        // Si el estudiante está agregado, retornar un mensaje
        if(enrollment is null)
        {
            classroomVM.Enrollment = new Models.Enrollment();
            classroomVM.Enrollment.StudentId = studentId;
            classroomVM.Enrollment.ClassroomId = classroomId;

            await _unitWork.Enrollment.AgregarAsync(classroomVM.Enrollment);
            await _unitWork.GuardarAsync();
            TempData[DS.Successfull] = "Participante agregado correctamente";
        }
        else
        {
            TempData[DS.Error] ="Error al agregar el participante, intente de nuevo";
        }

        return RedirectToAction("Details", new { id = classroomId});
    }

    [HttpPost]
    public async Task<IActionResult> DeleteStudent(int classroomId, int studentId)
    {
        try
        {
            // Buscar la matrícula del estudiante en la lista de matrículas del salón de clases
            var enrollment = await _unitWork.Enrollment.ObtenerPrimeroAsync(e => e.ClassroomId == classroomId && e.StudentId == studentId);

            if (enrollment != null)
            {
                // Eliminar la matrícula del estudiante del salón de clases (en memoria)
                _unitWork.Enrollment.Remover(enrollment);
                await _unitWork.GuardarAsync();

                TempData[DS.Successfull] = "Estudiante eliminado correctamente";

                // Devolver un JSON indicando que la eliminación fue exitosa
                return Json(new { success = true });
            }
            else
            {
                // Si la matrícula no se encuentra, devolver un mensaje de error
                return Json(new { success = false, message = "La matrícula del estudiante no se encontró en el salón de clases." });
            }
        }
        catch (Exception ex)
        {
            // Manejar cualquier excepción que pueda ocurrir durante el proceso de eliminación
            return Json(new { success = false, message = "Ocurrió un error al eliminar la matrícula del estudiante.", error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> EditarNotas(int studentId)
    {
        try
        {
            var enrollment = await _unitWork.Enrollment.ObtenerPrimeroAsync(e => e.StudentId == studentId);

            if (enrollment != null)
            {
                return Json(new
                {
                    success = true,
                    enrollmentId = enrollment.EnrollmentId,
                    studentId = enrollment.StudentId,
                    pretest = enrollment.PreTest,
                    posttest = enrollment.PostTest
                });
            }
            else
            {
                return Json(new { success = false, message = "Estudiante no encontrado." });
            }
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Error al cargar los datos del estudiante.", error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> EditarNotas(int studentId, decimal pretest, decimal posttest)
    {
        try
        {
            var enrollment = await _unitWork.Enrollment.ObtenerPrimeroAsync(e => e.StudentId == studentId);

            if (enrollment != null)
            {
                enrollment.PreTest = pretest;
                enrollment.PostTest = posttest;

                _unitWork.Enrollment.Actualizar(enrollment);
                await _unitWork.GuardarAsync();

                TempData[DS.Successfull] = "Notas Actualizadas Correctamente";

                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, message = "Estudiante no encontrado." });
            }
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Error al actualizar las notas.", error = ex.Message });
        }
    }

    /// <summary>
    /// Buscar participante segun valor ingresado
    /// </summary>
    /// <param name="term"></param>
    /// <returns></returns>

    [HttpGet]
    public async Task<IActionResult> SearchStudent(string term)
    {
        if (!string.IsNullOrEmpty(term))
        {
            var productsList = await _unitWork.Student.ObtenerTodosAsync(p => p.Status == true);
            var data = productsList.Where(x => x.Dni.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                                            x.FirstName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                                            x.LastName.Contains(term, StringComparison.OrdinalIgnoreCase)).ToList();
            return Json(data);
        }
        return Ok();
    }

    #region API para Javascript
    /// <summary>
    /// Listar todas las sesiones registradas
    /// </summary>
    /// <returns>Json</returns>
    [HttpGet]
    public async Task<IActionResult> ListarTodos()
    {
        var classrooms = await _unitWork.Classroom.ObtenerTodosAsync(
            includeProperties: "Trainer,Course",
            orderBy: c => c.OrderByDescending(c => c.ClassroomId),
            isTracking: false);

        return Json(new { data = classrooms });
    }
    #endregion
}
