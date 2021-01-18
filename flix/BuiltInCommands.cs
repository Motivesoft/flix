using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace flix
{
    public class BuiltInCommands
    {
        public readonly static string Open = "flix.default-commands.open";
        public readonly static string SelectAddressBar = "flix.default-commands.addressbar";
    }

    public class OpenCommand : ICommand
    {
        string ICommand.UniqueId => BuiltInCommands.Open;

        string ICommand.DisplayName => "Open";

        public IRunner GetRunner()
        {
            return new Runner();
        }

        class Runner : IRunner
        {
            public void Invoke( IContext context, string invocationString )
            {
                foreach ( var item in context.SelectedItems )
                {
                    if ( context.IsDirectory( item ) )
                    {
                        context.OpenDirectory( item );
                    }
                    else if ( context.IsFile( item ) )
                    {
                        context.OpenWithDefaultProgram( item );
                    }
                }
            }
        }
    }

    public class SelectAddressBarCommand : ICommand
    {
        string ICommand.UniqueId => BuiltInCommands.SelectAddressBar;

        string ICommand.DisplayName => "Select Address Bar";

        public IRunner GetRunner()
        {
            return new Runner();
        }

        class Runner : IRunner
        {
            public void Invoke( IContext context, string invocationString )
            {
                context.SelectAddressBar( invocationString );
            }
        }
    }
}
