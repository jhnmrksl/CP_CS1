using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text.RegularExpressions;
using System.Xml.Linq;

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

    public virtual void Render()
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

    public virtual char Choice()
    {
        ConsoleKeyInfo keyInfo = Console.ReadKey(true);
        char choice = char.ToLower(keyInfo.KeyChar);
        Console.Write(char.ToUpper(choice));
        return choice;
    }

    protected string CenterLine(string text)
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

class BalanceInquiryMenu : Menu
{
    private string accountNumber;
    private string accountName;
    private string accountBalance;

    public BalanceInquiryMenu(string accNum, string accName, string accBal)
        : base("BALANCE INQUIRY", new string[] { }, "Press X to Exit")
    {
        accountNumber = accNum;
        accountName = accName;
        accountBalance = accBal;
    }

    public override void Render()
    {
        Console.WriteLine("╔" + new string('═', Width - 2) + "╗");
        Console.WriteLine("║" + CenterLine("DBJ") + "║");
        Console.WriteLine("║" + CenterLine("Digital Bank of JRU") + "║");
        Console.WriteLine("╠" + new string('═', Width - 2) + "╣");
        Console.WriteLine("║" + CenterLine(Title) + "║");


        Console.WriteLine("║" + new string(' ', Width - 2) + "║");
        Console.WriteLine("║" + CenterLine($"Account #: {accountNumber}") + "║");
        Console.WriteLine("║" + CenterLine($"Account Name: {accountName}") + "║");
        Console.WriteLine("║" + CenterLine($"Balance: {accountBalance}") + "║");

        Console.WriteLine("║" + new string(' ', Width - 2) + "║");
        Console.WriteLine("║" + CenterLine(Prompt) + "║");
        Console.WriteLine("╚" + new string('═', Width - 2) + "╝");
    }

    public override char Choice()
    {
        while (true)
        {
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);
            char choice = char.ToLower(keyInfo.KeyChar);

            if (choice == 'x')
            {
                return choice;
            }
        }
    }
}

class Program
{
    public static string[] AccNum = {
        "0000-000-0000", "0123-4567-8901", "2345-6789-0123", "3456-7890-1234", "4567-8901-2345", "5678-9012-3456"
    };

    public static string[] AccName = {
        "Admin" ,"Roel Richard", "Donie Marie", "Railee Darrel", "Railynne Dessirei", "Raine Dessirei"
    };

    public static string[] Balance = {
        "1000000", "5000.00", "0.00", "10000", "2500", "10000",
    };

    public static string[] PinNum = {
        "0000", "1111", "2222", "3333", "4444", "5555"
    };

    public static string[] Status = {
        "Active", "Active", "Blocked", "Active", "Active", "Active"
    };

    static int currentUserIndex = -1;

    public static void Accounts(Action<int> callback)
    {
        for (int i = 0; i < AccNum.Length; i++)
        {
            callback(i);
        }
        Console.WriteLine("Press any Key...");
        Console.ReadKey();
    }

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
                    Console.Clear();
                    new Menu("QUITTING", new[] { "Exiting Program" }, "Press any Key...").Render();
                    Console.ReadKey();
                    break;

                default:
                    Console.Clear();
                    new Menu("INCORRECT", new[] { "Invalid Option" }, "Press any Key...").Render();
                    Console.ReadKey();
                    break;
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
        Console.SetCursorPosition(13, Console.CursorTop);

        string? inputAcc = Console.ReadLine();


        currentUserIndex = Array.IndexOf(AccNum, inputAcc);

        if (currentUserIndex == -1)
        {
            Console.Clear();
            new Menu("LOGIN", new[] { "Account not Found." }, "Press any Key").Render();
            Console.ReadKey();
            return false;
        }

        if (Status[currentUserIndex] == "Blocked")
        {
            Console.Clear();
            new Menu("LOGIN", new[] { "This Account is Blocked." }, "Press any Key...").Render();
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
                ""
            );

            pinMenu.Render();
            Console.SetCursorPosition(18, Console.CursorTop);
            string pin = "";
            ConsoleKeyInfo key;

