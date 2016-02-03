using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Msiler.AssemblyParser
{
    public interface IListingGenerator
    {
        string GenerateListing(AssemblyMethod method);
    }
}
