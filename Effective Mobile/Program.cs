using System.Configuration;
using System.Text.Json;

/// <summary>
/// Класс-логгер. При инициализации создает/переписывает файл-логгер
/// </summary>
public class Logger
{
    private readonly string _filePath;

    public Logger(string path)
    {
        _filePath = path;
        File.WriteAllText(_filePath, string.Empty);
    }

    /// <summary>
    /// Записываем сообщение в лог
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public async Task Log(string message)
    {
        DateTime currentTime = DateTime.Now;
        Console.Write($"{currentTime}: {message}\n");
        await File.AppendAllTextAsync(_filePath, $"{currentTime}: {message}\n");
    }
}

/// <summary>
/// Класс-заказ
/// </summary>
public class Order
{
    public int Id { get; set; }
    public double Weight { get; set; }
    public string District { get; set; }
    public DateTime DeliveryTime { get; set; }

    public Order(int id, double weight, string district, DateTime deliveryTime)
    {
        Id = id;
        Weight = weight;
        District = district;
        DeliveryTime = deliveryTime;
    }
}

/// <summary>
/// Класс-процессор. Считывает заказы из файла, применяет к ним фмльтр, записывает результат в выходной файл
/// </summary>
public class OrderProcessor
{
    private readonly Logger _logger;

    public OrderProcessor(Logger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Фильтруем заказы согласно ТЗ
    /// </summary>
    /// <param name="orders"></param>
    /// <param name="cityDistrict"></param>
    /// <param name="firstDeliveryTime"></param>
    /// <returns></returns>
    public static List<Order> FilterOrders(List<Order> orders, string cityDistrict, DateTime firstDeliveryTime)
    {
        return orders.Where(o =>
            o.District == cityDistrict &&
            o.DeliveryTime >= firstDeliveryTime
        ).ToList();
    }

    /// <summary>
    /// Обрабатываем заказы из входящего файла (не забудьте закинуть его в папку с exe-файлом)
    /// </summary>
    /// <param name="inputFilePath"></param>
    /// <param name="outputFilePath"></param>
    /// <param name="logFilePath"></param>
    /// <param name="cityDistrict"></param>
    /// <param name="firstDeliveryDateTime"></param>
    /// <returns></returns>
    public async Task ProcessOrders(string inputFilePath, string outputFilePath, string logFilePath, string cityDistrict, DateTime firstDeliveryDateTime)
    {
        try
        {
            // Проверяем входные данные
            if (string.IsNullOrEmpty(inputFilePath) || string.IsNullOrEmpty(outputFilePath) || string.IsNullOrEmpty(logFilePath) ||
                string.IsNullOrEmpty(cityDistrict) || firstDeliveryDateTime == default)
            {
                await _logger.Log("Ошибка: Некорректные входные данные.");
                return;
            }

            // Считываем данные из файла
            List<Order> orders = ReadOrdersFromFile(inputFilePath).Result;

            // Фильтруем заказы
            List<Order> filteredOrders = FilterOrders(orders, cityDistrict, firstDeliveryDateTime);

            // Записываем результат
            await WriteOrdersToFile(filteredOrders, outputFilePath);

            await _logger.Log($"Обработано заказов: {orders.Count}");
            await _logger.Log($"Найдено подходящих заказов: {filteredOrders.Count}");
        }
        catch (Exception ex)
        {
            await _logger.Log($"Ошибка: {ex.Message}");
        }
    }

    /// <summary>
    /// Считываем заказы из файла. Заказы должны быть представлены в формате Json. Даты - ISO 8601-1:2019
    /// </summary>
    /// <param name="inputFilePath"></param>
    /// <returns></returns>
    private async Task<List<Order>> ReadOrdersFromFile(string inputFilePath)
    {
        List<Order> orders = new List<Order>();
        using (StreamReader reader = new StreamReader(inputFilePath))
        {
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                try
                {
                    Order? order = JsonSerializer.Deserialize<Order>(line);
                    orders.Add(order != null ? order : new Order(-1, -1.0, "failed read", DateTime.MinValue));
                }
                catch (Exception ex)
                {
                    await _logger.Log($"Ошибка: {ex.Message}");
                }
            }
        }

        return orders;
    }

    /// <summary>
    /// Записываем Json-репрезентацию заказов в результирующий файл
    /// </summary>
    /// <param name="filteredOrders"></param>
    /// <param name="outputFilePath"></param>
    /// <returns></returns>
    private async Task WriteOrdersToFile(List<Order> filteredOrders, string outputFilePath)
    {
        File.WriteAllText(outputFilePath, string.Empty);
        using (StreamWriter writer = new StreamWriter(outputFilePath))
        {
            foreach (Order order in filteredOrders)
            {
                try
                {
                    string line = JsonSerializer.Serialize<Order>(order);
                    writer.WriteLine(line);
                }
                catch(Exception ex)
                {
                    await _logger.Log($"Ошибка: {ex.Message}");
                }
            }
        }
    }
}

public class Program
{
    public static async Task Main()
    {
        // Получаем параметры из App.config
        string currentTime = DateTime.Now.ToString().Replace(":", "-");
        string? inputFilePath = "orders.txt";
        string? outputFilePath = "result " + currentTime + ".txt";
        string? logFilePath = "log " + currentTime + ".txt";
        string? cityDistrict = ConfigurationManager.AppSettings["DistrictFilter"];
        string? firstDeliveryDateTimeStr = ConfigurationManager.AppSettings["FirstDateFilter"];
        DateTime firstDeliveryDateTime;

        // Валидируем время
        if (!DateTime.TryParse(firstDeliveryDateTimeStr, out firstDeliveryDateTime))
        {
            Console.WriteLine("Ошибка: Некорректный формат времени первой доставки.");
            return;
        }

        // Инициализируем
        Logger logger = new(logFilePath);
        OrderProcessor orderProcessor = new(logger);

        // Обрабатываем заказы
        await orderProcessor.ProcessOrders(inputFilePath, outputFilePath, logFilePath, cityDistrict, firstDeliveryDateTime);
        Console.WriteLine();
    }
}