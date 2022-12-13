using JwtWebApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JwtWebApi.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/employee")]
    [ApiController]
    public class EmployeeController : Controller
    {
        private readonly DatabaseContext _db;

        public EmployeeController(DatabaseContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Employee>>> Get()
        {
            var employees = await _db.Employees.ToListAsync();
            return Ok(employees);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Employee>> Get(int id)
        {
            var employee = await _db.Employees.FirstOrDefaultAsync(x => x.EmployeeID == id);
            if (employee == null)
            {
                return NotFound();
            }
            return Ok(employee);
        }

        [HttpPost]
        public async Task<ActionResult<Employee>> Post(Employee employee)
        {
            _db.Employees.AddAsync(employee);
            await _db.SaveChangesAsync();
            return Ok(employee);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Employee>> Put(int id, Employee employee)
        {
            if (id != employee.EmployeeID)
            {
                return BadRequest();
            }

            if (await EmployeeExists(id) == false)
            {
                return NotFound();
            }

            _db.Entry(employee).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return Ok(employee);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<bool>> Delete(int id)
        {
            var employee = await _db.Employees.FirstOrDefaultAsync(x => x.EmployeeID == id);

            if (employee == null)
            {
                return NotFound();
            }

            _db.Employees.Remove(employee);
            return Ok(await _db.SaveChangesAsync() > 0);
        }

        private async Task<bool> EmployeeExists(int id)
        {
            return await _db.Employees.AnyAsync(x => x.EmployeeID == id);
        }

    }
}
