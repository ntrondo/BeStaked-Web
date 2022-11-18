using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Willoch.DemoApp.Client.Code.Models;
using Willoch.DemoApp.Client.Services;

namespace UnitTests.UI.Services
{
    internal class MockStakeValuationService : StakeValuationService
    {
        public MockStakeValuationService(ILogger<StakeValuationService> logger, ITransferableStakeAccessor transferableStakeAccessor, IAssetsService assetsAccessor) 
            : base(logger, transferableStakeAccessor, assetsAccessor)
        {
            this._dailyDataLoadingBatchSize = 200;
        }
    }
}
