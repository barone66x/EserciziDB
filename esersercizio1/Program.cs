using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using System.Data.Common;

namespace esersercizio1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("hello");


            //  string connectionString = "Server=localhost\\SQLEXPRESS;Database=orders;Trusted_Connection=True; User ID=sa; Password=sa;";
            var stringa = @"Server=localhost; initial catalog=orders; User ID=sa; Password=sa;";
            // var stringa2 = "data source=.\\SQLEXPRESS; initial catalog=orders; User ID=sa; Password=sa;";

            //SqlConnection conn = new SqlConnection(connectionString);
            SqlConnection conn = new SqlConnection(stringa);

            using (conn) //garantisce chiusura della connessione in ogni modo di uscita
            {
                Console.WriteLine($"connessione creata {conn}");
                conn.Open();
                Console.WriteLine("connessione aperta");
                string q = "select count(*) from orders";
                var cmd = new SqlCommand(q, conn);
                var n = cmd.ExecuteScalar(); //ritorna un singolo risultato
                Console.WriteLine($"ci sono {n} ordini");

                cmd = new SqlCommand("select * from orders", conn); //ritorna una lista di risultati
                using (var orders = cmd.ExecuteReader()) //chiude orders quando si finisce di lavorare, orders è un cursore
                {
                    while (orders.Read())
                    {
                        Console.WriteLine($"{orders["orderid"]}, {orders["customer"]} ");

                    }
                }

                Console.WriteLine("inserisci utente: ");
                string user = Console.ReadLine();
                // string user = "Jack";
                cmd = new SqlCommand($"select * from orders where customer = @user", conn); //inizio query
                //SqlParameter par = new SqlParameter("@user", SqlDbType.VarChar, 50);
                cmd.Parameters.Add(new SqlParameter("@user", user)); //assegno parametro
                //par.Value = user;


                using (var orders = cmd.ExecuteReader()) //viene eseguita la query
                {
                    while (orders.Read())
                    {
                        Console.WriteLine("->{0} {1}", orders["orderid"], orders["customer"]);
                    }
                }
                cmd = new SqlCommand("select * from orderitems", conn);
                using (var ordersitems = cmd.ExecuteReader()) //viene eseguita la query
                {
                    while (ordersitems.Read())
                    {
                        Console.WriteLine($"ordersitems{ordersitems["orderid"]}, {ordersitems["item"]}");
                    }
                }

                ///DML TRANSACTION
                SqlTransaction tr = null;
                try
                {
                    tr = conn.BeginTransaction();
                    Console.WriteLine("DML UPDATE");
                    cmd = new SqlCommand("update orderitems set price = price+100 where orderid=@order", conn, tr);
                    cmd.Parameters.Add(new SqlParameter("@order", 1));
                    Console.WriteLine($"ho modificato {cmd.ExecuteNonQuery()} righe"); //esegue query che non è una select
                    Console.WriteLine("commit");
                    tr.Commit();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("roolback");
                    Console.WriteLine(ex.Message);
                    tr.Rollback();

                }



                ///ADAPTER
                SqlDataAdapter da = new SqlDataAdapter("select * from customers", conn);
                DataSet model = new DataSet();
                da.Fill(model, "customers");
                Console.WriteLine("carico il DataSet dei customer");
                foreach (DataRow c in model.Tables["customers"].Rows)
                {
                    Console.WriteLine($"{c["customer"]}");

                }



            }
            Console.WriteLine("connessione chiusa");


            Console.ReadKey();


        }
    }
}
