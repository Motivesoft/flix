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

        public void Run( string uniqueId, IContext context, string invocationString = "" )
        {
            if ( CommandSet.ContainsKey( uniqueId ) )
            {
                var command = CommandSet[ uniqueId ];
                command.GetRunner().Invoke( context, invocationString );
            }
            else
            {
                throw new ArgumentException( $"Command ID unknown: {uniqueId}" );
            }
        }

        public void Process( string keystroke, IContext context, string invocationString = "" )
        {
            var keyConfig = new Dictionary<string, string>();
            keyConfig.Add( "alt+D", BuiltInCommands.SelectAddressBar );

            if ( keyConfig.ContainsKey( keystroke ) )
            {
                Run( keyConfig[ keystroke ], context, invocationString );
            }
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