            while (true)
            {
                key = Console.ReadKey(true);

                // Quit
                if (key.KeyChar == 'q' || key.KeyChar == 'Q')
                {
                    Console.WriteLine();
                    return false;
                }

                // Enter = submit
                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }
                // Backspace
                else if (key.Key == ConsoleKey.Backspace && pin.Length > 0)
                {
                    pin = pin.Substring(0, pin.Length - 1);
                    Console.Write("\b \b");
                }
                // Normal input
                else if (!char.IsControl(key.KeyChar))
                {
                    pin += key.KeyChar;
                    Console.Write("*");
                }
            }

            if (inputAcc == "0000-000-0000")
            {
                if (pin == "0000")
                    Admin();
            }

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
            var transacMenu = new Menu(
                "SELECT TYPE OF TRANSACTION",
                new[] {
                    "B -> Balance Inquiry",
                    "W -> Withdrawal",
                    "D -> Deposit",
                    "T -> Transfer Fund",
                    "C -> Cancel"
                },
                "Enter transaction type: "

            );

            transacMenu.Render();
            char choice = char.ToUpper(Console.ReadKey().KeyChar);
            Console.WriteLine();

            switch (choice)
            {
                case 'B':
                    {
                        BalanceInquiryMenu();
                    }
                    break;

                case 'W':
                    {
                        Withdraw();
                    }
                    break;

                case 'D':
                    {
                        Deposit();
                    }
                    break;

                case 'T':
                    {
                        Transfer();
                    }
                    break;

                case 'C':
                    Console.Clear();
                    new Menu("", new[] { "Returning to Main Menu" }, "Press any key...").Render();
                    inMenu = false;
                    break;

                default:
                    Console.WriteLine("Invalid option.");
                    break;
            }
        }
    }

    //---------------- Balance Inquiry ----------------
    static void BalanceInquiryMenu()
    {
        Console.Clear();
        var balInqMenu = new BalanceInquiryMenu(
            AccNum[currentUserIndex],
            AccName[currentUserIndex],
            Balance[currentUserIndex]
            );

        bool inBalanceMenu = true;
        while (inBalanceMenu)
        {
            Console.Clear();
            balInqMenu.Render();
            char choice = balInqMenu.Choice();

            if (choice == 'x')
            {
                inBalanceMenu = false;
            }
        }
    }

    //---------------- Withdrawal ----------------
    static void Withdraw()
    {
        bool inMenu = true;

        while (inMenu)
        {
            Console.Clear();
            var withdrawMenu = new Menu(
                "WITHDRAW",
                new string[] { "Enter Amount to Withdraw" },
                ""

            );

            withdrawMenu.Render();
            Console.SetCursorPosition(17, Console.CursorTop);
            double currentBalance = double.Parse(Balance[currentUserIndex]);

            if (double.TryParse(Console.ReadLine(), out double withdrawAmount))
            {

                if (withdrawAmount > 0 && withdrawAmount <= currentBalance)
                {
                    if (withdrawAmount % 100 == 0)
                    {
                        currentBalance -= withdrawAmount;
                        Balance[currentUserIndex] = currentBalance.ToString("F2");

                        Console.Clear();
                        new Menu("WITHDRAW SUCCESS", new[] { $"New Balance: {Balance[currentUserIndex]}" }, "Press any key...").Render();
                        Console.ReadKey();
                        break;
                    }
                    else
                    {
                        Console.Clear();
                        new Menu("ERROR", new[] { "Withdraw must be Separable into 100s" }, "Press any key...").Render();
                        Console.ReadKey();
                    }
                }
                else
                {
                    if (withdrawAmount > currentBalance)
                    {
                        Console.Clear();
                        new Menu("ERROR", new[] { "Insufficient Balance" }, "Press any key...").Render();
                        Console.ReadKey();
                    }
                    else
                    {
                        Console.Clear();
                        new Menu("ERROR", new[] { "Invalid Amount" }, "Press any key...").Render();
                        Console.ReadKey();
                    }
                }
            }
            else
            {
                Console.Clear();
                new Menu("ERROR", new[] { "Invalid Input" }, "Press any key...").Render();
                Console.ReadKey();
            }
        }

    }

    //---------------- Deposit ----------------

    static void Deposit()
    {
        bool inMenu = true;

        while (inMenu)
        {
            Console.Clear();
            var depositMenu = new Menu(
                "DEPOSIT",
                new string[] { "Enter Amount to Deposit" },
                ""
            );

            depositMenu.Render();
            Console.SetCursorPosition(16, Console.CursorTop);
            double currentBalance = double.Parse(Balance[currentUserIndex]);

            if (double.TryParse(Console.ReadLine(), out double depositAmount))
            {

                if (depositAmount >= 100)
                {

                    currentBalance += depositAmount;
                    Balance[currentUserIndex] = currentBalance.ToString("F2");

                    Console.Clear();
                    new Menu("DEPOSIT SUCCESS", new[] { $"New Balance: {Balance[currentUserIndex]}" }, "Press any key...").Render();
                    Console.ReadKey();
                    break;
                }
                else
                {
                    Console.Clear();
                    new Menu("ERROR", new[] { "Invalid Amount" }, "Press any key...").Render();
                    Console.ReadKey();
                }
            }
            else
            {
                Console.Clear();
                new Menu("ERROR", new[] { "Invalid Input" }, "Press any key...").Render();
                Console.ReadKey();
            }
        }
    }

    //---------------- Transfer ----------------

    static void Transfer()
    {
        Console.Clear();
        bool inMenu = true;
        while (inMenu)
        {

            var transferMenu = new Menu(
                "TRANSFER FUND",
                new string[] { "Transfer to <Enter Account #>:" },
                ""
            );

            transferMenu.Render();
            Console.SetCursorPosition(12, Console.CursorTop);
            string? targetAcc = Console.ReadLine();

            if (targetAcc != null && targetAcc.ToUpper() == "X")
                return;

            int targetIndex = Array.IndexOf(AccNum, targetAcc);

            if (targetIndex == -1)
            {
                Console.Clear();
                new Menu("ERROR", new[] { "Account does not Exist." }, "Press any key...").Render();
                Console.ReadKey();
                return;
            }

            Console.Clear();

            var amountMenu = new Menu(
                "TRANSFER FUND",
                new string[] { "Amount:" },
                ""
            );

            amountMenu.Render();
            Console.SetCursorPosition(17, Console.CursorTop);

            string? inputAmount = Console.ReadLine();

            if (string.Equals(targetAcc, "X", StringComparison.OrdinalIgnoreCase))
                return;

            if (!double.TryParse(inputAmount, out double amount))
            {
                Console.Clear();
                new Menu("ERROR", new[] { "Invalid Amount." }, "Press any key...").Render();
                Console.ReadKey();
            }

            if (amount < 1000)
            {
                Console.Clear();
                new Menu("ERROR", new[] { "Minimum Transfer is 1000." }, "Press any key...").Render();
                Console.ReadKey();
            }

            double fee = (amount / 1000) * 25;
            double totalDeduction = amount + fee;

            double senderBalance = double.Parse(Balance[currentUserIndex]);

            if (senderBalance < totalDeduction)
            {
                Console.Clear();
                new Menu("ERROR", new[] { "Insufficient fund." }, "Press any key...").Render();
                Console.ReadKey();
            }

            senderBalance -= totalDeduction;
            Balance[currentUserIndex] = senderBalance.ToString("F2");

            double receiverBalance = double.Parse(Balance[targetIndex]);
            receiverBalance += amount;
            Balance[targetIndex] = receiverBalance.ToString("F2");

            bool isAdmin = AccName[currentUserIndex]
            .Equals("Admin", StringComparison.OrdinalIgnoreCase);

            Console.Clear();
            new Menu(
                "SUCCESS",
                new[]
                {
                "Transfer successful!",
                $"Transferred: {amount:F2}",
                $"Fee: {fee:F2}"
                },
                "Press any key..."
            ).Render();

            Console.ReadKey();

            if (isAdmin)
            {
                Admin();
            }
            else
            {
                break;
            }
        }
    }


    //---------------- Admin ----------------

    static void Admin()
    {
        Console.Clear();
        bool inMenu = true;

        while (inMenu)
        {
            Console.Clear();
            var adminMenu = new Menu(
                "SELECT TYPE OF TRANSACTION",
                new[] {
                    "1 - View Account Information",
                    "2 - Search Account",
                    "3 - Add New Account",
                    "4 - Edit Account Name",
                    "5 - Change Account PIN",
                    "6 - Transfer Fund",
                    "7 - Activate/Block Account",
                    "X - Cancel"
                },
                "Enter transaction type: "

            );

            adminMenu.Render();
            char choice = char.ToUpper(Console.ReadKey().KeyChar);
            Console.WriteLine();

            switch (choice)
            {
                case '1':
                    {
                        CustomInfo(i => new string[]
                        {
                            "Number: " + AccNum[i],
                            "Name: " + AccName[i],
                            "Balance: " + Balance[i],
                            "PIN: " + PinNum[i],
                            "Status: " + Status[i]
                        });
                    }
                    break;

                case '2':
                    {
                        SearchAccount();
                    }
                    break;

                case '3':
                    {
                        AddAccount();
                    }
                    break;

                case '4':
                    {
                        ChangeName();
                    }
                    break;

                case '5':
                    {
                        ChangePin();
                    }
                    break;

                case '6':
                    {
                        Transfer();
                    }
                    break;

                case '7':
                    {
                        SetStatus();
                    }
                    break;

                case 'X':
                    Console.Clear();
                    new Menu("", new[] { "Returning to Main Menu" }, "Press any key...").Render();
                    inMenu = false;
                    break;

                default:
                    Console.Clear();
                    new Menu("ERROR", new[] { "Invalid Input" }, "Press any key...").Render();
                    Console.ReadKey();
                    break;
            }
        }
    }

    //---------------- Customer Info ----------------
    public static void CustomInfo(Func<int, string[]> callback)
    {
        while (true)
        {
            Console.Clear();

            string[] options = new string[AccName.Length + 1];

            for (int i = 0; i < AccName.Length; i++)
            {
                options[i] = i + " - " + AccName[i];
            }

            options[AccName.Length] = "X - Cancel";

            var AccInfo = new Menu(
                "ACCOUNTS",
                options,
                "Enter choice: "
            );

            AccInfo.Render();
            string? input = Console.ReadLine();

            if (int.TryParse(input, out int index))
            {

                if (index >= 0 && index < AccName.Length)
                {
                    string[] info = callback(index);

                    Console.Clear();
                    var menu = new Menu(
                        "CUSTOMER INFORMATION",
                        info,
                        "Press any key to continue..."
                    );
                    menu.Render();
                    Console.ReadKey();
                    break;
                }
            }
            else if (input != null && input.ToUpper() == "X")
            {
                Console.Clear();
                break;
            }
            else
            {
                Console.Clear();
                new Menu("ERROR", new[] { "Invalid Input" }, "Press any key...").Render();
                Console.ReadKey();
                continue;
            }
        }
    }

    //---------------- Search Account ----------------
    static void SearchAccount()
    {
        while (true)
        {
            Console.Clear();
            var menu = new Menu(
                "SEARCH ACCOUNT",
                new[] { "Enter Account Number" },
                ""

                );
            menu.Render();
            Console.SetCursorPosition(13, Console.CursorTop);
            string? input = Console.ReadLine();

            int index = Array.IndexOf(AccNum, input);

            if (index != -1)
            {
                Console.Clear();

                new Menu(
                    "ACCOUNT FOUND",
                    new[]{
                "Number: " + AccNum[index],
                "Name: " + AccName[index],
                "Balance: " + Balance[index]
                    },
                    "Press any key..."
                ).Render();
                Console.ReadKey();
                break;
            }
            else
            {
                Console.Clear();
                new Menu("ERROR", new[] { "Account not Found." }, "Press any Key...").Render();
                Console.ReadKey();
                continue;
            }
        }
    }

    //---------------- Add New Account ----------------
    static void AddAccount()
    {
        bool inMenu = true;
        while (inMenu)
        {
            Console.Clear();
            var searchMenu = new Menu(
                "ADD ACCOUNT",
                new[] {
                    "Y - Yes",
                    "X - Quit"
                },
                "Press Y to Start or X to Return"
                );
            searchMenu.Render();
            char input = char.ToUpper(Console.ReadKey(true).KeyChar);

            if (input == 'Y')
            {

                Console.Clear();
                new Menu("CREATE ACCOUNT", new[] { "Enter Account Number (####-####-####):" }, "").Render();
                Console.SetCursorPosition(13, Console.CursorTop);

                string accNum = Console.ReadLine() ?? "";

                if (!Regex.IsMatch(accNum, @"^\d{4}-\d{4}-\d{4}$"))
                {
                    Console.Clear();
                    new Menu(
                        "ERROR",
                        new[] { "Invalid Format. Use ####-####-####" },
                        "Press any Key..."
                    ).Render();

                    Console.ReadKey();
                    continue;
                }
                if (AccNum.Contains(accNum))
                {
                    Console.Clear();
                    new Menu(
                        "ERROR",
                        new[] { "Account number already exists!" },
                        "Press any key..."
                    ).Render();

                    Console.ReadKey();
                    return;
                }

                Console.Clear();
                new Menu("CREATE ACCOUNT", new[] { "Enter Full Name:" }, "").Render();
                Console.SetCursorPosition(12, Console.CursorTop);
                string? name = Console.ReadLine() ?? "";
                if (!name.Replace(" ", "").All(char.IsLetter))
                {
                    Console.Clear();
                    new Menu(
                        "ERROR",
                        new[] { "Input must ONLY Contain Letters and Spaces" },
                        "Press any Key..."
                        ).Render();
                    Console.ReadKey();
                    return;
                }

                Console.Clear();
                new Menu("CREATE ACCOUNT", new[] { "Enter Balance:" }, "").Render();
                Console.SetCursorPosition(17, Console.CursorTop);
                string balanceInput = Console.ReadLine() ?? "";
                if (!int.TryParse(balanceInput, out int balance))
                {
                    Console.Clear();
                    new Menu(
                        "ERROR",
                        new[] { "Balance must be a Whole Number" },
                        "Press any Key..."
                    ).Render();
                    Console.ReadKey();
                    return;
                }


                Console.Clear();
                new Menu("CREATE ACCOUNT", new[] { "Enter PIN Number:" }, "").Render();
                Console.SetCursorPosition(18, Console.CursorTop);
                string? pin = Console.ReadLine() ?? "";
                if (!int.TryParse(pin, out _) || pin.Length != 4)
                {
                    Console.Clear();
                    new Menu(
                        "ERROR",
                        new[] { "PIN must be Exactly 4 Numbers" },
                        "Press any Key..."
                    ).Render();
                    Console.ReadKey();
                    return;
                }


                string? status = "Active";

                AccNum = AccNum.Append(accNum).ToArray();
                AccName = AccName.Append(name).ToArray();
                var balanceList = Balance.ToList();
                balanceList.Add(balance.ToString());
                Balance = balanceList.ToArray();
                PinNum = PinNum.Append(pin).ToArray();
                Status = Status.Append(status).ToArray();

                Console.Clear();
                new Menu("SUCCESS", new[] { "Account Created Successfully!" }, "Press any key...").Render();
                Console.ReadKey();
            }
            else if (input == 'X')
            {
                Console.Clear();
                break;
            }
            else
            {
                Console.Clear();
                new Menu("ERROR", new[] { "Invalid Input" }, "Press any key...").Render();
                Console.ReadKey();
                continue;
            }

        }
    }

    //---------------- Edit Account Info ----------------

    static void ChangeName()
    {
        while (true)
        {
            Console.Clear();
            new Menu("CHANGE ACCOUNT NAME", new[] { "Enter Account Number:" }, "").Render();

            Console.SetCursorPosition(12, Console.CursorTop);
            string accNumber = Console.ReadLine() ?? "";

            int index = Array.IndexOf(AccNum, accNumber);

            if (index == -1)
            {
                Console.Clear();
                new Menu("ERROR", new[] { "Account not found!" }, "Press any key...").Render();
                Console.ReadKey();
                continue;

            }

            Console.Clear();
            new Menu(
                "EDIT ACCOUNT NAME",
                new[] { $"Current Name: {AccName[index]}", "Enter New Name:" },
                ""
            ).Render();

            Console.SetCursorPosition(13, Console.CursorTop);
            string newName = Console.ReadLine() ?? "";

            if (!newName.Replace(" ", "").All(char.IsLetter))
            {
                Console.Clear();
                new Menu(
                    "ERROR",
                    new[] { "Input must ONLY Contain Letters and Spaces" },
                    "Press any Key..."
                    );
                Console.ReadKey();
                continue;
            }

            AccName[index] = newName;

            Console.Clear();
            new Menu("SUCCESS", new[] { "Account Name Updated!" }, "Press any Key...").Render();
            Console.ReadKey();
            break;
        }
    }

    //---------------- Edit Account Info ----------------

    static void ChangePin()
    {
        while (true)
        {
            Console.Clear();
            new Menu(
                "CHANGE PIN",
                new[] { "Enter Account Number:" },
                ""
            ).Render();

            Console.SetCursorPosition(13, Console.CursorTop);
            string accNumber = Console.ReadLine() ?? "";

            int index = Array.IndexOf(AccNum, accNumber);

            if (index == -1)
            {
                Console.Clear();
                new Menu("ERROR", new[] { "Account not Found!" }, "Press any Key...").Render();
                Console.ReadKey();
                continue;
            }

            Console.Clear();
            new Menu(
                "CHANGE PIN",
                new[]
                {
                $"Account: {AccNum[index]}",
                "Enter New 4-Digit PIN:"
                },
                ""
            ).Render();

            Console.SetCursorPosition(18, Console.CursorTop);
            string newPin = Console.ReadLine() ?? "";

            if (!int.TryParse(newPin, out _) || newPin.Length != 4)
            {
                Console.Clear();
                new Menu(
                    "ERROR",
                    new[] { "PIN MUST Exactly be 4 Digits" },
                    "Press any key..."
                ).Render();
                Console.ReadKey();
                continue;
            }

            PinNum[index] = newPin;

            Console.Clear();
            new Menu("SUCCESS", new[] { "PIN Updated Successfully!" }, "Press any Key...").Render();
            Console.ReadKey();
            break;
        }
    }

    //---------------- Edit Account Info ----------------

    static void SetStatus()
    {
        while (true)
        {
            Console.Clear();
            new Menu(
                "CHANGE ACCOUNT STATUS",
                new[] { "Enter Account Number (X to cancel):" },
                ""
            ).Render();

            Console.SetCursorPosition(13, Console.CursorTop);
            string accNumber = Console.ReadLine() ?? "";

            if (accNumber.ToUpper() == "X")
                return;

            int index = Array.IndexOf(AccNum, accNumber);

            if (index == -1)
            {
                Console.Clear();
                new Menu("ERROR", new[] { "Account not Found!" }, "Press any Key...").Render();
                Console.ReadKey();
                continue;
            }

            string newStatus = (Status[index] == "Active") ? "Blocked" : "Active";

            Console.Clear();
            new Menu(
                "CONFIRMATION",
                new[]
                {
            $"Current Status: {Status[index]}",
            $"New Status: {newStatus}",
            "",
            "Y - Confirm",
            "X - Cancel"
                },
                ""
            ).Render();

            char input = char.ToUpper(Console.ReadKey(true).KeyChar);

            if (input == 'X')
                return;

            if (input == 'Y')
            {
                Status[index] = newStatus;

                Console.Clear();
                new Menu(
                    "SUCCESS",
                    new[]
                    {
                $"Status updated to {Status[index]}"
                    },
                    "Press any key..."
                ).Render();

                Console.ReadKey();
                return;
            }
        }
    }
}
