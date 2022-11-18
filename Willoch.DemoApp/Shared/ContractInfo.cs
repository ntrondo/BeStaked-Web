using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Willoch.DemoApp.Shared
{
    public record ContractInfo(string Address);      
    
    public record ERC20Info(string Address, int Decimals) : ContractInfo(Address);
}
