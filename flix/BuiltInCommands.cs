using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace flix
{
    public class BuiltInCommands
    {
        public readonly static string Open = "flix.default-commands.open";
        public readonly static string SwitchToAddressBar = "flix.default-commands.addressBar";
        public readonly static string SwitchToList = "flix.default-commands.switchToList";
        public readonly static string OpenFromAddressBar = "flix.default-commands.openFromAddressBar";
        public readonly static string OpenParentDirectory = "flix.default-commands.openParent";
        public readonly static string OpenChildDirectory = "flix.default-commands.openChild";
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

    public class OpenFromAddressBarCommand : ICommand
    {
        string ICommand.UniqueId => BuiltInCommands.OpenFromAddressBar;

        string ICommand.DisplayName => "Open From Address Bar";

        public IRunner GetRunner()
        {
            return new Runner();
        }

        class Runner : IRunner
        {
            public void Invoke( IContext context, string invocationString )
            {
                context.OpenFromAddressBar( invocationString );
            }
        }
    }

    public class OpenParentCommand : ICommand
    {
        string ICommand.UniqueId => BuiltInCommands.OpenParentDirectory;

        string ICommand.DisplayName => "Open Parent Directory";

        public IRunner GetRunner()
        {
            return new Runner();
        }

        class Runner : IRunner
        {
            public void Invoke( IContext context, string invocationString )
            {
                context.UpDirectory();
            }
        }
    }

    public class OpenChildCommand : ICommand
    {
        string ICommand.UniqueId => BuiltInCommands.OpenChildDirectory;

        string ICommand.DisplayName => "Open Child Directory";

        public IRunner GetRunner()
        {
            return new Runner();
        }

        class Runner : IRunner
        {
            public void Invoke( IContext context, string invocationString )
            {
                var focusedItem = context.FocusedItem;
                if ( context.IsDirectory( focusedItem ) )
                {
                    context.OpenDirectory( focusedItem );
                }
            }
        }
    }

    public class SelectAddressBarCommand : ICommand
    {
        string ICommand.UniqueId => BuiltInCommands.SwitchToAddressBar;

        string ICommand.DisplayName => "Select Address Bar";

        public IRunner GetRunner()
        {
            return new Runner();
        }

        class Runner : IRunner
        {
            public void Invoke( IContext context, string invocationString )
            {
                context.SwitchToAddressBar();
            }
        }
    }

    public class SwitchToListCommand : ICommand
    {
        string ICommand.UniqueId => BuiltInCommands.SwitchToList;

        string ICommand.DisplayName => "Switch To List";

        public IRunner GetRunner()
        {
            return new Runner();
        }

        class Runner : IRunner
        {
            public void Invoke( IContext context, string invocationString )
            {
                context.SwitchToList();
            }
        }
    }
}
