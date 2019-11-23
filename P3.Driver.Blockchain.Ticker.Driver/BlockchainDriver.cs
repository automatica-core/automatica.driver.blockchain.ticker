﻿using Automatica.Core.Driver;
using Microsoft.Extensions.Logging;
using OpenWeatherMap;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using P3.Driver.Blockchain.Ticker.Driver;
using P3.Driver.Blockchain.Ticker.Driver.Bitcoin;

namespace P3.Driver.OpenWeatherMap.DriverFactory
{
    internal class BlockchainDriver : DriverBase
    {
        private Timer _timer = new Timer();
      
        private List<CoinNode> _nodes = new List<CoinNode>();

        private ILogger _logger;

        public BlockchainDriver(IDriverContext driverContext) : base(driverContext)
        {
            _logger = driverContext.Logger;
        }

        public override bool Init()
        {
            var pollTime = GetPropertyValueInt("poll");
            var apiKey = GetPropertyValueString("api-key");

            _timer.Elapsed += _timer_Elapsed;
            _timer.Interval = pollTime * 1000;


            return base.Init();
        }

        public override async Task<bool> Start()
        {
            _timer.Start();

            await ReadValues();

            return await base.Start();
        }

        private async void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            await ReadValues();

        }

        public override async Task<bool> Read()
        {
            await ReadValues();

            return true;
        }

        private async Task ReadValues()
        {
            foreach (var node in _nodes)
            {
                await node.Refresh();
            }
        }

        public override Task<bool> Stop()
        {
            _timer.Elapsed -= _timer_Elapsed;
            return base.Stop();
        }

        public override IDriverNode CreateDriverNode(IDriverContext ctx)
        {
            CoinNode node = null;
            switch(ctx.NodeInstance.This2NodeTemplateNavigation.Key)
            {
                case "blockchain-btc":
                    node = new BitcoinNode(ctx);
                    break;
            }

            if(node != null)
            {
                _nodes.Add(node);
            }

            return node;
        }
    }
}
