using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace MultiLevelIncludeQueries
{
    class Program
    {
        public static void Main(string[] args)
        {
            using (var db = new MyContext())
            {
                // Recreate database
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                // Seed database
                db.AddRange(new Customer
                {
                    Address = new Address(),
                    Orders = new List<Order>
                    {
                        new Order
                        {
                            OrderDiscount = new OrderDiscount(),
                            OrderDetails = new List<OrderDetail>
                            {
                                new OrderDetail(),
                                new OrderDetail()
                            }
                        },
                        new Order
                        {
                            OrderDiscount = new OrderDiscount(),
                            OrderDetails = new List<OrderDetail>
                            {
                                new OrderDetail(),
                                new OrderDetail()
                            }
                        },
                        new Order
                        {
                            OrderDiscount = new OrderDiscount()
                        },
                        new Order()
                    },
                },
                new Customer
                {
                    Address = new Address()
                },
                new Customer());

                db.SaveChanges();
            }

            using (var db = new MyContext())
            {
                //db.ChangeTracker.AutoDetectChangesEnabled = false;
                db.ChangeTracker.LazyLoadingEnabled = false;

                // Run queries
                // Tracking and Buffered
                Console.WriteLine("Tracking & Buffering");

                db.Customers.Include(c => c.Address).Load();
                var tmp22 = db.Customers.ToList();
                var tmp1 = db.Customers.Select(c => c.Address).ToList();

                var query = db.Customers.Include(c => c.Address);
                var result = query.ToList();

                var tmp = result.Select(c => c.Address).ToList();

                query.Include(c => c.Orders).ThenInclude(o => o.OrderDiscount).SelectMany(c => c.Orders).Load();
                query.SelectMany(c => c.Orders).SelectMany(o => o.OrderDetails).Load();

                // Following code is just to print out, above will run queries and stitch up graph
                // Since Include is not used for collection navigations,
                // the collection properties may be null if no related objects & not initialized
                foreach (var customer in result)
                {
                    Console.WriteLine($"CustomerId: {customer.Id}");
                    Console.WriteLine($"Customer Address: {customer.Address?.Id}");
                    if (customer.Orders != null)
                    {
                        Console.WriteLine($"Customer Orders.Count: {customer.Orders.Count}");

                        foreach (var order in customer.Orders)
                        {
                            Console.WriteLine($"OrderId: {order.Id}");
                            Console.WriteLine($"Order OrderDiscount: {order.OrderDiscount?.Id}");

                            if (order.OrderDetails != null)
                            {
                                Console.WriteLine($"Order OrderDetails.Count: {order.OrderDetails?.Count}");

                                foreach (var orderDetail in order.OrderDetails)
                                {
                                    Console.WriteLine($"OrderDetailId: {orderDetail.Id}");
                                }
                            }
                        }
                    }
                }
            }

            using (var db = new MyContext())
            {
                // Run queries
                // Tracking and non-buffered
                Console.WriteLine("Tracking & Non-buffering");
                var customers = db.Customers.Include(c => c.Address);
                var orders = customers.Include(c => c.Orders).ThenInclude(o => o.OrderDiscount).SelectMany(c => c.Orders).GetEnumerator();
                orders.MoveNext();
                var orderDetails = customers.SelectMany(c => c.Orders).SelectMany(o => o.OrderDetails).GetEnumerator();
                orderDetails.MoveNext();

                // Above will run queries and get enumerators, following code will actually enumerate.
                // The following code blocks will move each enumerators upto the point it is needed to generate the current result
                // Since Include is not used for collection navigations,
                // the collection properties may be null if no related objects & not initialized
                foreach (var customer in customers)
                {
                    Console.WriteLine($"CustomerId: {customer.Id}");
                    Console.WriteLine($"Customer Address: {customer.Address?.Id}");

                    while (orders.Current?.CustomerId == customer.Id)
                    {
                        // Enumerate orders as long as the order is related to customer
                        if (!orders.MoveNext())
                        {
                            break;
                        }
                    }

                    if (customer.Orders != null)
                    {
                        Console.WriteLine($"Customer Orders.Count: {customer.Orders.Count}");

                        foreach (var order in customer.Orders)
                        {
                            Console.WriteLine($"OrderId: {order.Id}");
                            Console.WriteLine($"Order OrderDiscount: {order.OrderDiscount?.Id}");

                            while (orderDetails.Current?.OrderId == order.Id)
                            {
                                // Enumerate orderDetails as long as the orderDetail is related to order
                                if (!orderDetails.MoveNext())
                                {
                                    break;
                                }
                            }

                            if (order.OrderDetails != null)
                            {
                                Console.WriteLine($"Order OrderDetails.Count: {order.OrderDetails.Count}");

                                foreach (var orderDetail in order.OrderDetails)
                                {
                                    Console.WriteLine($"OrderDetailId: {orderDetail.Id}");
                                }
                            }
                        }
                    }
                }

                orders.Dispose();
                orderDetails.Dispose();
            }

            using (var db = new MyContext())
            {
                // Run queries
                // Non-tracking
                Console.WriteLine("Non-tracking");
                var customers = db.Customers.Include(c => c.Address).AsNoTracking();
                var orders = customers.Include(c => c.Orders).ThenInclude(o => o.OrderDiscount).SelectMany(c => c.Orders)
                    .Select(o => new
                    {
                        // We connect order to related customer by comparing value of FK to PK.
                        // If FK property is not shadow then this custom projection is not necessary as you can access o.CustomerId
                        // If FK property is shadow then project out FK value and use it for comparison.
                        o.CustomerId, // For shadow property use EF.Property<int>(o, "CustomerId")
                        o
                    }).GetEnumerator();
                orders.MoveNext();
                var orderDetails = customers.SelectMany(c => c.Orders).SelectMany(o => o.OrderDetails)
                    .Select(od => new
                    {
                        od.OrderId,
                        od
                    })
                    .GetEnumerator();
                orderDetails.MoveNext();

                // Above will run queries and get enumerators, following code will actually enumerate.
                // The following code blocks will move each enumerators upto the point it is needed to generate the current result
                // And stitch up navigations.
                // If you want to buffer the result, create collection to store top level objects.

                foreach (var customer in customers)
                {
                    Console.WriteLine($"CustomerId: {customer.Id}");
                    Console.WriteLine($"Customer Address: {customer.Address?.Id}");
                    customer.Orders = new List<Order>();

                    while (orders.Current?.CustomerId == customer.Id)
                    {
                        // Add order to collection
                        customer.Orders.Add(orders.Current.o);
                        // Set inverse navigation to customer
                        orders.Current.o.Customer = customer;
                        // Enumerate orders as long as the order is related to customer
                        if (!orders.MoveNext())
                        {
                            break;
                        }
                    }

                    Console.WriteLine($"Customer Orders.Count: {customer.Orders.Count}");

                    foreach (var order in customer.Orders)
                    {
                        Console.WriteLine($"OrderId: {order.Id}");
                        Console.WriteLine($"Order OrderDiscount: {order.OrderDiscount?.Id}");
                        order.OrderDetails = new List<OrderDetail>();

                        while (orderDetails.Current?.OrderId == order.Id)
                        {
                            // Add orderDetail to collection
                            order.OrderDetails.Add(orderDetails.Current.od);
                            // Set inverse navigation to order
                            orderDetails.Current.od.Order = order;
                            // Enumerate orderDetails as long as the orderDetail is related to order
                            if (!orderDetails.MoveNext())
                            {
                                break;
                            }
                        }

                        Console.WriteLine($"Order OrderDetails.Count: {order.OrderDetails.Count}");

                        foreach (var orderDetail in order.OrderDetails)
                        {
                            Console.WriteLine($"OrderDetailId: {orderDetail.Id}");
                        }
                    }
                }

                orders.Dispose();
                orderDetails.Dispose();
            }

            Console.WriteLine("Program finished.");
        }
    }

    public class MyContext : DbContext
    {
        public DbSet<Customer> Customers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Select 1 provider
            optionsBuilder
                .UseSqlServer(@"Server=MSI;Database=_ModelApp;Trusted_Connection=True;Connect Timeout=5;ConnectRetryCount=0;MultipleActiveResultSets=true");
        }
    }

    public class Customer
    {
        public int Id { get; set; }
        public virtual Address Address { get; set; }
        public ICollection<Order> Orders { get; set; }
    }

    public class Address
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public virtual Customer Customer { get; set; }
    }

    public class Order
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public virtual Customer Customer { get; set; }
        public virtual OrderDiscount OrderDiscount { get; set; }
        public ICollection<OrderDetail> OrderDetails { get; set; }
    }

    public class OrderDetail
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; }
    }

    public class OrderDiscount
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; }
    }
}
