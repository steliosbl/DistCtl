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
        private DistCommon.TaskQueue queue;

        public Console()
        {
            this.prompt = new DistCommon.LivePrompt();
            this.prompt.AddInputHandler(this.InputHandler);
            this.queue = new DistCommon.TaskQueue();
            this.knownCommands = new Dictionary<string, Action<string[]>>()
            {
                { "help", this.Help },
                { "exit", this.Exit },
                { "add", this.Add },
                { "remove", this.Remove }
            };
        }

        public void Say(string msg, ConsoleColor foregroundColor = ConsoleColor.White)
        {
            this.prompt.Say(msg, foregroundColor);
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
            Task.Run(() => this.queue.Enqueue(() => this.HandleCommand(command)));
        }

        private async Task<bool> HandleCommand(string command)
        {
            string command_main = command.Split(new char[] { ' ' }).First();
            string[] arguments = command.Split(new char[] { ' ' }).Skip(1).ToArray();
            if (this.knownCommands.ContainsKey(command_main))
            {
                Action<string[]> function_to_execute = null;
                this.knownCommands.TryGetValue(command_main, out function_to_execute);
                await Task.Run(() => function_to_execute(arguments));
            }
            else
            {
                this.Say("Command '" + command_main + "' not found");
            }

            return true;
        }

        private async void Add(string[] args)
        {
            if (args.Length > 1)
            {
                string subCmd = args[0];
                args = args.Skip(1).ToArray();
                switch (subCmd)
                {
                    case "job":
                        await this.AddJob(args);
                        break;
                    case "node":
                        await this.AddNode(args);
                        break;
                    default:
                        this.SayInvalid();
                        break;
                }
            }
            else
            {
                this.SayInvalid();
            }
        }

        private async Task<bool> AddJob(string[] args)
        {
            int id = -1, priority = -1, node = -1;
            string cmd = null;
            foreach (string arg in args)
            {
                var arr = arg.Split('=');
                if (arr.Length == 2)
                {
                    switch (arr[0])
                    {
                        case "id":
                             if (int.TryParse(arr[1], out id))
                            {
                                id = Math.Abs(id);
                            }

                            break;
                        case "priority":
                            if (int.TryParse(arr[1], out priority))
                            {
                                priority = Math.Abs(priority);
                            }

                            break;
                        case "node":
                            if (int.TryParse(arr[1], out node))
                            {
                                node = Math.Abs(node);
                            }

                            break;
                        case "cmd":
                            cmd = arr[1].Replace('"'.ToString(), string.Empty).Replace("'", string.Empty);
                            break;
                    }
                }
            }

            if (id != -1 && priority != -1 && !string.IsNullOrEmpty(cmd) && !string.IsNullOrWhiteSpace(cmd))
            {
                int result = await this.controller.Add(new DistCommon.Job.Blueprint(id, priority, cmd), node);

                this.SayResult(result);
            }
            else
            {
                this.SayInvalid();
            }

            return true;
        }

        private async Task<bool> AddNode(string[] args)
        {
            int id = -1, slots = -1;
            System.Net.IPEndPoint address = null;
            bool primary = true;
            foreach (string arg in args)
            {
                var arr = arg.Split('=');
                if (arr.Length == 2)
                {
                    switch (arr[0])
                    {
                        case "id":
                            if (int.TryParse(arr[1], out id))
                            {
                                id = Math.Abs(id);
                            }

                            break;
                        case "slots":
                            if (int.TryParse(arr[1], out slots))
                            {
                                slots = Math.Abs(slots);
                            }

                            break;
                        case "primary":
                            bool.TryParse(arr[1], out primary);
                            break;

                        case "address":
                            string str = arr[1].Replace('"'.ToString(), string.Empty).Replace("'", string.Empty);
                            var addrarr = str.Split(':');
                            int port = -1;
                            System.Net.IPAddress addr;
                            if (addrarr.Length == 2 && int.TryParse(addrarr[1], out port) && System.Net.IPAddress.TryParse(addrarr[0], out addr))
                            {
                                address = new System.Net.IPEndPoint(addr, port);
                            }

                            break;
                    }
                }
            }

            if (id != -1 && slots != -1 && address != null)
            {
                int result = await this.controller.Add(new DistCommon.Schema.Node(id, slots, address, primary));
                this.SayResult(result);
            }
            else
            {
                this.SayInvalid();
            }

            return true;
        }

        private async void Remove(string[] args)
        {
            if (args.Length > 1)
            {
                string subCmd = args[0];
                int id = -1;
                foreach (string arg in args)
                {
                    var arr = arg.Split('=');
                    if (arr.Length == 2)
                    {
                        switch (arr[0])
                        {
                            case "id":
                                if (int.TryParse(arr[1], out id))
                                {
                                    id = Math.Abs(id);
                                }
                                break;
                        }
                    }
                }

                if (id != -1)
                {
                    int result;
                    if (subCmd == "job" || subCmd == "node")
                    {
                        if (subCmd == "job")
                        {
                            result = await this.controller.Remove(id, 0);
                        }
                        else
                        {
                            result = await this.controller.Remove(id);
                        }

                        this.SayResult(result);
                    }
                }
            }
            else
            {
                this.SayInvalid();
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

        private void SayResult(int result)
        {
            this.Say(string.Format("Command execution finished with result [{0}]", DistCommon.Constants.Results.Messages[result]));
        }

        private void SayInvalid()
        {
            this.Say("Invalid command");
        }
    }
}
