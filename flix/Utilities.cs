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

        public static bool ProcessKeyStoke( KeyEventArgs e, Func<string, bool> action )
        {
            // If only a modifier, stop processing
            if ( e.KeyCode == Keys.ControlKey || e.KeyCode == Keys.ShiftKey || e.KeyCode == Keys.Menu )
            {
                return false;
            }

            var keyString = new StringBuilder();

            // Modifiers
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

            // Normalise
            var keyCode = e.KeyCode;
            if ( e.KeyCode == Keys.Return )
            {
                keyCode = Keys.Enter;
            }

            // Complete the string
            keyString.Append( keyCode );

            // Process the keystroke
            return action( keyString.ToString() );
        }
    }
}
