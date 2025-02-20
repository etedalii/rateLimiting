using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Memory;
using Models;
using RateLimiting.BackgroundQueue;
using RateLimiting.Services;

namespace RateLimiting.Controllers;

[ApiController]
[Route("api/players")] // Base route for the controller
public class PlayerController : ControllerBase
{
    private readonly IPlayerService _playerService;
    private static int _requestCount = 1;
    private readonly IMemoryCache _resultCache;
    private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);


    public PlayerController(IPlayerService playerService, IMemoryCache memoryCache)
    {
        _playerService = playerService;
        _resultCache = memoryCache;
    }

    // ✅ Get all players
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Player>>> GetAllPlayers()
    {
        var players = await _playerService.GetAsync();
        return Ok(players);
    }

    // ✅ Get a single player by ID
    [HttpGet("{id}")]
    public async Task<ActionResult<Player>> GetPlayer(string id)
    {
        var player = await _playerService.GetAsync(id);
        if (player == null)
            return NotFound($"Player with ID {id} not found.");

        return Ok(player);
    }

    // ✅ Create a new player
    [HttpPost]
    public async Task<ActionResult<Player>> CreatePlayer([FromBody] Player newPlayer, [FromServices] IBackgroundTaskQueue taskQueue)
    {
        if (newPlayer == null)
            return BadRequest("Invalid player data.");

        //var createdPlayer = await _playerService.CreateAsync(newPlayer);
        // return CreatedAtAction(nameof(GetPlayer), new { id = createdPlayer.Id }, createdPlayer);
        // Queue the background task to create the player

        var referenceId = Guid.NewGuid().ToString(); // Generate tracking ID
        taskQueue.QueueBackgroundWorkItem(async token =>
        {
            await Task.Delay(100, token); // Simulate a long-running task
            await _playerService.CreateAsync(newPlayer);
        });

        return Accepted(new
        {
            Message = "Player creation is being processed.",
            Status = "Processing",
            ReferenceId = referenceId
        });
    }

    // ✅ Update an existing player
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePlayer(string id, [FromBody] Player updatedPlayer)
    {
        if (updatedPlayer == null || id != updatedPlayer.Id)
            return BadRequest("Player ID mismatch.");

        var existingPlayer = await _playerService.GetAsync(id);
        if (existingPlayer == null)
            return NotFound($"Player with ID {id} not found.");

        await _playerService.UpdateAsync(id, updatedPlayer);
        return NoContent(); // 204 No Content on successful update
    }

    // ✅ Delete a player
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePlayer(string id)
    {
        var existingPlayer = await _playerService.GetAsync(id);
        if (existingPlayer == null)
            return NotFound($"Player with ID {id} not found.");

        await _playerService.RemoveAsync(id);
        return NoContent();
    }

    // ✅ Test Rate Limiting with Database Interaction
    [HttpGet("test-db-rate-limit")]
    public async Task<ActionResult<IEnumerable<Player>>> TestDatabaseRateLimit()
    {
        var players = await _playerService.GetAsync();
        return Ok(players);
    }

    // ✅ Test Rate Limiting with Database Interaction
    [HttpGet("test-bck-queue")]
    public async Task<ActionResult<IEnumerable<Player>>> TestBackgroundQueue([FromServices] IBackgroundTaskQueue taskQueue)
    {
        // Generate a unique job reference ID
        var referenceId = Guid.NewGuid().ToString();

        // Enqueue the background task to fetch players and store the result
        taskQueue.QueueBackgroundWorkItem(async token =>
        {
            // Execute the heavy database operation
            var players = await _playerService.GetAsync();

            // Store the result in a temporary cache, database, or in-memory dictionary keyed by referenceId
            // For illustration, assume you have a service: _resultCache
            _resultCache.Set(referenceId, players, TimeSpan.FromMinutes(5));
        });

        // Return immediately with a reference ID
        return Accepted(new
        {
            Message = "Request is being processed.",
            ReferenceId = referenceId
        });
    }

    [HttpGet("result/{referenceId}")]
    public ActionResult<IEnumerable<Player>> GetResult(string referenceId)
    {
        // Try to retrieve the result from the cache
        if (_resultCache.TryGetValue(referenceId, out IEnumerable<Player> players))
        {
            return Ok(players);
        }
        else
        {
            return NotFound("Result not ready yet or job ID is invalid.");
        }
    }

    [DisableRateLimiting]
    [HttpGet("no-rate-limit")]
    public IActionResult NoRateLimitEndpoint()
    {
        return Ok("No rate limiting here.");
    }

    // This endpoint simulates a critical operation protected by the semaphore.
    [HttpGet("semaphore")]
    public async Task<IActionResult> TestSemaphore()
    {
        await _semaphore.WaitAsync();
        try
        {
            // Simulate a long-running operation
            await Task.Delay(1);
            return Ok("Processed with semaphore protection.");
        }
        finally
        {
            _semaphore.Release();
        }
    }
}