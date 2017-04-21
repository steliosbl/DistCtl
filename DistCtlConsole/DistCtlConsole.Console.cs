namespace DistCtlConsole
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class Console
    {
        private readonly Dictionary<string, Action<string[]>> knownCommands;
        private DistCommon.LivePrompt prompt;
        private DistCtl.Controller controller;

        public Console()
        {
            this.prompt = new DistCommon.LivePrompt();
            this.prompt.AddInputHandler(this.InputHandler);
            this.knownCommands = new Dictionary<string, Action<string[]>>()
            {
                { "help", this.Help },
                { "exit", this.Exit }
            };
        }

        public void Say(string msg)
        {
            this.prompt.Say(msg);
        }

        public void Start()
        {
            this.prompt.MainLoop();
        }

        public void Stop()
        {
            this.prompt = null;
        }

        public void AddController(DistCtl.Controller controller)
        {
            this.controller = controller;
        }

        private void InputHandler(string command)
        {
            string command_main = command.Split(new char[] { ' ' }).First();
            string[] arguments = command.Split(new char[] { ' ' }).Skip(1).ToArray();
            if (this.knownCommands.ContainsKey(command_main))
            {
                Action<string[]> function_to_execute = null;
                this.knownCommands.TryGetValue(command_main, out function_to_execute);
                Task.Run(() => function_to_execute(arguments));
            }
            else
            {
                this.Say("Command '" + command_main + "' not found");
            } 
        }

        private void Help(string[] args)
        {
            this.Say("test");
        }

        private void Exit(string[] args)
        {
            this.controller.Exit();
        }
    }
}
