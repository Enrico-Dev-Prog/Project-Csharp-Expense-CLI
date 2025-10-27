using System.Globalization;

record Expense(DateOnly Date, string Category, decimal Amount, string Note);

class Repo {
    const string Path = "Expenses.csv";
    public static List<Expense> Load() {
        if (!File.Exists(Path)) File.WriteAllText(Path, "date,category,amount,note\n");
        return File.ReadAllLines(Path).Skip(1).Where(l => !string.IsNullOrWhiteSpace(l))
            .Select(l => {
                var p = l.Split(',');
                return new Expense(DateOnly.Parse(p[0]), p[1], decimal.Parse(p[2], CultureInfo.InvariantCulture), p[3]);
            }).ToList();
    }
    public static void Save(Expense e) =>
        File.AppendAllText(Path, $"{e.Date:yyyy-MM-dd},{e.Category},{e.Amount.ToString(CultureInfo.InvariantCulture)},{e.Note}\n");
}

class App {
    static void Help() {
        Console.WriteLine("""
        Expense CLI
        add <YYYY-MM-DD> <categoria> <valor> <nota...>
        list [YYYY-MM]      -> lista mês
        sum  [YYYY-MM]      -> total mês
        """);
    }
    public static void Main(string[] args) {
        if (args.Length == 0) { Help(); return; }

        var cmd = args[0].ToLower();
        var data = Repo.Load();

        switch (cmd) {
            case "add":
                if (args.Length < 5) { Help(); return; }
                var date = DateOnly.Parse(args[1]);
                var cat = args[2];
                var amount = decimal.Parse(args[3], CultureInfo.InvariantCulture);
                var note = string.Join(' ', args.Skip(4));
                var e = new Expense(date, cat, amount, note);
                Repo.Save(e);
                Console.WriteLine("Ok, gasto registrado.");
                break;

            case "list":
                var ymL = args.Length > 1 ? args[1] : DateTime.Today.ToString("yyyy-MM");
                var items = data.Where(x => $"{x.Date:yyyy-MM}" == ymL).ToList();
                foreach (var i in items) Console.WriteLine($"{i.Date} | {i.Category,-10} | {i.Amount,8:C} | {i.Note}");
                Console.WriteLine($"Total: {items.Sum(x => x.Amount):C}");
                break;

            case "sum":
                var ymS = args.Length > 1 ? args[1] : DateTime.Today.ToString("yyyy-MM");
                Console.WriteLine(data.Where(x => $"{x.Date:yyyy-MM}" == ymS).Sum(x => x.Amount).ToString("C"));
                break;

            default: Help(); break;
        }
    }
}
