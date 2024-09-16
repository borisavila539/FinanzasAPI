using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IConciliacionBancaria
    {
        public Task<string> CreatereConciliacionFolders(string path, string date, string dataAreaId);
    }
}
