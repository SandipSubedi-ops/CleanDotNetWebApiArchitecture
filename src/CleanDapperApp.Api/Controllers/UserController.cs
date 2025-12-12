using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CleanDapperApp.Models.Interfaces;
using CleanDapperApp.Models.UserModel;
using Dapper;
using System.Data;

namespace CleanDapperApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Requires authentication for all endpoints
    public class UserController : ControllerBase
    {
        private readonly IBaseRepository _baseRepository;

        public UserController(IBaseRepository baseRepository)
        {
            _baseRepository = baseRepository;
        }

        /// <summary>
        /// Get all users
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<UserResponseDto>), 200)]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var parameters = new DynamicParameters();
                
                var users = await _baseRepository.GetList<UserResponseDto>(
                    "sp_GetAllUsers",
                    parameters);

                return Ok(new { Success = true, Data = users });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UserDetailsDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetUserById(int id)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", id, DbType.Int32);

                var user = await _baseRepository.GetSingleData<UserDetailsDto>(
                    "sp_GetUserById",
                    parameters);

                if (user == null)
                {
                    return NotFound(new { Success = false, Message = "User not found" });
                }

                return Ok(new { Success = true, Data = user });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }

        /// <summary>
        /// Get users by email (search)
        /// </summary>
        [HttpGet("search")]
        [ProducesResponseType(typeof(List<UserResponseDto>), 200)]
        public async Task<IActionResult> SearchUsersByEmail([FromQuery] string email)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@Email", email, DbType.String);

                var users = await _baseRepository.GetList<UserResponseDto>(
                    "sp_SearchUsersByEmail",
                    parameters);

                return Ok(new { Success = true, Data = users });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(UserResponseDto), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto createUserDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { Success = false, Message = "Invalid data", Errors = ModelState });
                }

                var parameters = new DynamicParameters();
                parameters.Add("@Username", createUserDto.Username, DbType.String);
                parameters.Add("@Email", createUserDto.Email, DbType.String);
                parameters.Add("@PhoneNumber", createUserDto.PhoneNumber, DbType.String);
                parameters.Add("@Address", createUserDto.Address, DbType.String);
                parameters.Add("@UserId", dbType: DbType.Int32, direction: ParameterDirection.Output);

                var result = await _baseRepository.PostandGetSingleData<UserResponseDto>(
                    "sp_CreateUser",
                    parameters);

                var newUserId = parameters.Get<int>("@UserId");

                return CreatedAtAction(
                    nameof(GetUserById),
                    new { id = newUserId },
                    new { Success = true, Message = "User created successfully", Data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing user
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(UserResponseDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto updateUserDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { Success = false, Message = "Invalid data", Errors = ModelState });
                }

                if (id != updateUserDto.Id)
                {
                    return BadRequest(new { Success = false, Message = "ID mismatch" });
                }

                var parameters = new DynamicParameters();
                parameters.Add("@UserId", id, DbType.Int32);
                parameters.Add("@Username", updateUserDto.Username, DbType.String);
                parameters.Add("@Email", updateUserDto.Email, DbType.String);
                parameters.Add("@PhoneNumber", updateUserDto.PhoneNumber, DbType.String);
                parameters.Add("@Address", updateUserDto.Address, DbType.String);
                parameters.Add("@RowsAffected", dbType: DbType.Int32, direction: ParameterDirection.Output);

                var result = await _baseRepository.PostandGetSingleData<UserResponseDto>(
                    "sp_UpdateUser",
                    parameters);

                var rowsAffected = parameters.Get<int>("@RowsAffected");

                if (rowsAffected == 0)
                {
                    return NotFound(new { Success = false, Message = "User not found" });
                }

                return Ok(new { Success = true, Message = "User updated successfully", Data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }

        /// <summary>
        /// Delete a user (soft delete - set IsActive = false)
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", id, DbType.Int32);
                parameters.Add("@RowsAffected", dbType: DbType.Int32, direction: ParameterDirection.Output);

                await _baseRepository.PostandGetSingleData<int>(
                    "sp_DeleteUser",
                    parameters);

                var rowsAffected = parameters.Get<int>("@RowsAffected");

                if (rowsAffected == 0)
                {
                    return NotFound(new { Success = false, Message = "User not found" });
                }

                return Ok(new { Success = true, Message = "User deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }

        /// <summary>
        /// Get active users only
        /// </summary>
        [HttpGet("active")]
        [ProducesResponseType(typeof(List<UserResponseDto>), 200)]
        public async Task<IActionResult> GetActiveUsers()
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@IsActive", true, DbType.Boolean);

                var users = await _baseRepository.GetList<UserResponseDto>(
                    "sp_GetActiveUsers",
                    parameters);

                return Ok(new { Success = true, Data = users });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }

        /// <summary>
        /// Get user statistics (using multiple result sets)
        /// </summary>
        [HttpGet("{id}/statistics")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> GetUserStatistics(int id)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", id, DbType.Int32);

                var results = await _baseRepository.GetMultipleData(
                    "sp_GetUserStatistics",
                    parameters);

                // Assuming the stored procedure returns:
                // Result Set 1: User basic info
                // Result Set 2: User orders summary
                // Result Set 3: User recent activities

                return Ok(new
                {
                    Success = true,
                    Data = new
                    {
                        UserInfo = results.Count > 0 ? results[0] : null,
                        OrdersSummary = results.Count > 1 ? results[1] : null,
                        RecentActivities = results.Count > 2 ? results[2] : null
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }

        /// <summary>
        /// Example using transaction - Create user with initial order
        /// </summary>
        [HttpPost("with-order")]
        [ProducesResponseType(201)]
        public async Task<IActionResult> CreateUserWithOrder([FromBody] CreateUserDto createUserDto)
        {
            IDbTransaction transaction = null;
            try
            {
                transaction = _baseRepository.BeginTransaction();

                // Create user
                var userParams = new DynamicParameters();
                userParams.Add("@Username", createUserDto.Username, DbType.String);
                userParams.Add("@Email", createUserDto.Email, DbType.String);
                userParams.Add("@PhoneNumber", createUserDto.PhoneNumber, DbType.String);
                userParams.Add("@Address", createUserDto.Address, DbType.String);
                userParams.Add("@UserId", dbType: DbType.Int32, direction: ParameterDirection.Output);

                await _baseRepository.PostandGetSingleData<UserResponseDto>(
                    "sp_CreateUser",
                    userParams,
                    transaction);

                var newUserId = userParams.Get<int>("@UserId");

                // Create welcome order
                var orderParams = new DynamicParameters();
                orderParams.Add("@UserId", newUserId, DbType.Int32);
                orderParams.Add("@OrderType", "Welcome", DbType.String);

                await _baseRepository.PostandGetSingleData<int>(
                    "sp_CreateWelcomeOrder",
                    orderParams,
                    transaction);

                transaction.Commit();

                return CreatedAtAction(
                    nameof(GetUserById),
                    new { id = newUserId },
                    new { Success = true, Message = "User and welcome order created successfully" });
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
            finally
            {
                transaction?.Dispose();
            }
        }
    }
}
