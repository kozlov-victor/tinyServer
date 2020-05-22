using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using tinyServer.request;
using tinyServer.response;
using tinyServer.server;

namespace tinyServer.client
{
    // Класс-обработчик клиента
    class Client
    {

        private readonly static string n = "\n";

        // Отправка страницы с ошибкой
        private void SendError(TcpClient client, int code)
        {
            // Получаем строку вида "200 OK"
            // HttpStatusCode хранит в себе все статус-коды HTTP/1.1
            string codeStr = $"{code} {(HttpStatusCode)code}";
            // Код простой HTML-странички
            string html = $"<html><body><h1>{codeStr}</h1></body></html>";
            // Необходимые заголовки: ответ сервера, тип и длина содержимого. После двух пустых строк - само содержимое
            string str = $"HTTP/1.1 {codeStr}{n}Content-type: text/html{n}Content-Length:{html.Length}{n}{n}{html}";
            // Приведем строку к виду массива байт
            byte[] buffer = Encoding.ASCII.GetBytes(str);
            // Отправим его клиенту
            client.GetStream().Write(buffer, 0, buffer.Length);
            // Закроем соединение
            client.Close();
        }

        private void Send(string contentType, Stream sourceStream, TcpClient client)
        {
            // Посылаем заголовки

            string headers = $"HTTP/1.1 200 OK{n}Content-Type: {contentType}{n}Content-Length: {sourceStream.Length}{n}{n}";
            byte[] headersBuffer = Encoding.ASCII.GetBytes(headers);
            client.GetStream().Write(headersBuffer, 0, headersBuffer.Length);
            int count;
            byte[] buffer = new byte[1024];
            // Пока не достигнут конец файла
            while (sourceStream.Position < sourceStream.Length)
            {
                // Читаем данные из файла
                count = sourceStream.Read(buffer, 0, buffer.Length);
                // И передаем их клиенту
                client.GetStream().Write(buffer, 0, count);
            }

            // Закроем файл и соединение
            sourceStream.Close();
            client.Close();
        }

        private void ReadAndSendLocalFile(string requestUri, TcpClient client)
        {
            // Если в строке содержится двоеточие, передадим ошибку 400
            // Это нужно для защиты от URL типа http://example.com/../../file.txt
            if (requestUri.IndexOf("..") >= 0)
            {
                SendError(client, 400);
                return;
            }

            // Если строка запроса оканчивается на "/", то добавим к ней index.html
            if (requestUri.EndsWith("/"))
            {
                requestUri += "index.html";
            }

            string filePath = "./" + requestUri;


            // Если в папке не существует данного файла, посылаем ошибку 404
            if (!File.Exists(filePath))
            {
                SendError(client, 404);
                return;
            }

            // Получаем расширение файла из строки запроса
            string extension = requestUri.Substring(requestUri.LastIndexOf('.'));

            // Тип содержимого
            string contentType;

            // Пытаемся определить тип содержимого по расширению файла
            switch (extension)
            {
                case ".htm":
                case ".html":
                    contentType = "text/html";
                    break;
                case ".css":
                    contentType = "text/stylesheet";
                    break;
                case ".js":
                    contentType = "text/javascript";
                    break;
                case ".jpg":
                    contentType = "image/jpeg";
                    break;
                case ".jpeg":
                case ".png":
                case ".gif":
                    contentType = "image/" + extension.Substring(1);
                    break;
                default:
                    if (extension.Length > 1)
                    {
                        contentType = "application/" + extension.Substring(1);
                    }
                    else
                    {
                        contentType = "application/unknown";
                    }
                    break;
            }

            // Открываем файл, страхуясь на случай ошибки
            Stream sourceStream;
            try
            {
                sourceStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            catch (Exception)
            {
                // Если случилась ошибка, посылаем клиенту ошибку 500
                SendError(client, 500);
                return;
            }
            Send(contentType, sourceStream, client);

        }

        private void ReadAndSendControllerResult(Stream sourceStream, string contentType, TcpClient client)
        {
            Send(contentType, sourceStream, client);
        }

        private string ReadRequest(TcpClient client)
        {
            // Объявим строку, в которой будет хранится запрос клиента
            string request = "";
            // Буфер для хранения принятых от клиента данных
            byte[] buffer = new byte[1024];
            // Переменная для хранения количества байт, принятых от клиента
            int count;
            // Читаем из потока клиента до тех пор, пока от него поступают данные
            while ((count = client.GetStream().Read(buffer, 0, buffer.Length)) > 0)
            {
                // Преобразуем эти данные в строку и добавим ее к переменной Request
                request += Encoding.ASCII.GetString(buffer, 0, count);
                // Запрос должен обрываться последовательностью \r\n\r\n
                // Либо обрываем прием данных сами, если длина строки Request превышает 4 килобайта
                if (request.IndexOf("\r\n\r\n") >= 0 || request.Length > 4096)
                {
                    break;
                }
            }
            return request;
        }

        // Конструктор класса. Ему нужно передавать принятого клиента от TcpListener
        public Client(TcpClient client)
        {

            string requestBody = ReadRequest(client);
            // Парсим строку запроса с использованием регулярных выражений
            // При этом отсекаем все переменные GET-запроса

            // GET / ACTION = TEST HTTP / 1.1
            // Host: localhost: 8080
            // User - Agent: Mozilla / 5.0(Windows; U; Windows NT 5.1; en - GB; rv: 1.9.0.10) Gecko / 2009042316 Firefox / 3.0.10
            // Accept: text / html,application / xhtml + xml,application / xml; q = 0.9,*/*;q=0.8
            // Accept - Language: en-gb,en;q=0.5
            // Accept - Encoding: gzip,deflate
            // Accept - Charset: ISO-8859-1,utf-8;q=0.7,*;q=0.7
            // Keep - Alive: 300
            // Connection: keep-alive


            Match reqMatch = Regex.Match(requestBody, @"^\w+\s+([^\s]+)[^\s]*\s+HTTP/.*|");

            // Если запрос не удался
            if (reqMatch == Match.Empty)
            {
                // Передаем клиенту ошибку 400 - неверный запрос
                SendError(client, 400);
                return;
            }

            // Получаем строку запроса
            Request request = Request.FromRaw(reqMatch.Groups[1].Value);
            
            if (Server.ControllerRegistry.IsUrlRegistered(request.Url))
            {
                Response response = Server.ControllerRegistry.CallMethodByUrl(request);
                ReadAndSendControllerResult(response.Stream, response.ContentType, client);
            }
            else
            {
                ReadAndSendLocalFile(request.Url, client);
            }

        }
    }
}
