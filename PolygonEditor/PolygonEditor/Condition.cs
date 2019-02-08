using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project1
{
    [Flags]
    public enum Condition
    {
        none=0,
        vertical=0b1,
        horizontal=0b10,
    }
}
