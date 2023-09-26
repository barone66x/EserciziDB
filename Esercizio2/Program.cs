using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Esercizio2
{
    internal class Program
    {
        static void Main(string[] args)
        {

            var stringa = @"Server=localhost; initial catalog=orders; User ID=sa; Password=sa;";
            SqlConnection conn = new SqlConnection(stringa);
            int input = 0;




            using (conn) //garantisce chiusura della connessione in ogni modo di uscita
            {
                Console.WriteLine($"connessione creata {conn}");
                conn.Open();
                Console.WriteLine("connessione aperta");
                CheckAdmin(conn);
                if (!CheckUser(conn))
                {
                    return;
                }
                while (input != 5)
                {
                    Console.WriteLine("1 per creare utente");
                    Console.WriteLine("2 per lista ordini");
                    Console.WriteLine("3 per dettaglio ordine");
                    Console.WriteLine("4 per crare ordine");
                    Console.WriteLine("5 per uscire ");
                    input = int.Parse(Console.ReadLine());


                    switch (input)
                    {
                        case 1:
                            if (CreateUser(conn))
                            {
                                Console.WriteLine("utente creato");
                            }
                            else
                            {
                                Console.WriteLine("utente non creato");
                            }
                            break;
                        case 2:
                            ListaOrdini(conn);
                            break;
                        case 3:
                            OrderDetails(conn);
                            break;
                        case 4:
                            if (OrderCreate(conn))
                            {
                                Console.WriteLine("ordine andato a buon fine");
                            }
                            else
                            {
                                Console.WriteLine("ordine fallito");
                            };
                            break;
                        case 5:
                            Console.WriteLine("uscita");
                            Console.ReadKey();
                            return;
                        default:
                            Console.WriteLine("scelta sbagliata");
                            break;
                    }
                }


            }
            Console.ReadLine();
        }




        private static void CheckAdmin(SqlConnection conn)
        {
            var cmd = new SqlCommand("select count(*) from utenti", conn);
            var vuoto = cmd.ExecuteScalar();
            int numero = (int)vuoto;
            if (numero <= 0)
            {
                Console.WriteLine("tabella vuota");
                Console.WriteLine("creo utente admin con password admin");
                cmd = new SqlCommand("insert into utenti Values('admin','admin')", conn);
                cmd.ExecuteReader().Close();
                Console.WriteLine("utente creato");
            }
            else
            {
                Console.WriteLine("tabella non vuota");
            }


        }





        private static bool CheckUser(SqlConnection conn)
        {
            Console.WriteLine("Benvenuto, inserisci username: ");
            string username = Console.ReadLine();

            var cmd = new SqlCommand($"select * from utenti where username = @user", conn);
            cmd.Parameters.Add("@user", username);



            using (var utenti = cmd.ExecuteReader())
            {
                if (utenti.Read())
                {
                    username = (string)utenti["username"];
                    Console.WriteLine("utente trovato");

                }
                else
                {
                    Console.WriteLine("utente non trovato");
                    Console.ReadLine();
                    return false;
                }

            }


            Console.WriteLine("inserisci password");
            var password = Console.ReadLine();
            cmd = new SqlCommand("select * from utenti where username = @user and password = @password", conn);
            cmd.Parameters.Add("@password", password);
            cmd.Parameters.Add("@user", username);
            using (var utente = cmd.ExecuteReader())
            {
                if (utente.Read())
                {
                    Console.WriteLine($"benvenuto {utente["username"]}");
                    return true;
                }
                else
                {
                    Console.WriteLine("password sbagliata");
                    Console.ReadLine();
                    return false;
                }
            }
        }

        private static void ListaOrdini(SqlConnection conn)
        {
            var stringa = "  select o.orderid, customer, orderdate, sum(price*qty) as 'tot speso'" +
                " from orders as o inner join orderitems as i on o.orderid = i.orderid" +
                "  group by o.customer,o.orderid, o.orderdate";

            var cmd = new SqlCommand(stringa, conn);

            using (var orders = cmd.ExecuteReader()) //chiude orders quando si finisce di lavorare, orders è un cursore
            {
                while (orders.Read())
                {
                    Console.WriteLine($"numero ordine:{orders["orderid"]}, {orders["customer"]}, {orders["orderdate"]} {orders["tot speso"]} euro spesi ");
                }
            }
        }


        private static bool CreateUser(SqlConnection conn)
        {
            Console.WriteLine("crea username");
            var user = Console.ReadLine();
            Console.WriteLine("crea password");
            var psw = Console.ReadLine();

            SqlTransaction tr = null; //istanzio oggetto transaction a null
            try
            {
                tr = conn.BeginTransaction(); //inizio la transaction
                Console.WriteLine("INSERIMENTO UTENTE");

                var cmd = new SqlCommand("insert into utenti Values(@user, @psw)", conn, tr);
                cmd.Parameters.Add("@user", user);
                cmd.Parameters.Add("@psw", psw);


                Console.WriteLine($"ho aggiunto {cmd.ExecuteNonQuery()} utente"); //esegue query che non è una select
                tr.Commit(); //Committo la transaction
                return true;
            }
            catch (Exception ex)
            {

                tr.Rollback(); //in caso di errore eseguo rollback della transaction
                return false;

            }




        }
        private static void OrderDetails(SqlConnection conn)
        {
            Console.WriteLine("inserisci numero di orderid: ");
            int id = int.Parse(Console.ReadLine());
            var stringa = "select item, qty, price, vat" +
                "from orderitems where orderid = 1";
            string stringa2 = "select item, qty, price, vat from orderitems where orderid = @id";

            var cmd = new SqlCommand(stringa2, conn);
            cmd.Parameters.Add("@id", id);
            using (var order = cmd.ExecuteReader()) //chiude orders quando si finisce di lavorare, orders è un cursore
            {
                while (order.Read())
                {
                    Console.WriteLine($"nome oggetto: {order["item"]}, quantità: {order["qty"]}, prezzo: {order["price"]}, vat: {order["vat"]}");
                }
            }


        }


        private static bool OrderCreate(SqlConnection conn)
        {
            Console.WriteLine("inserisci il numero dell'utente");
            var cmd = new SqlCommand("select * from customers", conn);
            int input = -1;

            List<string> names = new List<string>();
            List<string> items = new List<string>();

            using (var customers = cmd.ExecuteReader())
            {
                int i = 0;
                while (customers.Read())
                {
                    Console.WriteLine($"numero: {i}   {customers["customer"]}, {customers["country"]}");
                    i += 1;
                    names.Add(customers["customer"].ToString());
                }

            }
            input = CheckInput(input, names);
            // Console.WriteLine("numero valido");
            string name = names[input];







            SqlTransaction tr = null; //istanzio oggetto transaction a null
            try
            {
                tr = conn.BeginTransaction(); //inizio la transaction
                Console.WriteLine("INSERIMENTO ORDINE");
                cmd = new SqlCommand("select MAX(orderid) from orders", conn, tr);
                int neworderid = (int)cmd.ExecuteScalar();
                neworderid += 1;

                cmd = new SqlCommand("insert into orders values (@neworderid, @customer,@date)", conn, tr);
                cmd.Parameters.Clear();
                cmd.Parameters.Add("@neworderid", neworderid);
                cmd.Parameters.Add("@customer", name);
                cmd.Parameters.Add("@date", DateTime.Now);
                cmd.ExecuteNonQuery();
                // Console.WriteLine("nuovo ordine correttamente creato");
                string scelta = "";
                while (scelta != "e")
                {
                    Console.WriteLine("premi un tasto per inserire un oggetto, e per terminare l'ordine");
                    scelta = Console.ReadLine();

                    cmd = new SqlCommand("select * from items", conn, tr);
                    using (var itemss = cmd.ExecuteReader())
                    {
                        int i = 0;
                        while (itemss.Read())
                        {
                            Console.WriteLine($"numero: {i}   {itemss["item"]}, {itemss["color"]}");
                            i += 1;
                            items.Add(itemss["item"].ToString());

                        }

                    }
                    input = CheckInput(input, items);
                    string item = items[input];
                    Console.WriteLine("inserisci prezzo singolo prodotto");
                    int price = int.Parse(Console.ReadLine());
                    int qty = -1;
                    qty = CheckQty(qty);
                    cmd = new SqlCommand("insert into orderitems values(@orderid,@item,@qty,@qty*@price)", conn, tr);
                    cmd.Parameters.Clear();

                    cmd.Parameters.Add("@orderid", neworderid);
                    cmd.Parameters.Add("@item", item);
                    cmd.Parameters.Add("@price", price);
                    cmd.Parameters.Add("@qty", qty);
                    cmd.ExecuteNonQuery();
                    Console.WriteLine($"singolo prodotto inserito nell'ordine {neworderid}");

                }
                tr.Commit();
                return true;
            }
            catch (Exception ex)
            {
                //Console.WriteLine("inserimento ordine fallito");
                tr.Rollback(); //in caso di errore eseguo rollback della transaction
                return false;

            }


        }

        private static int CheckInput(int input, List<string> lista)
        {
            while (input < 0 | input > lista.Count - 1)
            {
                try
                {
                    input = int.Parse(Console.ReadLine());
                }
                catch (Exception ex)
                {
                    Console.WriteLine("inserisci un numero");
                    input = -1;
                }
            }

            Console.WriteLine("selezione valida");
            return input;
        }


        private static int CheckQty(int qty)
        {
            while (qty <= 0)
            {
                try
                {
                    qty = int.Parse(Console.ReadLine());
                }
                catch (Exception ex)
                {
                    Console.WriteLine("inserisci un numero");
                    qty = 0;
                }

            }
            Console.WriteLine("quantià valida");
            return qty;
        }
    }
}

