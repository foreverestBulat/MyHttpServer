using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHttpServer.Attribuets
{
    public interface IHttpMethodAttribute
    {
        public string ActionName { get; }
    }
}
