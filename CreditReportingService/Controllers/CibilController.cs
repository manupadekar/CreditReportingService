using CreditReporting.Application.DTOs;
using CreditReporting.Application.Interfaces;
using CreditReporting.Application.Services;
using CreditReportingService.Response;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CreditReportingService.Controllers
{
    [ApiController]
    [Route("api/cibil")]
    public class CibilController(ICibilService cibilService) : ControllerBase
    {
        private readonly ICibilService _cibilService = cibilService;



        [HttpPost("check")]
        public async Task<IActionResult> Create([FromBody] CibilCheckRequest reportDto)
        {
            var result = await _cibilService.CreateAsync(reportDto);
            return CreatedAtAction(nameof(GetByCustomerId), new { customerId = result.CustomerId }, ApiResponse<CibilReportDto>.SuccessResponse(result, "CIBIL report generated successfully."));
        }

        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetByCustomerId(int customerId)
        {
            var result = await _cibilService.GetByCustomerIdAsync(customerId);
            if (result == null)
                return NotFound(ApiResponse<CibilReportDto>.FailureResponse("CIBIL report not found for the given Customer ID."));

            return Ok(ApiResponse<CibilReportDto>.SuccessResponse(result));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _cibilService.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<CibilReportDto>>.SuccessResponse(result));
        }

       

        [HttpDelete("reports/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _cibilService.DeleteAsync(id);
            return Ok(ApiResponse<bool>.SuccessResponse(true, "Report deleted successfully."));
        }
    }
}
