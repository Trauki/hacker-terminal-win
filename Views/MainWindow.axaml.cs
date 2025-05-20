using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia;
using Avalonia.Markup.Xaml;
using System.Threading.Tasks;
using Avalonia.Threading;
using System.Linq;
using System;

namespace HackerTerminal.Views;

public partial class MainWindow : Window
{
    // Command to edit a "resolver.c" file .\micro.exe resolver.c
    string[] micro_code = new string[] {".\\mi", "cro.e", "xe ","reso", "lve", "r.c"};
    
    // C code to simulate "hacker" aesthetic
    string[] exploit_code = new string[] {
        "long g","etip","(name)\u2028",
        "char", "*nam","e;\u2028",
        "{\u2028",
        "    st","ruct", "host","ent", "*hp;\u2028",
        "    ","lon","g ip;\u2028",
        "    e","xte","rn int"," h_err","no;\u2028",
        "\u2028",
        "    i","f ((","ip = i","net","_a","ddr","(nam","e))"," < 0)\u2028",
        "    {\u2028",
        "        i","f (!(","hp = ","geth","ost","byn","am","e(n","ame",")))\u2028",
        "        {\u2028",
        "            ","fpr","intf","(st","derr",", \"geth","ost","byna","me(): %s\\n\",\u2028",
        "                ","strer","ror(h_err","no));\u2028",
        "            ","exit","(1);\u2028",
        "        }\u2028",
        "        ","memcpy","(&ip,"," (hp","->","h_add","r), 4);\u2028",
        "    }\u2028",
        "\u2028",
        "    ","retur","n ip;\u2028",
        "}\u2028",
        "\u2028",
        "int co","nne","ctt","oftp","(host",", port)\u2028",
        "char *host;\u2028",
        "int ","port;\u2028",
        "{\u2028",
        "    int s","ockfd;\u2028",
        "    ","stru","ct soc","kaddr_","in cli;\u2028",
        "\u2028",
        "    ","bzero","(&cli,"," sizeof","(cli","));\u2028",
        "    ","cli.","sin_","fam","ily = AF","_IN","ET;\u2028",
        "    ","cli.","sin_","addr",".s_add","r=g","etip","(host);\u2028",
        "    ","cli.","sin_","port"," = hto","ns(port);\u2028",
        "\u2028",
        "    if((s","ockfd = ","socket(AF","_INET, SO","CK_STREAM",", 0)) < 0)\u2028",
        "    {\u2028",
        "        ","per","ror(\"sock","et\");\u2028",
        "        ","retu","rn -1;\u2028",
        "    }\u2028",
        "\u2028",
        "    i","f(con","nect(sockfd, (","struct"," sock","addr *",")&cli",", si","zeof","(cli)) < 0)\u2028",
        "    {","\u2028",
        "        ","perro","r(\"conn","ect\");\u2028",
        "        ","c","los","e(soc","kfd);\u2028",
        "        ","r","etu","rn -1;\u2028",
        "    }\u2028",
        "\u2028",
        "    c","in = ","fdop","en(soc","kfd, \"r\");\u2028",
        "    ","cout = ","fdop","en(soc","kfd, \"w\");\u2028",
        "\u2028",
        "    if (","!cin"," || ","!cout)\u2028",
        "    {\u2028",
        "        ","close","(soc","kfd);\u2028",
        "        ","retu","rn -1;\u2028",
        "    }\u2028",
        "\u2028",
        "    return"," sockfd;\u2028",
        "}\u2028"
    };
    private int currentIndex = 0;

    // Used to alter between the default terminal view and the micro text editor display
    private bool microActive = false;
    private int microIndex = 0;
    public MainWindow()
    {
        InitializeComponent();
        InitializeCaretBlink();
        this.KeyDown += OnKeyDown;

        // Validate resources exist
        if (this.FindResource("restore_regular") is not Geometry 
            || this.FindResource("maximize_regular") is not Geometry)
        {
            throw new XamlLoadException("Missing icon resources");
        }

        // Subscribe to WindowState changes using Avalonia's property system
        WindowStateProperty.Changed.AddClassHandler<Window>(
            (sender, e) => UpdateMaximizeIcon(e));
    }

    // Updates the cursor position values on the micro text editor panel
    private void UpdateCursorPosition()
    {
        if (TerminalOutput.Text == null)
            return;

        var text = TerminalOutput.Text;
        // Get line count from the number of line breaks currently in the display
        int line = text.Count(c => c == '\u2028') + 1;
        // Get the column value based on the length of the text after the last line break (-1 to account for it)
        int lastLineStart = text.LastIndexOf('\u2028');
        int column = lastLineStart == -1 
            ? text.Length + 1 
            : text.Length - lastLineStart;

        // Update only the line and column values in the text
        CursorPositionText.Text = $"resolver.c ({line},{column}) | ft:c | dos | utf-8";
    }

