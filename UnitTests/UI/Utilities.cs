using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.UI
{
    internal static class Utilities
    {
        internal static async Task WaitForCondition(Func<bool> condition, TimeSpan delay, TimeSpan timeout)
        {
            var to = Task.Delay(timeout);
            var task = Task.Run(async () =>
            {
                while (!condition() && !to.IsCompleted)
                    await Task.Delay(delay);
            });
            await Task.WhenAny(task, to);
        }
    }
}
