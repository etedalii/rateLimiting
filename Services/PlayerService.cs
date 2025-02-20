using Microsoft.Extensions.Options;
using Models;
using MongoDB.Driver;
using RateLimiting.Data;

namespace RateLimiting.Services;

public class PlayerService : MongoService<Player>, IPlayerService
{
    public PlayerService(IOptions<MongoDbSettings> mongoDbSettings) : base(mongoDbSettings)
    {
    }
}