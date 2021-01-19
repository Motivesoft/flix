using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace flix
{
    public class Commands
    {
        private IDictionary<string, ICommand> CommandSet
        {
            get;
            set;
        }

        public Commands()
        {
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany( x => x.GetTypes() )
                .Where( x => typeof( ICommand ).IsAssignableFrom( x ) && !x.IsInterface && !x.IsAbstract );

            CommandSet = new Dictionary<string, ICommand>();
            foreach( var type in types )
            {
                try
                {
                    var command = Activator.CreateInstance( type ) as ICommand;
                    CommandSet[ command.UniqueId ] = command;
                }
                catch( Exception ex )
                {
                    // TODO Log this and subtly raise an error - maybe pass in a reporting mechanism or may this
                    // an initialiser method with a return type
                    System.Windows.Forms.MessageBox.Show( $"Failed to load command: ${type.FullName}: {ex.Message}" );
                }
            }
        }

        public bool Run( string uniqueId, IContext context, string invocationString = "" )
        {
            bool result = CommandSet.ContainsKey( uniqueId );
            if ( result )
            {
                var command = CommandSet[ uniqueId ];
                command.GetRunner().Invoke( context, invocationString );
            }
            else
            {
                throw new ArgumentException( $"Command ID unknown: {uniqueId}" );
            }

            return result;
        }

        public bool Process( string keystroke, Control control, IContext context, string invocationString = "" )
        {
            var keyConfig = GetKeyConfig( control );

            if ( keyConfig.ContainsKey( keystroke ) )
            {
                return Run( keyConfig[ keystroke ], context, invocationString );
            }

            return false;
        }

        private Dictionary<string, string> GetKeyConfig( Control control )
        {
            Dictionary<string, string> keyConfig = new Dictionary<string, string>();

            switch ( control )
            {
                case Control.AddressBar:
                    keyConfig.Add( "alt+D", BuiltInCommands.SwitchToAddressBar );
                    keyConfig.Add( "Down", BuiltInCommands.SwitchToList );
                    keyConfig.Add( "Return", BuiltInCommands.OpenFromAddressBar );
                    break;

                case Control.List:
                    keyConfig.Add( "alt+D", BuiltInCommands.SwitchToAddressBar );
                    keyConfig.Add( "Return", BuiltInCommands.Open );
                    keyConfig.Add( "Left", BuiltInCommands.OpenParentDirectory );
                    keyConfig.Add( "Right", BuiltInCommands.OpenChildDirectory );
                    break;
            }

            return keyConfig;
        }
    }

    public interface ICommand
    {
        string UniqueId
        {
            get;
        }

        string DisplayName
        {
            get;
        }

        IRunner GetRunner();
    }

    public interface IRunner
    {
        void Invoke( IContext context, string invocationString );
    }
}
