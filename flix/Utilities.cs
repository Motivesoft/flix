using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace flix
{
    public static class Utilities
    {
        public interface IKeyStrokeAction
        {
            void Process( string keystroke );
        }

        public static bool ProcessKeyStoke( KeyEventArgs e, Func<string, bool> a )
        {
            if ( e.KeyCode == Keys.ControlKey || e.KeyCode == Keys.ShiftKey || e.KeyCode == Keys.Menu )
            {
                return false;
            }

            var keyString = new StringBuilder();
            if ( e.Control )
            {
                keyString.Append( "ctrl+" );
            }
            if ( e.Shift )
            {
                keyString.Append( "shift+" );
            }
            if ( e.Alt )
            {
                keyString.Append( "alt+" );
            }

            keyString.Append( e.KeyCode );

            return a( keyString.ToString() );
        }
    }
}
