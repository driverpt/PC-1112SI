using System.Collections.Generic;

namespace Parte1_4
{
    public static class ExtensionMethods
    {
        public static IEnumerable<int> To( this int source, int target )
        {
            for( int i = source; i <= target; ++i )
            {
                yield return i;
            }
        } 
    }
}