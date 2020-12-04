using PGMS.CQSLight.Infra.Commands;
using PGMS.CQSLight.Infra.Commands.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PGMS.CQSLight.Helpers
{
    public class CommandHelper
    {
        private readonly IBus bus;

        public CommandHelper(IBus bus)
        {
            this.bus = bus;
        }

        public async Task Send(ICommand command, string username) => await Task.Run(() =>
        {
            command.ByUsername = username;
            bus.Send(command);
        });

        public async Task Send(List<ICommand> commands, string username)
        {
            foreach (var command in commands)
            {
                await Send(command, username);
            }
        }
    }
}