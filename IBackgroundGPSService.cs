using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemindMe
{
    public interface IBackgroundGPSService
    {
        void Start();
        void Stop();
    }
}
