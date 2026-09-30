using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Restaurant_Management.Models.Entities;

namespace Restaurant_Management.Data
{
    public static class DbInitializer
    {
        public static void Initialize(RestaurantDbContext context)
        {
            context.Database.EnsureCreated();

            // Check if DB already seeded
            if (context.Categories.Any())
            {
                // Ensure existing dishes have verified working image URLs
                var friesItem = context.MenuItems.FirstOrDefault(m => m.Name.Contains("Fries"));
                if (friesItem != null && (friesItem.ImageUrl == null || friesItem.ImageUrl.Contains("1576107232684-1279f3908594")))
                {
                    friesItem.ImageUrl = "https://images.unsplash.com/photo-1573080496219-bb080dd4f877?auto=format&fit=crop&w=800&q=80";
                }

                var arrabbiataItem = context.MenuItems.FirstOrDefault(m => m.Name.Contains("Arrabbiata"));
                if (arrabbiataItem != null && (arrabbiataItem.ImageUrl == null || arrabbiataItem.ImageUrl.Contains("1621996346565-e3d5d6281057")))
                {
                    arrabbiataItem.ImageUrl = "https://images.unsplash.com/photo-1608897013039-887f21d8c804?auto=format&fit=crop&w=800&q=80";
                }

                context.SaveChanges();
                return; // DB has been seeded
            }

            // 1. Seed Roles
            var adminRole = new Role { Name = "Admin" };
            var customerRole = new Role { Name = "Customer" };
            context.Roles.AddRange(adminRole, customerRole);
            context.SaveChanges();

            // 2. Seed Users
            var hasher = new PasswordHasher<User>();

            var adminUser = new User
            {
                FullName = "Restaurant Administrator",
                Email = "admin@restaurant.com",
                PhoneNumber = "+91 98765 43210",
                RoleId = adminRole.RoleId,
                CreatedAt = DateTime.UtcNow.AddMonths(-3)
            };
            adminUser.PasswordHash = hasher.HashPassword(adminUser, "Admin@123");

            var customerUser = new User
            {
                FullName = "Rahul Sharma",
                Email = "customer@restaurant.com",
                PhoneNumber = "+91 98123 45678",
                RoleId = customerRole.RoleId,
                CreatedAt = DateTime.UtcNow.AddMonths(-2)
            };
            customerUser.PasswordHash = hasher.HashPassword(customerUser, "Customer@123");

            var customerUser2 = new User
            {
                FullName = "Priya Patel",
                Email = "priya@example.com",
                PhoneNumber = "+91 97234 56789",
                RoleId = customerRole.RoleId,
                CreatedAt = DateTime.UtcNow.AddMonths(-1)
            };
            customerUser2.PasswordHash = hasher.HashPassword(customerUser2, "Customer@123");

            context.Users.AddRange(adminUser, customerUser, customerUser2);
            context.SaveChanges();

            // 3. Seed Categories
            var categories = new List<Category>
            {
                new Category
                {
                    Name = "Starters & Appetizers",
                    Description = "Crispy, savory delights to tantalize your palate before the feast.",
                    ImageUrl = "https://images.unsplash.com/photo-1541529086526-db283c563270?auto=format&fit=crop&w=800&q=80",
                    DisplayOrder = 1
                },
                new Category
                {
                    Name = "Wood-Fired Pizzas",
                    Description = "Handcrafted dough baked to crisp perfection with premium artisanal toppings.",
                    ImageUrl = "https://images.unsplash.com/photo-1513104890138-7c749659a591?auto=format&fit=crop&w=800&q=80",
                    DisplayOrder = 2
                },
                new Category
                {
                    Name = "Gourmet Pastas & Burgers",
                    Description = "Rich pasta tossed in handmade sauces and brioche burgers stacked with flavor.",
                    ImageUrl = "https://images.unsplash.com/photo-1550547660-d9450f859349?auto=format&fit=crop&w=800&q=80",
                    DisplayOrder = 3
                },
                new Category
                {
                    Name = "Chef's Signature Mains",
                    Description = "Exquisite pan-seared meats, aromatic royal curries, and gourmet platters.",
                    ImageUrl = "https://images.unsplash.com/photo-1544025162-d76694265947?auto=format&fit=crop&w=800&q=80",
                    DisplayOrder = 4
                },
                new Category
                {
                    Name = "Decadent Desserts",
                    Description = "Velvety confections, molten chocolate delights, and artisan gelatos.",
                    ImageUrl = "https://images.unsplash.com/photo-1551024709-8f23befc6f87?auto=format&fit=crop&w=800&q=80",
                    DisplayOrder = 5
                },
                new Category
                {
                    Name = "Handcrafted Beverages",
                    Description = "Refreshing signature coolers, botanical mocktails, and barista roasted coffees.",
                    ImageUrl = "https://images.unsplash.com/photo-1551024709-8f23befc6f87?auto=format&fit=crop&w=800&q=80",
                    DisplayOrder = 6
                }
            };
            context.Categories.AddRange(categories);
            context.SaveChanges();

            // 4. Seed Menu Items
            var menuItems = new List<MenuItem>
            {
                // Starters
                new MenuItem
                {
                    CategoryId = categories[0].CategoryId,
                    Name = "Truffle Parmesan Herb Fries",
                    Description = "Crispy golden russet potato wedges infused with white truffle oil, grated aged parmesan, and rosemary.",
                    Price = 280.00m,
                    ImageUrl = "https://images.unsplash.com/photo-1573080496219-bb080dd4f877?auto=format&fit=crop&w=800&q=80",
                    IsVegetarian = true,
                    SpicyLevel = 0,
                    PreparationTimeMinutes = 12
                },
                new MenuItem
                {
                    CategoryId = categories[0].CategoryId,
                    Name = "Smoked Paneer Tikka Bruschetta",
                    Description = "Tandoor charred cottage cheese cubes on garlic toasted rustic sourdough with mint mousse and pomegranate pearls.",
                    Price = 340.00m,
                    ImageUrl = "https://images.unsplash.com/photo-1567188040759-fb8a883dc6d8?auto=format&fit=crop&w=800&q=80",
                    IsVegetarian = true,
                    SpicyLevel = 1,
                    PreparationTimeMinutes = 15
                },
                new MenuItem
                {
                    CategoryId = categories[0].CategoryId,
                    Name = "Peri-Peri Crispy Chicken Wings",
                    Description = "Double-fried succulent chicken wings glazed in our fiery signature house-made African bird's eye chili glaze.",
                    Price = 390.00m,
                    ImageUrl = "https://images.unsplash.com/photo-1567620832903-9fc6debc209f?auto=format&fit=crop&w=800&q=80",
                    IsVegetarian = false,
                    SpicyLevel = 3,
                    PreparationTimeMinutes = 18
                },

                // Pizzas
                new MenuItem
                {
                    CategoryId = categories[1].CategoryId,
                    Name = "Margherita D.O.P. Classica",
                    Description = "San Marzano tomato sauce, fior di latte mozzarella, fresh fragrant basil leaves, and cold-pressed EVOO.",
                    Price = 450.00m,
                    ImageUrl = "https://images.unsplash.com/photo-1604382354936-07c5d9983bd3?auto=format&fit=crop&w=800&q=80",
                    IsVegetarian = true,
                    SpicyLevel = 0,
                    PreparationTimeMinutes = 16
                },
                new MenuItem
                {
                    CategoryId = categories[1].CategoryId,
                    Name = "Spicy Pepperoni & Hot Honey",
                    Description = "Crispy cured beef pepperoni cups, crushed chili flakes, smoked provolone, finished with hot chili wildflower honey.",
                    Price = 590.00m,
                    ImageUrl = "https://images.unsplash.com/photo-1628840042765-356cda07504e?auto=format&fit=crop&w=800&q=80",
                    IsVegetarian = false,
                    SpicyLevel = 2,
                    PreparationTimeMinutes = 18
                },
                new MenuItem
                {
                    CategoryId = categories[1].CategoryId,
                    Name = "Wild Forest Mushroom & Truffle",
                    Description = "Sauteed cremini and shiitake mushrooms, caramelized shallots, gorgonzola cheese, and white truffle cream drizzle.",
                    Price = 520.00m,
                    ImageUrl = "https://images.unsplash.com/photo-1574071318508-1cdbab80d002?auto=format&fit=crop&w=800&q=80",
                    IsVegetarian = true,
                    SpicyLevel = 0,
                    PreparationTimeMinutes = 18
                },

                // Pastas & Burgers
                new MenuItem
                {
                    CategoryId = categories[2].CategoryId,
                    Name = "Creamy Wild Mushroom Fettuccine",
                    Description = "Al dente fresh ribbon pasta tossed in rich garlic parmesan cream with wild portobello and fresh thyme.",
                    Price = 420.00m,
                    ImageUrl = "https://images.unsplash.com/photo-1645112411341-6c4fd023714a?auto=format&fit=crop&w=800&q=80",
                    IsVegetarian = true,
                    SpicyLevel = 0,
                    PreparationTimeMinutes = 15
                },
                new MenuItem
                {
                    CategoryId = categories[2].CategoryId,
                    Name = "The Grand Smoked Smash Burger",
                    Description = "Double prime grilled patties, double melted cheddar, caramelized balsamic onions, gherkins on toasted brioche.",
                    Price = 480.00m,
                    ImageUrl = "https://images.unsplash.com/photo-1568901346375-23c9450c58cd?auto=format&fit=crop&w=800&q=80",
                    IsVegetarian = false,
                    SpicyLevel = 1,
                    PreparationTimeMinutes = 18
                },
                new MenuItem
                {
                    CategoryId = categories[2].CategoryId,
                    Name = "Penne Arrabbiata Con Burrata",
                    Description = "Quill-shaped durum pasta in spicy sun-ripened tomato basil sauce, crowned with an entire fresh Italian burrata sphere.",
                    Price = 460.00m,
                    ImageUrl = "https://images.unsplash.com/photo-1608897013039-887f21d8c804?auto=format&fit=crop&w=800&q=80",
                    IsVegetarian = true,
                    SpicyLevel = 2,
                    PreparationTimeMinutes = 16
                },

                // Chef's Mains
                new MenuItem
                {
                    CategoryId = categories[3].CategoryId,
                    Name = "Old Delhi Butter Chicken Royal",
                    Description = "Charcoal smoked boneless chicken simmered in rich satin makhani gravy infused with kasuri methi and butter.",
                    Price = 540.00m,
                    ImageUrl = "https://images.unsplash.com/photo-1588166524941-3bf61a9c41db?auto=format&fit=crop&w=800&q=80",
                    IsVegetarian = false,
                    SpicyLevel = 1,
                    PreparationTimeMinutes = 20
                },
                new MenuItem
                {
                    CategoryId = categories[3].CategoryId,
                    Name = "Shahi Paneer Lababdar",
                    Description = "Creamy cottage cheese blocks cooked in cashew cream and onion-tomato reduction with roasted coriander seeds.",
                    Price = 460.00m,
                    ImageUrl = "https://images.unsplash.com/photo-1631452180519-c014fe946bc7?auto=format&fit=crop&w=800&q=80",
                    IsVegetarian = true,
                    SpicyLevel = 1,
                    PreparationTimeMinutes = 18
                },
                new MenuItem
                {
                    CategoryId = categories[3].CategoryId,
                    Name = "Herb Crusted Atlantic Salmon",
                    Description = "Pan-seared salmon fillet over saffron mashed potato mousseline, asparagus spears, and lemon caper emulsion.",
                    Price = 780.00m,
                    ImageUrl = "https://images.unsplash.com/photo-1467003909585-2f8a72700288?auto=format&fit=crop&w=800&q=80",
                    IsVegetarian = false,
                    SpicyLevel = 0,
                    PreparationTimeMinutes = 22
                },

                // Desserts
                new MenuItem
                {
                    CategoryId = categories[4].CategoryId,
                    Name = "Classic Venetian Tiramisu",
                    Description = "Savoiardi ladyfingers soaked in dark espresso and marsala, layered with whipped mascarpone and Valrhona cocoa.",
                    Price = 320.00m,
                    ImageUrl = "https://images.unsplash.com/photo-1571877227200-a0d98ea607e9?auto=format&fit=crop&w=800&q=80",
                    IsVegetarian = true,
                    SpicyLevel = 0,
                    PreparationTimeMinutes = 8
                },
                new MenuItem
                {
                    CategoryId = categories[4].CategoryId,
                    Name = "Belgian Molten Chocolate Lava Cake",
                    Description = "Warm baked dark chocolate fondant with gooey center, served alongside Tahitian vanilla bean gelato.",
                    Price = 350.00m,
                    ImageUrl = "https://images.unsplash.com/photo-1606313564200-e75d5e30476c?auto=format&fit=crop&w=800&q=80",
                    IsVegetarian = true,
                    SpicyLevel = 0,
                    PreparationTimeMinutes = 14
                },
                new MenuItem
                {
                    CategoryId = categories[4].CategoryId,
                    Name = "Saffron Pistachio Baklava Sundae",
                    Description = "Crispy golden filo pastry triangles infused with wildflower honey syrup and crushed pistachios, with saffron kulfi.",
                    Price = 290.00m,
                    ImageUrl = "https://images.unsplash.com/photo-1519869325930-281384150729?auto=format&fit=crop&w=800&q=80",
                    IsVegetarian = true,
                    SpicyLevel = 0,
                    PreparationTimeMinutes = 10
                },

                // Beverages
                new MenuItem
                {
                    CategoryId = categories[5].CategoryId,
                    Name = "Electric Blue Curacao Mocktail",
                    Description = "Refreshing blue citrus liqueur blend with crushed mint leaves, fresh lime juice, club soda, and passion fruit foam.",
                    Price = 220.00m,
                    ImageUrl = "https://images.unsplash.com/photo-1513558161293-cdaf765ed2fd?auto=format&fit=crop&w=800&q=80",
                    IsVegetarian = true,
                    SpicyLevel = 0,
                    PreparationTimeMinutes = 5
                },
                new MenuItem
                {
                    CategoryId = categories[5].CategoryId,
                    Name = "Wild Berry Basil Infused Cooler",
                    Description = "Muddled forest blackberries, strawberries, and sweet Italian basil shaken with organic cranberry juice and sparkling tonic.",
                    Price = 240.00m,
                    ImageUrl = "https://images.unsplash.com/photo-1536935338788-846bb9981813?auto=format&fit=crop&w=800&q=80",
                    IsVegetarian = true,
                    SpicyLevel = 0,
                    PreparationTimeMinutes = 5
                },
                new MenuItem
                {
                    CategoryId = categories[5].CategoryId,
                    Name = "Caramel Hazelnut Cold Brew Affogato",
                    Description = "Slow-steeped single origin Arabica cold brew poured over artisan caramel gelato and topped with roasted hazelnuts.",
                    Price = 260.00m,
                    ImageUrl = "https://images.unsplash.com/photo-1517701550927-30cf4ba1dba5?auto=format&fit=crop&w=800&q=80",
                    IsVegetarian = true,
                    SpicyLevel = 0,
                    PreparationTimeMinutes = 6
                }
            };
            context.MenuItems.AddRange(menuItems);
            context.SaveChanges();

            // 5. Seed Dining Tables
            var tables = new List<DiningTable>
            {
                new DiningTable { TableNumber = "T-01", Capacity = 2, Location = "Window View", Status = "Available" },
                new DiningTable { TableNumber = "T-02", Capacity = 2, Location = "Window View", Status = "Available" },
                new DiningTable { TableNumber = "T-03", Capacity = 2, Location = "Romantic Booth", Status = "Reserved" },
                new DiningTable { TableNumber = "T-04", Capacity = 4, Location = "Main Dining", Status = "Occupied" },
                new DiningTable { TableNumber = "T-05", Capacity = 4, Location = "Main Dining", Status = "Available" },
                new DiningTable { TableNumber = "T-06", Capacity = 4, Location = "Main Dining", Status = "Available" },
                new DiningTable { TableNumber = "T-07", Capacity = 4, Location = "Garden Patio", Status = "Available" },
                new DiningTable { TableNumber = "T-08", Capacity = 6, Location = "Garden Patio", Status = "Available" },
                new DiningTable { TableNumber = "T-09", Capacity = 6, Location = "Family Lounge", Status = "Available" },
                new DiningTable { TableNumber = "T-10", Capacity = 6, Location = "Family Lounge", Status = "Reserved" },
                new DiningTable { TableNumber = "T-11", Capacity = 8, Location = "VIP Royal Suite", Status = "Available" },
                new DiningTable { TableNumber = "T-12", Capacity = 12, Location = "VIP Banquet Hall", Status = "Available" }
            };
            context.DiningTables.AddRange(tables);
            context.SaveChanges();

            // 6. Seed Realistic Historical Orders across different peak hours & days
            var rnd = new Random(42);
            var now = DateTime.UtcNow;
            var days = new[] { 0, 1, 2, 3, 4, 5, 6 };
            var peakHours = new[] { 12, 13, 13, 14, 15, 18, 19, 19, 20, 20, 21, 21, 22 };

            int orderSequence = 1001;
            var sampleOrders = new List<Order>();

            foreach (var dayOffset in days)
            {
                var baseDay = now.AddDays(-dayOffset).Date;
                foreach (var hour in peakHours)
                {
                    var orderTime = baseDay.AddHours(hour).AddMinutes(rnd.Next(2, 58));
                    var selectedTable = tables[rnd.Next(tables.Count)];
                    
                    var order = new Order
                    {
                        OrderNumber = $"ORD-2026-{orderSequence++}",
                        UserId = (orderSequence % 3 == 0) ? customerUser.UserId : ((orderSequence % 3 == 1) ? customerUser2.UserId : null),
                        TableId = selectedTable.TableId,
                        OrderType = (rnd.Next(10) > 2) ? "Dine-In" : "Takeaway",
                        OrderDate = orderTime,
                        OrderStatus = dayOffset == 0 && hour >= now.Hour ? "Preparing" : "Completed",
                        CustomerName = (orderSequence % 3 == 0) ? customerUser.FullName : (orderSequence % 3 == 1 ? customerUser2.FullName : $"Guest #{orderSequence}"),
                        CustomerPhone = "+91 98" + rnd.Next(10000000, 99999999),
                        CustomerEmail = (orderSequence % 3 == 0) ? customerUser.Email : (orderSequence % 3 == 1 ? customerUser2.Email : $"guest{orderSequence}@domain.com"),
                        OrderNotes = (rnd.Next(4) == 0) ? "Please keep it mildly spiced." : null
                    };

                    // Add 2 to 4 items
                    int itemCount = rnd.Next(2, 5);
                    decimal subTotal = 0;
                    var addedItems = new HashSet<int>();

                    for (int i = 0; i < itemCount; i++)
                    {
                        var mItem = menuItems[rnd.Next(menuItems.Count)];
                        if (addedItems.Contains(mItem.MenuItemId)) continue;
                        addedItems.Add(mItem.MenuItemId);

                        int qty = rnd.Next(1, 4);
                        decimal line = mItem.Price * qty;
                        subTotal += line;

                        order.OrderItems.Add(new OrderItem
                        {
                            MenuItemId = mItem.MenuItemId,
                            Quantity = qty,
                            UnitPrice = mItem.Price,
                            LineTotal = line
                        });
                    }

                    order.SubTotal = subTotal;
                    order.TaxAmount = Math.Round(subTotal * 0.05m, 2); // 5% GST
                    order.TotalAmount = order.SubTotal + order.TaxAmount;

                    // Payment
                    var paymentMethods = new[] { "Credit Card", "UPI Payment", "NetBanking", "Cash POS" };
                    order.PaymentTransaction = new PaymentTransaction
                    {
                        TransactionReference = $"TXN-2026{orderTime:MMdd}-{rnd.Next(100000, 999999)}",
                        PaymentMethod = paymentMethods[rnd.Next(paymentMethods.Length)],
                        Amount = order.TotalAmount,
                        PaymentDate = orderTime.AddMinutes(rnd.Next(10, 30)),
                        Status = "Completed",
                        CardLastFour = "4242",
                        UpiId = "user@upi",
                        GatewayAuthCode = "AUTH" + rnd.Next(10000, 99999)
                    };

                    sampleOrders.Add(order);
                }
            }

            context.Orders.AddRange(sampleOrders);
            context.SaveChanges();

            // 7. Seed Table Reservations
            var reservations = new List<TableReservation>
            {
                new TableReservation
                {
                    TableId = tables[2].TableId, // T-03
                    UserId = customerUser.UserId,
                    CustomerName = "Rahul Sharma",
                    CustomerEmail = "customer@restaurant.com",
                    CustomerPhone = "+91 98123 45678",
                    ReservationDate = DateTime.Today,
                    TimeSlot = "19:30 - 21:00",
                    GuestCount = 2,
                    SpecialRequests = "Anniversary celebration table requested by the window with candle setup.",
                    Status = "Confirmed"
                },
                new TableReservation
                {
                    TableId = tables[9].TableId, // T-10
                    UserId = customerUser2.UserId,
                    CustomerName = "Priya Patel",
                    CustomerEmail = "priya@example.com",
                    CustomerPhone = "+91 97234 56789",
                    ReservationDate = DateTime.Today,
                    TimeSlot = "20:00 - 22:00",
                    GuestCount = 5,
                    SpecialRequests = "Family birthday dinner. Please arrange baby chair.",
                    Status = "Confirmed"
                },
                new TableReservation
                {
                    TableId = tables[3].TableId, // T-04
                    CustomerName = "Vikram Aditya",
                    CustomerEmail = "vikram@outlook.com",
                    CustomerPhone = "+91 99887 76655",
                    ReservationDate = DateTime.Today,
                    TimeSlot = "13:00 - 14:30",
                    GuestCount = 4,
                    Status = "Seated"
                },
                new TableReservation
                {
                    TableId = tables[7].TableId, // T-08
                    CustomerName = "Ananya Roy",
                    CustomerEmail = "ananya@gmail.com",
                    CustomerPhone = "+91 98711 22334",
                    ReservationDate = DateTime.Today.AddDays(1),
                    TimeSlot = "19:00 - 20:30",
                    GuestCount = 4,
                    Status = "Confirmed"
                },
                new TableReservation
                {
                    TableId = tables[10].TableId, // T-11
                    CustomerName = "Tech Corp Team",
                    CustomerEmail = "events@techcorp.in",
                    CustomerPhone = "+91 98999 11223",
                    ReservationDate = DateTime.Today.AddDays(2),
                    TimeSlot = "20:00 - 23:00",
                    GuestCount = 8,
                    SpecialRequests = "Executive project milestone celebration. Chef's tasting menu.",
                    Status = "Confirmed"
                }
            };
            context.TableReservations.AddRange(reservations);
            context.SaveChanges();
        }
    }
}
