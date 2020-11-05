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
        static Toplevel top;


        static string selectedKeyValue = "";


        static void Main(string[] args)
        {
            Application.Init();
            top = Application.Top;

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
                new MenuItem ("_Cut", "", null),
                new MenuItem ("_Paste", "", null)
            }),
            new MenuBarItem ("_View", new MenuItem [] {
                new MenuItem ("_As Text", "", viewAsText),
                new MenuItem ("_As JSON", "", viewAsJSON)
            }),
            new MenuBarItem ("_Key", new MenuItem [] {
                new MenuItem ("_Add", "", addKey),
                new MenuItem ("_Delete", "", deleteKey)
            })
            });
            top.Add(menu);

            try
            {

                loadKeys();
                listView = new ListView(new Rect(0, 0, top.Frame.Width / 5, 200), listOfNames);
                listView.OpenSelectedItem += onItemClick;
                textView = new TextView() { X = 0 , Y = 0, Width = Dim.Fill(), Height = Dim.Fill() };


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
            selectedKeyValue = db.StringGet((string)lviea.Value);
            textView.Text = selectedKeyValue;
        }

        static void deleteKey()
        {
            int selectedKey = listView.SelectedItem;
            IDatabase db = redis.GetDatabase();
            db.KeyDelete(listOfNames[selectedKey]);
            loadKeys();
        }

        static void addKey()
        {
            bool okpressed = false;
            Action okay = () => { Application.RequestStop(); okpressed = true; };
            var ok = new Button(3, 14, "Ok")
            {
                //Clicked = okay
            };
            ok.Clicked += () => { Application.RequestStop(); okpressed = true; };
            var cancel = new Button(10, 14, "Cancel")
            {
                
            };
            cancel.Clicked += () => Application.RequestStop();
            var dialog = new Dialog("Add Key", 60, 18, ok, cancel);

            var entryKey = new TextField()
            {
                X = 1,
                Y = 1,
                Width = Dim.Fill(),
                Height = 1
            };
            var entryValue = new TextField()
            {
                X = 1,
                Y = 2,
                Width = Dim.Fill(),
                Height = 1
            };
            dialog.Add(entryKey);
            dialog.Add(entryValue);
            Application.Run(dialog);
            if (okpressed)
            {
                IDatabase db = redis.GetDatabase();
                db.StringSet(entryKey.Text.ToString(), entryValue.Text.ToString());
                loadKeys();
            }
                
        }

        static void loadKeys()
        {
            IServer server = redis.GetServer("localhost", 6379);
            listOfNames.Clear();
            foreach (var key in server.Keys())
            {
                listOfNames.Add(key);
            }
            if(listView != null)
            {
                if(listView.SelectedItem == 0)
                {
                    listView.MoveDown();
                }
                else
                {
                    listView.MoveUp();
                }
            }
                
        }

        static void viewAsText()
        {
            textView.Text = selectedKeyValue;
        }
        static void viewAsJSON()
        {
            try
            {
                textView.Text = PrettyJson(selectedKeyValue);
            }catch(Exception e)
            {
                MessageBox.ErrorQuery("ERROR", e.ToString(), "Okay");
            }
            
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
