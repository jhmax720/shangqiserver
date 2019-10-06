﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Shangqi.Logic.Services
{
    public class BackgroundService: IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            
            return Task.CompletedTask;
        }

        public BackgroundService(ILogger<BackgroundService> logger)
        {
            Debug.WriteLine("hello BackgroundService!");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            //New Timer does not have a stop. 
            return Task.CompletedTask;
        }
    }
}
