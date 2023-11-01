using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyORM
{
    interface IMyDataContext
    {
        public void Add<T>(T row) where T : class;
        public void Update<T>(T row) where T : class;
        public void Delete<T>(int id) where T : class;
        public List<T> Select<T>() where T : class;
        public T SelectByID<T>(int id) where T : class;
    }
}
