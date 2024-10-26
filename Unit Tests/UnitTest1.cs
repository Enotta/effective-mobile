namespace Unit_Tests
{
    [TestClass]
    public class UnitTests
    {
        List<Order> orders = new List<Order>() { 
            new Order(1, 1.0, "d1", DateTime.Parse("2024-12-01T17:14:21")), 
            new Order(2, 1.0, "d3", DateTime.Parse("2024-07-01T17:11:10")), 
            new Order(3, 1.0, "d5", DateTime.Parse("2024-03-01T17:09:12")), 
            new Order(4, 1.0, "d2", DateTime.Parse("2024-08-01T17:23:13")), 
            new Order(5, 1.0, "d2", DateTime.Parse("2024-09-01T17:17:14")), 
            new Order(6, 1.0, "d2", DateTime.Parse("2024-11-01T17:16:15")), 
            new Order(7, 1.0, "d4", DateTime.Parse("2024-01-01T17:18:16")), 
            new Order(8, 1.0, "d4", DateTime.Parse("2024-04-01T17:01:57")), 
            new Order(9, 1.0, "d5", DateTime.Parse("2024-02-01T17:02:58")), 
            new Order(10, 1.0, "d3", DateTime.Parse("2024-05-01T17:03:59"))
        };

        [TestMethod]
        public void FilterTest1()
        {
            List<Order> sortedOrders = OrderProcessor.FilterOrders(orders, "d1", DateTime.Parse("2022-03-01T17:09:12"));
            Assert.AreEqual(1, sortedOrders.Count);
        }

        [TestMethod]
        public void FilterTest2()
        {
            List<Order> sortedOrders = OrderProcessor.FilterOrders(orders, "", DateTime.Parse("2022-03-01T17:09:12"));
            Assert.AreEqual(0, sortedOrders.Count);
        }

        [TestMethod]
        public void FilterTest3()
        {
            List<Order> sortedOrders = OrderProcessor.FilterOrders(orders, "d2", DateTime.Parse("2025-03-01T17:09:12"));
            Assert.AreEqual(0, sortedOrders.Count);
        }

        [TestMethod]
        public void FilterTest4()
        {
            List<Order> sortedOrders = OrderProcessor.FilterOrders(orders, "d2", DateTime.Parse("2024-08-01T17:23:14"));
            Assert.AreEqual(2, sortedOrders.Count);
        }
    }
}
