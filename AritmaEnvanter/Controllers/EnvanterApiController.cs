using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AritmaEnvanter.Data;

[Route("api/[controller]")]
[ApiController]
public class EnvanterApiController : ControllerBase
{
    private readonly AppDbContext _context;

    public EnvanterApiController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetEnvanter()
    {
        var veriler = await _context.Set<AritmaEnvanter.Models.Entities.DepoStok>().ToListAsync(); 
        return Ok(veriler);
    }
}
