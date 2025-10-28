using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace EmployeeService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RedisTestController : ControllerBase
    {
        private readonly IDistributedCache _cache;
        public RedisTestController(IDistributedCache cache)
        {
            _cache = cache;
        }

        [HttpPost("store")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> StoreValue([FromQuery] string key, [FromQuery] string value)
        {
            await _cache.SetStringAsync(key, value, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            });
            return Ok($"Value '{value}' stored with key '{key}'.");
        }

        [HttpGet("get")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetValue([FromQuery] string key)
        {
            var value = await _cache.GetStringAsync(key);
            return Ok(value ?? "(null)");
        }
    }
}
