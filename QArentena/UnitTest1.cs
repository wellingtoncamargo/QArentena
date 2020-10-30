using Microsoft.VisualBasic.CompilerServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using RestSharp;
using System;

namespace QArentena
{
    public class Tests
    {
        public RestClient client;
        public RestRequest endpoint;
        public IRestResponse resp;
        public string nome, status, com_body, nome_alterado, status_alterado;
        public JArray foto, foto_alterada;
        public RestClient Client(string uri)
        {
            client = new RestClient(uri);
            return client;
        }

        public RestRequest Endpoint(string rota)
        {
            endpoint = new RestRequest(rota);
            return endpoint;
        }

        public void Get()
        {
            endpoint.Method = Method.GET;
            endpoint.RequestFormat = DataFormat.Json;
        }

        public void Post()
        {
            endpoint.Method = Method.POST;
            endpoint.RequestFormat = DataFormat.Json;
        }

        public void Put()
        {
            endpoint.Method = Method.PUT;
            endpoint.RequestFormat = DataFormat.Json;
        }

        public void Delete()
        {
            endpoint.Method = Method.DELETE;
            endpoint.RequestFormat = DataFormat.Json;
        }

        public void Body_json(string _body)
        {
            endpoint.AddParameter("application/json",
                _body,
                ParameterType.RequestBody);
        }


        public IRestResponse StatusCode(int code)
        {
            resp = client.Execute(endpoint);
            if (resp.IsSuccessful)
            {
                var status = (int)resp.StatusCode;
                Assert.AreEqual(code, status);
            }
            else
            {
                var status = (int)resp.StatusCode;
                var desc = resp.StatusDescription;
                var content = resp.Content;

                Console.WriteLine($"{status} - {desc}");
                Console.WriteLine(content);
                Assert.AreEqual(code, status);
            }
            return resp;
        }

        public void ReturnText()
        {
            JObject obs = JObject.Parse(resp.Content);
            Console.WriteLine(obs);
        }

        public string _json()
        {
            var body = @"{
                          ""id"": 12345,
                          ""category"": {
                            ""name"": ""SRD""
                          },
                          ""name"": ""Nina Maria"",
                          ""photoUrls"": [
                            ""file:///C:/Users/wncg/Desktop/QArentena/Nina_Maria1.jpeg""                            
                          ],
                          ""status"": ""Em Adoção""
                        }";
            return body;
        }

        public dynamic Busca_valor(dynamic chave)
        {
            dynamic obj = JProperty.Parse(resp.Content);
            var valor = obj[chave];
            return valor;
        }

        public void Header(string chave, string valor)
        {
            endpoint.AddHeader(chave, valor);
        }

        [Test]
        public void Consulta(string nome, JArray foto, string status)
        {
            Client("https://petstore.swagger.io");
            Endpoint("v2/pet/12345");
            Get();
            StatusCode(200);
            ReturnText();

            string nome_consulta = Busca_valor("name");
            Assert.AreEqual(nome, nome_consulta);

            JArray foto_consulta = Busca_valor("photoUrls");
            Assert.AreEqual(foto, foto_consulta);

            string status_consulta = Busca_valor("status");
            Assert.AreEqual(status, status_consulta);

        }

        public void Consulta_Retirada(int code)
        {
            Client("https://petstore.swagger.io");
            Endpoint("/v2/pet/12345");
            Get();
            StatusCode(code);
            ReturnText();

            string msg = Busca_valor("message");
            Assert.AreEqual(msg, "Pet not found");
        }

        public void Consulta_diferenca(dynamic chave)
        {
            Client("https://petstore.swagger.io");
            Endpoint("/v2/pet/12345");
            Get();
            StatusCode(200);
            ReturnText();

            status_alterado = Busca_valor(chave);
            Assert.AreNotEqual(status, status_alterado);

        }

        [Test]
        public void Retira_Adocao()
        {
            Client("https://petstore.swagger.io");
            Endpoint("/v2/pet/12345");
            Header("api_key", "special-key");
            Delete();
            StatusCode(200);
            ReturnText();
        }

        [Test]
        public void Cadastra()
        {
            Client("https://petstore.swagger.io");
            Endpoint("/v2/pet");
            Post();
            Body_json(_json());
            StatusCode(200);
            ReturnText();

            nome = Busca_valor("name");
            Console.WriteLine(nome);

            foto = Busca_valor("photoUrls");
            Console.WriteLine(foto);

            status = Busca_valor("status");
            Console.WriteLine(status);
        }

        public string Altera_json(string body, string chave, string valor)
        {
            JObject _body = JObject.Parse(body);
            if(chave == "foto")
            {
                _body["photoUrls"][0] = valor;
            }
            _body[chave] = valor;
            string novo_body = JsonConvert.SerializeObject(_body);
            return novo_body;
        }

        [Test]
        public void Altera_Cadastra()
        {
            Client("https://petstore.swagger.io");
            Endpoint("/v2/pet");
            Post();
            com_body = _json();
            com_body = Altera_json(com_body, "status", "Lar temporario");
            com_body = Altera_json(com_body, "foto", "file:///C:/Users/wncg/Desktop/QArentena/Nina_Maria2.jpeg");
            Body_json(com_body);
            StatusCode(200);
            ReturnText();

            nome = Busca_valor("name");
            Console.WriteLine(nome);

            foto = Busca_valor("photoUrls");
            Console.WriteLine(foto);

            status = Busca_valor("status");
            Console.WriteLine(status);
        }

        public void Altera_Cadastra_Adotada()
        {
            Client("https://petstore.swagger.io");
            Endpoint("/v2/pet");
            Post();
            com_body = _json();
            com_body = Altera_json(com_body, "status", "Adotada");
            com_body = Altera_json(com_body, "foto", "file:///C:/Users/wncg/Desktop/QArentena/Nina_Maria3.jpeg");
            Body_json(com_body);
            StatusCode(200);
            ReturnText();

            nome_alterado = Busca_valor("name");
            Console.WriteLine(nome);

            foto_alterada = Busca_valor("photoUrls");
            Console.WriteLine(foto);

            status_alterado = Busca_valor("status");
            Console.WriteLine(status);
        }

        [Test]
        public void Valida_cadastro()
        {
            Cadastra();
            Consulta(nome, foto, status);
        }
        [Test]
        public void Valida_Alteracao()
        {
            Altera_Cadastra();
            Consulta(nome, foto, status);
        }
        [Test]
        public void Valida_Alteracao_Adotada()
        {
            Cadastra();
            Consulta(nome, foto, status);
            Altera_Cadastra_Adotada();
            Consulta_diferenca("status");

        }
        [Test]
        public void Valida_retirada_Adocao()
        {
            Cadastra();
            Consulta(nome, foto, status);
            Altera_Cadastra_Adotada();
            Consulta_diferenca("status");
            Retira_Adocao();
            Consulta_Retirada(404);
        }
    }
}