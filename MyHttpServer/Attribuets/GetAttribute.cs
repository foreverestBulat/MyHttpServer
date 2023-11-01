using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHttpServer.Attribuets
{
    public class GetAttribute : Attribute, IHttpMethodAttribute
    {
        public GetAttribute(string actionName)
        {
            ActionName = actionName;
        }

        public string ActionName { get; }
    }
}
