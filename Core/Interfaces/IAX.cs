using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IAX
    {
        public Task<string> InsertDefectos(int id);
        public Task<string> RegistroPagoBoletin(string JournalID);

    }
}
