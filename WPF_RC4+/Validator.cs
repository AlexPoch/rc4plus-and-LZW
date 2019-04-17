using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Validator
{
    public static class Validator
    {
        public static bool fileIsEmpty(this string value)
        {
            if (value == "" || value == null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
