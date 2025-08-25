using Microsoft.AspNetCore.Mvc;
using OrangeHrmApi.Models.DTOs;
using OrangeHrmApi.Services;

namespace OrangeHrmApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrangeHrmController : ControllerBase
    {
        private readonly IOrangeHrmService _orangeHrmService;
        private readonly ILogger<OrangeHrmController> _logger;

        public OrangeHrmController(IOrangeHrmService orangeHrmService, ILogger<OrangeHrmController> logger)
        {
            _orangeHrmService = orangeHrmService;
            _logger = logger;
        }

        [HttpPost("employees")]
        public async Task<ActionResult<ApiResponse<EmployeeResponse>>> AddEmployee([FromBody] AddEmployeeRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return BadRequest(new ApiResponse<EmployeeResponse>
                    {
                        Success = false,
                        ErrorMessage = string.Join("; ", errors)
                    });
                }

                if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
                {
                    return BadRequest(new ApiResponse<EmployeeResponse>
                    {
                        Success = false,
                        ErrorMessage = "FirstName and LastName are required"
                    });
                }

                var (success, result, error) = await _orangeHrmService.AddEmployeeAsync(request);

                if (!success)
                {
                    if (error?.Contains("already exists") == true)
                    {
                        return Conflict(new ApiResponse<EmployeeResponse>
                        {
                            Success = false,
                            ErrorMessage = error
                        });
                    }

                    if (error?.Contains("not found") == true)
                    {
                        return BadRequest(new ApiResponse<EmployeeResponse>
                        {
                            Success = false,
                            ErrorMessage = error
                        });
                    }

                    return StatusCode(500, new ApiResponse<EmployeeResponse>
                    {
                        Success = false,
                        ErrorMessage = "Internal server error"
                    });
                }

                return Ok(new ApiResponse<EmployeeResponse>
                {
                    Success = true,
                    Data = new EmployeeResponse { EmployeeId = result! }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in AddEmployee");
                return StatusCode(500, new ApiResponse<EmployeeResponse>
                {
                    Success = false,
                    ErrorMessage = "Internal server error"
                });
            }
        }

        [HttpPost("claims")]
        public async Task<ActionResult<ApiResponse<ClaimResponse>>> CreateClaim([FromBody] CreateClaimRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return BadRequest(new ApiResponse<ClaimResponse>
                    {
                        Success = false,
                        ErrorMessage = string.Join("; ", errors)
                    });
                }

                if (string.IsNullOrWhiteSpace(request.EmployeeId) && string.IsNullOrWhiteSpace(request.EmployeeName))
                {
                    return BadRequest(new ApiResponse<ClaimResponse>
                    {
                        Success = false,
                        ErrorMessage = "Either EmployeeId or EmployeeName must be provided"
                    });
                }

                if (string.IsNullOrWhiteSpace(request.Event) || string.IsNullOrWhiteSpace(request.Currency))
                {
                    return BadRequest(new ApiResponse<ClaimResponse>
                    {
                        Success = false,
                        ErrorMessage = "Event and Currency are required"
                    });
                }

                var (success, result, error) = await _orangeHrmService.CreateClaimAsync(request);

                if (!success)
                {
                    if (error?.Contains("not found") == true)
                    {
                        return BadRequest(new ApiResponse<ClaimResponse>
                        {
                            Success = false,
                            ErrorMessage = error
                        });
                    }

                    return StatusCode(500, new ApiResponse<ClaimResponse>
                    {
                        Success = false,
                        ErrorMessage = "Internal server error"
                    });
                }

                return Ok(new ApiResponse<ClaimResponse>
                {
                    Success = true,
                    Data = new ClaimResponse { ReferenceId = result! }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in CreateClaim");
                return StatusCode(500, new ApiResponse<ClaimResponse>
                {
                    Success = false,
                    ErrorMessage = "Internal server error"
                });
            }
        }
    }
}
