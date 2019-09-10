using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarthogChart
{
    interface IControlSet
    {
        SerializableDictionary<string, string> GetSet();
        void SetSet(SerializableDictionary<string, string> set);
        void CleanUp();
    }

}
