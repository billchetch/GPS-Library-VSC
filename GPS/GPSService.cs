using System;
using Chetch.ChetchXMPP;
using Chetch.Database;
using Microsoft.Extensions.Logging;

namespace Chetch.GPS;

public class GPSService : ChetchXMPPService<GPSService>
{
    public GPSService(ILogger<GPSService> Logger) : base(Logger)
    {
        ChetchDbContext.Config = Config;

        
    }

    protected override Task Execute(CancellationToken stoppingToken)
    {
        return base.Execute(stoppingToken);
    }
}