    // Handles displaying the column of line numbers inside the micro text editor display

    private void UpdateLineNumbers()
    {
        if (!microActive) return;

        int lines = TerminalOutput.Text?.Split('\u2028').Length ?? 1;
        System.Diagnostics.Debug.WriteLine("Debug info");
        LineNumbers.Text = string.Join('\u2028', Enumerable.Range(1, lines).Select(i => i.ToString()));
    }

    // Handles displaying the code when keys are pressed.
    private void OnKeyDown(object? sender, KeyEventArgs e)
    {

        // Removes the caret to prevent it from being included in the text
        if (TerminalOutput.Text != null && TerminalOutput.Text.EndsWith("|"))
        {
            TerminalOutput.Text = TerminalOutput.Text.TrimEnd('|');
        }

        // Start by displaying the initial `micro resolver.c` text into the terminal
        if (microActive == false) {
            // Display the initial code 
            if(microIndex < micro_code.Length) {
                TerminalOutput.Text += micro_code[microIndex];
                microIndex++;
                return;
            } else {
                
                // Once the code has been displayed, we need to update the UI, reset TerminalOutput.Text, and mark microActive as true.
                microActive = true;
                ShellText.IsVisible = false;
                ShellTextPath.IsVisible = false;
                TerminalOutputPanel.Margin = new Thickness(2, 2, 4, 0);
                TerminalOutput.Margin = new Thickness(16, 8, 16, 0);
                TerminalOutput.Text = "";
                TerminalOutput.Foreground = new SolidColorBrush(Colors.White);
                LineNumbers.IsVisible = true;
                MicroActiveGrid.IsVisible = true;
                
                return;
            }
        }

        // Hacker Text logic (once the display has been updated to mock a text editor)
        // Ignore special non-printable keys
        if (e.Key == Key.Back)
        {
            if (TerminalOutput.Text != null && TerminalOutput.Text.Length > 0)
            {
                TerminalOutput.Text = TerminalOutput.Text[..^1];
                UpdateLineNumbers();
                UpdateCursorPosition();
            }
        }
        else
        {
            // Show the next exploit code piece from the array
            if (currentIndex < exploit_code.Length)
            {
                TerminalOutput.Text += exploit_code[currentIndex];
                UpdateLineNumbers();
                UpdateCursorPosition();
                currentIndex++;
            }
            else
            {
                // Reset the index to loop through the exploit code again
                currentIndex = 0;
                TerminalOutput.Text += exploit_code[currentIndex];
                UpdateLineNumbers();
                UpdateCursorPosition();
                currentIndex++;
            }
        }
        // Scroll to bottom after text is added
        ScrollToBottom();
    }

    private async void ScrollToBottom()
    {
        // Small delay to ensure the UI has updated
        await Task.Delay(10);
        MainScrollViewer.ScrollToEnd();
    }

    // Logic that makes the caret blink
    private async void InitializeCaretBlink()
    {
        while (true)
        {
            await Task.Delay(500); // blink interval

            // Run on the UI thread to safely toggle visibility (avoids stalling constructor)
            await Dispatcher.UIThread.InvokeAsync(() =>
            {   if(TerminalOutput.Text == null) {
                    TerminalOutput.Text += "|";
                }
                else if (TerminalOutput.Text.EndsWith("|"))
                {
                    TerminalOutput.Text = TerminalOutput.Text.TrimEnd('|');
                }
                else
                {
                    TerminalOutput.Text += "|";
                }
            });
        }
    }

    // Update maximize icon based on window state
    private void UpdateMaximizeIcon(AvaloniaPropertyChangedEventArgs e)
    {   
        // If changed state is WindowState then update maximize icon
        if (e.Property == Window.WindowStateProperty && e.GetNewValue<WindowState>() is WindowState newState)
        {   
            if (MaximizeIcon != null)
            {
                // Set icon based on current window state
                MaximizeIcon.Data = newState == WindowState.Maximized
                    ? (Geometry)this.FindResource("restore_regular")!
                    : (Geometry)this.FindResource("maximize_regular")!;
            }
        }
    }

    // Handle window drag when the title bar is clicked
     private void TitleBar_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            this.BeginMoveDrag(e);
        }
    }

    // Minimizes the window
    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        this.WindowState = WindowState.Minimized;
    }

    // Maximize the window
    private void MaximizeButton_Click(object sender, RoutedEventArgs e)
    {
        if (this.WindowState == WindowState.Maximized)
        {
            this.WindowState = WindowState.Normal;
        }
        else
        {
            this.WindowState = WindowState.Maximized;
        }
    }

    // Close the window
    private void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        this.Close();
    }
}