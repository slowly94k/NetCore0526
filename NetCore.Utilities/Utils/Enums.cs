using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCore.Utilities
{
    public class Enums
    {
        /// <summary>
        /// 암호화 유형
        /// </summary>
        public enum CryptoType
        {
            Unmanaged = 1,

            Managed = 2,

            CngCbc = 3,

            CngGcm = 4
        }
    }
}
