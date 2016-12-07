using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundBoxRemoteLib.Models
{
    interface IServerItem
    {
        SoundBoxServer Server { get; }
    }
}
