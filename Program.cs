using System;
using System.Linq;

class Menu
{
    public string Title { get; }
    public string[] Options { get; }
    public string Prompt { get; }
    public int Width { get; }

    public Menu(string title, string[] options, string prompt, int width = 40)
    {
        Title = title;
        Options = options;
        Prompt = prompt;
        Width = width;
    }

    public void Render()
    {
        Console.WriteLine("╔" + new string('═', Width - 2) + "╗");
        Console.WriteLine("║" + CenterLine("DBJ") + "║");
        Console.WriteLine("║" + CenterLine("Digital Bank of JRU") + "║");
        Console.WriteLine("╠" + new string('═', Width - 2) + "╣");
        Console.WriteLine("║" + CenterLine(Title) + "║");

        int maxOptionLength = Options.Max(o => o.Length);
        int blockWidth = maxOptionLength;
        int leftPadding = (Width - 2 - blockWidth) / 2;
        int rightPadding = (Width - 2) - blockWidth - leftPadding;

        foreach (var option in Options)
        {
            string line = new string(' ', leftPadding) + option.PadRight(blockWidth) + new string(' ', rightPadding);
            Console.WriteLine("║" + line + "║");
        }

        Console.WriteLine("║" + new string(' ', Width - 2) + "║");
        Console.WriteLine("║" + CenterLine(Prompt) + "║");
        Console.WriteLine("╚" + new string('═', Width - 2) + "╝");

        int cursorLeft = (Width - 2 - Prompt.Length) / 2 + 2 + Prompt.Length;
        int cursorTop = Console.CursorTop - 2;

        cursorLeft = Math.Clamp(cursorLeft, 0, Console.BufferWidth - 1);
        cursorTop = Math.Clamp(cursorTop, 0, Console.BufferHeight - 1);

        Console.SetCursorPosition(cursorLeft, cursorTop);
    }

    public char Choice()
    {
        ConsoleKeyInfo keyInfo = Console.ReadKey(true);
        char choice = char.ToLower(keyInfo.KeyChar);
        Console.Write(char.ToUpper(choice));
        return choice;
    }

    private string CenterLine(string text)
    {
        int width = Width - 2;
        int padding = (width - text.Length) / 2;
        return new string(' ', padding) + text + new string(' ', width - text.Length - padding);
    }
}

class StartMenu : Menu
{
    public StartMenu()
        : base(" ", new[] { "S -> Start Transaction", "Q -> Quit" }, "Enter your choice:")
    {
    }
}

class Program
{
    static string[] AccNum = {
        "0123-4567-8901", "2345-6789-0123", "3456-7890-1234", "4567-8901-2345", "5678-9012-3456"
    };

    static string[] AccName = {
        "Roel Richard", "Donie Marie", "Railee Darrel", "Railynne Dessirei", "Raine Dessirei"
    };

    static string[] Balance = {
        "5000.00", "0.00", "10000", "2500", "10000"
    };

    static string[] PinNum = {
        "1111", "2222", "3333", "4444", "5555"
    };

    static string[] Status = {
        "Active", "Blocked", "Active", "Active", "Active"
    };

    static int currentUserIndex = -1;

    static void Main()
    {
        bool running = true;

        while (running)
        {
            Console.Clear();
            var menu = new StartMenu();
            menu.Render();
            char choice = menu.Choice();

            switch (choice)
            {
                case 's':
                    Console.Clear();
                    if (LoginSystem())
                    {
                        TransactionMenu();
                    }
                    break;

                case 'q':
                    running = false;
                    Console.WriteLine("\n\nExiting program...");
                    break;

                default:
                    Console.WriteLine("\n\nInvalid option.");
                    break;
            }

            if (running)
            {
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
            }
        }
    }

    // ---------------- LOGIN SYSTEM ----------------
    static bool LoginSystem()
    {
        Console.Clear();

        var accMenu = new Menu(
            "LOGIN",
            new string[] { "Enter Account Number" },
            ""
        );

        accMenu.Render();
        Console.SetCursorPosition(12, Console.CursorTop);
        string inputAcc = Console.ReadLine();

        currentUserIndex = Array.IndexOf(AccNum, inputAcc);

        if (currentUserIndex == -1)
        {
            Console.Clear();
            new Menu("LOGIN", new[] { "Account not found." }, "Press any key...").Render();
            Console.ReadKey();
            return false;
        }

        if (Status[currentUserIndex] == "Blocked")
        {
            Console.Clear();
            new Menu("LOGIN", new[] { "This account is blocked." }, "Press any key...").Render();
            Console.ReadKey();
            return false;
        }

        int attempts = 0;

        while (attempts < 3)
        {
            Console.Clear();

            var pinMenu = new Menu(
                "ENTER PIN",
                new string[]
                {
                "Enter PIN (Q to quit)",
                $"Attempts left: {3 - attempts}"
                },
                "PIN: "
            );

            pinMenu.Render();
            Console.SetCursorPosition(Console.CursorLeft + 5, Console.CursorTop);
            string pin = Console.ReadLine();

            if (pin?.ToLower() == "q")
                return false;

            if (pin == PinNum[currentUserIndex])
            {
                Console.Clear();
                new Menu("SUCCESS", new[] { "Login successful!" }, "Press any key...").Render();
                Console.ReadKey();
                return true;
            }

            attempts++;

            Console.Clear();
            new Menu("ERROR", new[] { "Incorrect PIN." }, "Press any key...").Render();
            Console.ReadKey();
        }

        Status[currentUserIndex] = "Blocked";

        Console.Clear();
        new Menu("BLOCKED", new[] { "Account blocked after 3 failed attempts." }, "Press any key...").Render();
        Console.ReadKey();

        return false;
    }

    // ---------------- TRANSACTION MENU ----------------
    static void TransactionMenu()
    {
        bool inMenu = true;

        while (inMenu)
        {
            Console.Clear();
            Console.WriteLine("SELECT TYPE OF TRANSACTION");
            Console.WriteLine("B -> Balance Inquiry");
            Console.WriteLine("W -> Withdrawal");
            Console.WriteLine("D -> Deposit");
            Console.WriteLine("T -> Transfer Fund");
            Console.WriteLine("C -> Cancel");

            Console.Write("Enter transaction type: ");
            char choice = char.ToUpper(Console.ReadKey().KeyChar);
            Console.WriteLine();

            switch (choice)
            {
                case 'B':
                    Console.WriteLine($"Balance: {Balance[currentUserIndex]}");
                    break;

                case 'W':
                    Console.WriteLine("Withdrawal selected (logic not yet implemented).");
                    break;

                case 'D':
                    Console.WriteLine("Deposit selected (logic not yet implemented).");
                    break;

                case 'T':
                    Console.WriteLine("Transfer selected (logic not yet implemented).");
                    break;

                case 'C':
                    Console.WriteLine("Returning to main menu...");
                    inMenu = false;
                    break;

                default:
                    Console.WriteLine("Invalid option.");
                    break;
            }

            if (inMenu)
            {
                Console.WriteLine("\nPress any key...");
                Console.ReadKey();
            }
        }
    }
}