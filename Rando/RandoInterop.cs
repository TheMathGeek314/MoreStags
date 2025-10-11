using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoreStags.Rando {
    internal static class RandoInterop {
        public static void Hook() {
            RandoMenuPage.Hook();
        }
    }
}
