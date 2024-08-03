using System;
using System.Data;
using System.Net;
using System.Net.Mail;
using System.IO;

namespace sendmail
{
    class Program
    {

        static void Main(string[] args)
        {
            const string Log = @".\log.txt";
            StreamWriter sw = !File.Exists(Log) ? File.CreateText(Log) : new StreamWriter(Log, append: true);
            // MENSAJES DE AYUDA
            var mensajes = new
            {
                ayudaBasica = "\nDebe ingresar una accion.\n" +
                    "Uso: sendmail.exe [/add | /delete | /test | /queryAll | /queryByName | /send]\n\n" +
                    "/add\t\t agrega un registro de parametros para envio de correos.\n" +
                    "/delete\t\t elimina el registro de parametros para envio de correos.\n" +
                    "/test\t\t prueba de envio de correo.\n" +
                    "/queryAll\t muestra todos los registros de envio de correo disponibles.\n" +
                    "/queryByName\t muestra los parametros para envio de correo de un registro especifico.\n" +
                    "/send\t\t accion para envio de correos a varios destinatarios, destinatarios de respuesta y adjuntos.\n",
                envioSatisfactorio = "Correo enviado satisfactoriamente.\n",
                ayudaAdd = "\nAyuda para la accion sendmail.exe /add. \nAgrega un nuevo registro para envio de correos.\n" +
                    "Uso: sendmail.exe /add [smtpName] [smtpHost] [smtpPort] [smtpEncrypt] [smtpFrom] [smtpUser] [smtpPass]\n\n" +
                    "smtpName\t Nombre identificador del registro.\n" +
                    "smtpHost\t Servidor SMTP para envio de correos.\n" +
                    "smtpPort\t Puerto del servidor SMTP.\n" +
                    "smtpEncrypt\t [True | False] Establece si se requiere encriptacion para el servidor SMTP.\n" +
                    "isBodyHtml\t [True | False] Establece si el cuerpo del mensaje es en formato HTML.\n" +
                    "smtpFrom\t Direccion del remitente.\n" +
                    "smtpUser\t Usuario de autenticacion para el servidor SMTP.\n" +
                    "smtpPass\t Contraseña de autenticacion para el servidor SMTP.\n\n" +
                    "Ejemplo: \nsendmail.exe /add prueba smtp.dominioDePruena.test 587 True 'Usuario <usuario@dominioDePruena.test>' asda4HG2342#$\n",
                ayudaDelete = "\nAyuda para la accion sendmail.exe /delete. \nElimina un registro de parametros para envio de correos.\n" +
                    "Uso: sendmail.exe /delete [smtpName]\n\n" +
                    "smtpName\t Identificador del registro de parametros para envio de correos. " +
                    "\n\t\t Utiliza sendmail.exe /queryAll para consultar los registros agregados.\n\n" +
                    "Ejemplo: \nsendmail.exe /delete prueba\n",
                ayudaTest = "\nAyuda para la accion sendmail.exe /test. \nEnvia un correo de prueba.\n" +
                    "Uso: sendmail.exe /test [smtpName] [destinatario]\n\n" +
                    "smtpName\t Identificador del registro de parametros para el envio del correo de prueba. " +
                    "\n\t\t Utiliza sendmail.exe /queryAll para consultar los registros agregados.\n" +
                    "destinatario\t Correo electronico del destinatario para la prueba.\n\n" +
                    "Ejemplo: \nsendmail.exe /test identificador buzonDePrueba@dominio.test \n",
                ayudaSend = "\nAyuda para la accion sendmail.exe /send. \nEnvia correos a varios destinatarios y adjuntos.\n" +
                    "Uso: sendmail.exe /send [smtpName] [destinatario] [respuesta] [remitente] [asunto] [cuerpo] [adjunto: opcional]\n\n" +
                    "smtpName\t Identificador del registro de parametros para el envio del correo de prueba." +
                    "\n\t\t Utiliza sendmail.exe /queryAll para consultar los registros agregados.\n" +
                    "destinatario\t Correo electronico del destinatario. Varios destinatarios son separados con ;\n" +
                    "respuesta\t Direccion de correo para respuesta.\n" +
                    "remitente\t Direccion de correo que se muestra en el remitente.\n" +
                    "asunto\t\t Asunto del mensaje.\n" +
                    "cuerpo\t\t Cuerpo del mensaje.\n" +
                    "adjunto\t\t (Opcional) Ruta del archivo adjunto para el correo" +
                    "\n\t\t Debe encerrar la ruta completa del archivo entre comillas dobles \"." +
                    "\n\t\t Varios archivos adjuntos son separados con ;\n\n" +
                    "Ejemplo: \nsendmail.exe /send identificador buzonDePrueba@dominio.test;buzonDePrueba2@dominio.test buzonDeRespuesta@dominio.test remitente@dominio.test \"Asunto del correo\" \"Cuerpo del mensaje\" \"c:\\ruta\\archivo.ext;d:\\ruta2\\archivo2.ext\"\n",
                ayudaQueryByName = "\nAyuda para la accion sendmail.exe /queryByName. \nMuestra los parametros del registro consultado.\n" +
                    "Uso: sendmail.exe /queryByName [smtpName]\n\n" +
                    "smtpName\t Identificador del registro de parametros para el envio de correos." +
                    "\n\t\t Utiliza sendmail.exe /queryAll para consultar los registros agregados.\n\n" +
                    "Ejemplo: \nsendmail.exe /queryByName prueba\n",
                duplicado = "\nYa existe un registro con el identificador que ingresaste.\n",
                registroNoEncontrado = "\nNo existe registrado el identificador que señalaste.\n"
            };

            // COMPRUEBA SI FUE TECLEADO UNA ACCION DEFINIDA DE LO CONTRARIO
            // SE MUESTRA LA AYUDA INICIAL.
            if (args.Length == 0)
            {
                Console.WriteLine(mensajes.ayudaBasica);
                Environment.Exit(0);
            }

            // METODO PARA ENVIO DE CORREOS
            void sendMail(DataTable smtpServer, string smtpTo, string smtpReplyTo, string smtpRemitente, string subject, string body, string attachment = "")
            {
                var destinatarios = smtpTo.Split(';');
                var adjuntos = attachment.Split(';');
                try
                {
                    SmtpClient smtp = new SmtpClient();
                    smtp.Host = smtpServer.Rows[0]["smtpHost"].ToString();
                    smtp.Port = int.Parse(smtpServer.Rows[0]["smtpPort"].ToString());
                    smtp.EnableSsl = int.Parse(smtpServer.Rows[0]["smtpEncrypt"].ToString()) == 1 ? true : false;
                    smtp.UseDefaultCredentials = false;
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.Credentials = new NetworkCredential(smtpServer.Rows[0]["smtpUser"].ToString(), smtpServer.Rows[0]["smtpPass"].ToString());
                    MailMessage message = new MailMessage();
                    message.From = smtpRemitente == "" ? new MailAddress(smtpServer.Rows[0]["smtpFrom"].ToString()) : new MailAddress(smtpRemitente);
                    foreach(string destinatario in destinatarios)
                    {
                        message.To.Add(new MailAddress(destinatario));
                    }
                    message.ReplyToList.Add(new MailAddress(smtpReplyTo));
                    if (attachment != "")
                    {
                        foreach (string adjunto in adjuntos)
                        {
                            message.Attachments.Add(new Attachment($"{adjunto}"));
                        }
                    }
                    message.Subject = subject;
                    message.IsBodyHtml = int.Parse(smtpServer.Rows[0]["isBodyHtml"].ToString()) == 1 ? true : false; //to make message body as html  
                    message.Body = body;
                    smtp.Send(message);
                    Console.WriteLine(mensajes.envioSatisfactorio);
                    sw.WriteLine(DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss") + "\t" + mensajes.envioSatisfactorio);
                    sw.Close();
                    Environment.Exit(0);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    sw.WriteLine(DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss") + "\t" + ex.Message);
                    sw.Close();
                    Environment.Exit(0);
                }
            }

            // METODO PARA ENVIO DE CORREO DE PRUEBA
            void sendTestMail(DataTable smtpServer, string destinatario)
            {
                try
                {
                    SmtpClient smtp = new SmtpClient();
                    smtp.Host = smtpServer.Rows[0]["smtpHost"].ToString();
                    smtp.Port = int.Parse(smtpServer.Rows[0]["smtpPort"].ToString());                    
                    smtp.EnableSsl = int.Parse(smtpServer.Rows[0]["smtpEncrypt"].ToString()) == 1 ? true : false;
                    smtp.UseDefaultCredentials = false;
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.Credentials = new NetworkCredential(smtpServer.Rows[0]["smtpUser"].ToString(), smtpServer.Rows[0]["smtpPass"].ToString());
                    MailMessage message = new MailMessage();
                    message.From = new MailAddress(smtpServer.Rows[0]["smtpFrom"].ToString());
                    message.To.Add(new MailAddress(destinatario));
                    message.Subject = "Correo de prueba";
                    message.IsBodyHtml = false; //to make message body as html  
                    message.Body = "Si puede ver esto entonces el correo se envio satisfactoriamente.";
                    smtp.Send(message);
                    Console.WriteLine(mensajes.envioSatisfactorio);
                    sw.WriteLine(DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss") + "\t" + mensajes.envioSatisfactorio);
                    sw.Close();
                    Environment.Exit(0);
                }
                catch (Exception ex)
                {
                    //Console.WriteLine(ex);
                    sw.WriteLine(DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss") + "\t" + ex);
                    sw.Close();
                    Environment.Exit(0);
                }
            }

            // ACCION SELECCIONADA DESDE ARGUMENTOS.
            
            switch (args[0])
            { 
                case "/add":
                    // Console.WriteLine("\nSe ha releccionado la accion: /add");
                    if (args.Length < 8)
                    {
                        Console.WriteLine(mensajes.ayudaAdd);
                        sw.WriteLine(DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss") + "\t" + "Error: Faltan argumentos");
                        sw.WriteLine(mensajes.ayudaAdd);
                        sw.Close();
                        Environment.Exit(0);
                    }
                    var duplicado = settingsdb.qSelectByName(args[1]);
                    if (duplicado.Rows.Count > 0)
                    {
                        if (duplicado.Rows[0]["smtpName"].ToString() == args[1])
                        {
                            Console.WriteLine(mensajes.duplicado);
                            sw.WriteLine(DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss") + "\t" + mensajes.duplicado);
                            sw.Close();
                            Environment.Exit(0);
                        }
                    }
                    settingsdb.qInsert(args[1], args[2], int.Parse(args[3]), int.Parse(args[4].ToString()), int.Parse(args[5].ToString()), args[6], args[7], args[8]);
                    sw.Close();
                    Environment.Exit(0);
                    break;
                case "/delete":
                    // Console.WriteLine("\nSe ha releccionado la accion: /delete");
                    if (args.Length < 2)
                    {
                        Console.WriteLine(mensajes.ayudaDelete);
                        sw.WriteLine(DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss") + "\t" + "Error: Faltan argumentos");
                        sw.WriteLine(mensajes.ayudaDelete);
                        sw.Close();
                        Environment.Exit(0);
                    }
                    var registroNoEncontrado = settingsdb.qSelectByName(args[1]);
                    if (registroNoEncontrado.Rows.Count < 1)
                    {
                        Console.WriteLine(mensajes.registroNoEncontrado);
                        sw.WriteLine(DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss") + "\t" + mensajes.registroNoEncontrado);
                        sw.Close();
                        Environment.Exit(0);
                    }
                    settingsdb.qDelete(args[1]);
                    Environment.Exit(0);
                    break;
                case "/send":
                    // Console.WriteLine("\nSe ha releccionado la accion: /test");
                    if (args.Length < 5)
                    {
                        Console.WriteLine(mensajes.ayudaSend);
                        sw.WriteLine(DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss") + "\t" + "Error: Faltan argumentos");
                        sw.WriteLine(mensajes.ayudaSend);
                        sw.Close();
                        Environment.Exit(0);
                    }
                    var smtpParams = settingsdb.qSelectByName((args[1]));
                    if (args.Length < 8)
                    {
                        sendMail(smtpParams, args[2], args[3], args[4], args[5], args[6]);
                    } else
                    {
                        sendMail(smtpParams, args[2], args[3], args[4], args[5], args[6], args[7]);
                    }
                    Environment.Exit(0);
                    break;
                case "/test":
                    // Console.WriteLine("Se ha releccionado la accion: /test");
                    if (args.Length < 2)
                    {
                        Console.WriteLine(mensajes.ayudaTest);
                        sw.WriteLine(DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss") + "\t" + "Error: Faltan argumentos");
                        sw.WriteLine(mensajes.ayudaTest);
                        sw.Close();
                        Environment.Exit(0);
                    }
                    var smtpTest = settingsdb.qSelectByName((args[1]));
                    sendTestMail(smtpTest, args[2]);
                    Environment.Exit(0);
                    break;
                case "/queryAll":
                    var data = settingsdb.qSelect();
                    foreach (DataRow row in data.Rows)
                    {
                        Console.WriteLine($"\nIdentificador: {row["smtpName"].ToString()}");
                        Console.WriteLine($"Servidor SMTP: {row["smtpHost"].ToString()}");
                        Console.WriteLine($"Puerto: {row["smtpPort"].ToString()}");
                        Console.WriteLine($"SSL/TLS: {row["smtpEncrypt"].ToString()}");
                        Console.WriteLine($"BodyHtml: {row["isBodyHtml"].ToString()}");
                        Console.WriteLine($"Remitente: {row["smtpFrom"].ToString()}");
                        Console.WriteLine($"Usuario: {row["smtpUser"].ToString()}\n");
                    }
                    Environment.Exit(0);
                    break;
                case "/queryByName":
                    if (args.Length < 2)
                    {
                        Console.WriteLine(mensajes.ayudaQueryByName);
                        sw.WriteLine(DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss") + "\t" + "Error: Faltan argumentos");
                        sw.WriteLine(mensajes.ayudaQueryByName);
                        sw.Close();
                        Environment.Exit(0);
                    }
                    var registroNoEncontrado2 = settingsdb.qSelectByName(args[1]);
                    if (registroNoEncontrado2.Rows.Count < 1)
                    {
                        Console.WriteLine(mensajes.registroNoEncontrado);
                        sw.WriteLine(DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss") + "\t" + mensajes.registroNoEncontrado);
                        sw.Close();
                        Environment.Exit(0);
                    }
                    var smtpByName = settingsdb.qSelectByName((args[1]));
                    foreach(DataRow row in smtpByName.Rows)
                    {
                        Console.WriteLine($"\nIdentificador: {row["smtpName"].ToString()}");
                        Console.WriteLine($"Servidor SMTP: {row["smtpHost"].ToString()}");
                        Console.WriteLine($"Puerto: {row["smtpPort"].ToString()}");
                        Console.WriteLine($"SSL/TLS: {row["smtpEncrypt"].ToString()}");
                        Console.WriteLine($"BodyHtml: {row["isBodyHtml"].ToString()}");
                        Console.WriteLine($"Remitente: {row["smtpFrom"].ToString()}");
                        Console.WriteLine($"Usuario: {row["smtpUser"].ToString()}\n");
                    }
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine(mensajes.ayudaBasica);
                    sw.WriteLine(DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss") + "\t" + "Ayuda basica.");
                    sw.Write(mensajes.ayudaBasica);
                    sw.Close();
                    Environment.Exit(0);
                    break;
            }
            Console.Read();
        }
    }
}
