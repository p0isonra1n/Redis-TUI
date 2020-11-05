using System;
using System.Collections.Generic;
using System.Text.Json;
using Terminal.Gui;
using StackExchange.Redis;

namespace Redis_TUI
{
    class Program
    {

        static List<string> listOfNames = new List<string>();


        static List<string> secondList = new List<string>();
        static ListView listView;
        static ListView rightListView;
        static TextView textView;

        static ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");


        static void Main(string[] args)
        {
            Application.Init();
            var top = Application.Top;

            // Creates the top-level window to show
            var winNodes = new Window("Nodes")
            {
                X = 0,
                Y = 1, // Leave one row for the toplevel menu

                // By using Dim.Fill(), it will automatically resize without manual intervention
                Width = top.Frame.Width / 5,
                Height = Dim.Fill()
            };
            var winDetails = new Window("Details")
            {
                X = Pos.Right(winNodes),
                Y = Pos.AnchorEnd(top.Frame.Height / 5), // Leave one row for the toplevel menu

                // By using Dim.Fill(), it will automatically resize without manual intervention
                Width = Dim.Fill(),
                Height = top.Frame.Height / 5
            };
            var winValues = new Window("Values")
            {
                X = Pos.Right(winNodes),
                Y = 1, // Leave one row for the toplevel menu

                // By using Dim.Fill(), it will automatically resize without manual intervention
                Width = Dim.Fill(),
                Height = Dim.Fill() - (top.Frame.Height / 5)
            };

            top.Add(winNodes);
            top.Add(winValues);
            top.Add(winDetails);

            // Creates a menubar, the item "New" has a help menu.
            var menu = new MenuBar(new MenuBarItem[] {
            new MenuBarItem ("_File", new MenuItem [] {
				//new MenuItem ("_New", "Creates new file", NewFile),
				//new MenuItem ("_Close", "", () => Close ()),
				//new MenuItem ("_Quit", "", () => { if (Quit ()) top.Running = false; })
			}),
            new MenuBarItem ("_Edit", new MenuItem [] {
                new MenuItem ("_Copy", "", null),
                new MenuItem ("C_ut", "", null),
                new MenuItem ("_Paste", "", null)
            })
            });
            top.Add(menu);

            try
            {
                IServer server = redis.GetServer("localhost", 6379);
                foreach (var key in server.Keys())
                {
                    listOfNames.Add(key);
                }

                listView = new ListView(new Rect(0, 0, top.Frame.Width / 5, 200), listOfNames);
                listView.OpenSelectedItem += onItemClick;
                textView = new TextView() { X = 0 , Y = 0, Width = Dim.Fill(), Height = Dim.Fill() };

                var labelNodes = new Label("Nodes: ") { X = 0, Y = 0 };
                var labelValue = new Label("Value: ") { X = Pos.Right(listView), Y = 0 };

                winNodes.Add(listView);
                winValues.Add(textView);
            }
            catch (Exception e)
            {
                MessageBox.ErrorQuery("Unable to connect to Redis", "Please check Redis is running", "Okay");
            }

            Application.Run();
        }

        static void onItemClick(ListViewItemEventArgs lviea)
        {
            IDatabase db = redis.GetDatabase();
            //MessageBox.Query("Is this right", (string)db.StringGet((string)lviea.Value), "Yes", "no");
            //listOfNames.Insert(lviea.Item + 1, "->TESTTST");
            //listView.MoveDown();
            //textView.Text = PrettyJson((string)db.StringGet((string)lviea.Value));
            textView.Text = (string)db.StringGet((string)lviea.Value);
        }
        public static string PrettyJson(string unPrettyJson)
        {
            var options = new JsonSerializerOptions()
            {
                WriteIndented = true
            };

            var jsonElement = JsonSerializer.Deserialize<JsonElement>(unPrettyJson);

            return JsonSerializer.Serialize(jsonElement, options);
        }
    }
}
