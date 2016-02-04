using System.Drawing.Text;
using System.Linq;

namespace Msiler.Helpers
{
    public static class FontHelpers
    {
        public static bool IsFontFamilyExist(string fontFamily) {
            var fontsCollection = new InstalledFontCollection();
            return fontsCollection.Families.Any(ff => ff.Name == fontFamily);
        }
    }
}
